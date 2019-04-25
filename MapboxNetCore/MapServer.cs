using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using uhttpsharp;
using uhttpsharp.Listeners;
using uhttpsharp.RequestProviders;

namespace MapboxNetCore
{
    public class MapServer
    {
        public int MinZoom { get; private set; }
        public int MaxZoom { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string MBTilesVersion { get; private set; }
        public GeoExtent Bounds { get; private set; }
        public GeoLocation Center { get; private set; }
        public IPEndPoint EndPoint { get; private set; }

        public string TilesPath { get; private set; }

        public string TilesURL
        {
            get
            {
                return "http://" + EndPoint.Address + ":" + EndPoint.Port + "/tiles/{z}/{x}/{y}";
            }
        }

        public string GlyphsPath { get; set; }

        public string GlyphsURL
        {
            get
            {
                return "http://" + EndPoint.Address + ":" + EndPoint.Port + "/glyphs/{fontstack}/{range}.pbf";
            }
        }

        Dictionary<string, string> corsHeaders = new Dictionary<string, string>()
        {
            ["Access-Control-Allow-Origin"] = "*",
            ["Access-Control-Allow-Headers"] = "Origin, X-Requested-With, Content-Type, Accept",
        };


        enum SourceType
        {
            Mbtiles,
            Files,
        }

        SourceType sourceType;

        public MapServer(string path)
        {
            TilesPath = path;

            if(path.ToLowerInvariant().EndsWith(".mbtiles"))
            {
                sourceType = SourceType.Mbtiles;
                loadMbTiles();
            } else
            {
                sourceType = SourceType.Files;
            }
        }

        void loadMbTiles()
        {
            // MbTiles loading code in GIST by geobabbler
            // https://gist.github.com/geobabbler/9213392

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(String.Format("Data Source={0};Version=3;", this.TilesPath)))
                {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand() { Connection = conn, CommandText = "SELECT * FROM metadata;" })
                    {
                        SQLiteDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            string name = reader["name"].ToString();
                            switch (name.ToLower())
                            {
                                case "bounds":
                                    string val = reader["value"].ToString();
                                    string[] vals = val.Split(new char[] { ',' });
                                    this.Bounds = new GeoExtent() { West = Convert.ToDouble(vals[0]), South = Convert.ToDouble(vals[1]), East = Convert.ToDouble(vals[2]), North = Convert.ToDouble(vals[3]) };
                                    break;
                                case "center":
                                    val = reader["value"].ToString();
                                    vals = val.Split(new char[] { ',' });
                                    this.Center = new GeoLocation(Convert.ToDouble(vals[1]), Convert.ToDouble(vals[0]));
                                    break;
                                case "minzoom":
                                    this.MinZoom = Convert.ToInt32(reader["value"]);
                                    break;
                                case "maxzoom":
                                    this.MaxZoom = Convert.ToInt32(reader["value"]);
                                    break;
                                case "name":
                                    this.Name = reader["value"].ToString();
                                    break;
                                case "description":
                                    this.Description = reader["value"].ToString();
                                    break;
                                case "version":
                                    this.MBTilesVersion = reader["value"].ToString();
                                    break;

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new MemberAccessException("Could not load Mbtiles source file");
            }
        }

        Stream getMbtile(int x, int y, int zoom)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(String.Format("Data Source={0};Version=3;", TilesPath)))
                {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand() { Connection = conn, CommandText = String.Format("SELECT * FROM tiles WHERE tile_column = {0} and tile_row = {1} and zoom_level = {2}", x, y, zoom) })
                    {
                        SQLiteDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            var stream = reader.GetStream(reader.GetOrdinal("tile_data"));
                            return stream;
                        }
                    }
                }
            }
            catch
            {
                throw new MemberAccessException("Could not load tile from Mbtiles");
            }

            return null;
        }

        Stream getFileTile(int x, int y, int zoom)
        {
            var qualifiedPath = TilesPath
                .Replace("{x}", x.ToString())
                .Replace("{y}", y.ToString())
                .Replace("{z}", zoom.ToString());

            return File.Open(qualifiedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public Stream GetTile(int x, int y, int zoom)
        {
            if(sourceType == SourceType.Mbtiles)
            {
                return getMbtile(x, y, zoom);
            }
            else if(sourceType == SourceType.Files)
            {
                return getFileTile(x, y, zoom);
            }

            return null;
        }

        bool isGZipped(Stream stream)
        {
            return isZipped(stream, 3, "1F-8B-08");
        }

        bool isZipped(Stream stream, int signatureSize = 4, string expectedSignature = "50-4B-03-04")
        {
            if (stream.Length < signatureSize)
                return false;
            byte[] signature = new byte[signatureSize];
            int bytesRequired = signatureSize;
            int index = 0;
            while (bytesRequired > 0)
            {
                int bytesRead = stream.Read(signature, index, bytesRequired);
                bytesRequired -= bytesRead;
                index += bytesRead;
            }
            stream.Seek(0, SeekOrigin.Begin);
            string actualSignature = BitConverter.ToString(signature);
            if (actualSignature == expectedSignature) return true;
            return false;
        }

        private Stream unzipStream(Stream stream)
        {
            if (isGZipped(stream))
            {
                using (var zipStream = new GZipStream(stream, CompressionMode.Decompress))
                {
                    var resultStream = new MemoryStream();
                    zipStream.CopyTo(resultStream);
                    resultStream.Seek(0, SeekOrigin.Begin);
                    stream.Dispose();
                    return resultStream;
                }
            }
            else
            {
                return stream;
            }
        }

        public IPEndPoint Start()
        {
            return Start(IPAddress.Loopback, 0);
        }

        public IPEndPoint Start(IPAddress ipAddress, int port)
        {
            var requestProvider = new HttpRequestProvider();
            var httpServer = new HttpServer(requestProvider);

            httpServer.Use(Serve);

            var listener = new TcpListener(ipAddress, port);
            httpServer.Use(new TcpListenerAdapter(listener));

            httpServer.Start();

            EndPoint = (IPEndPoint)listener.LocalEndpoint;

            return EndPoint;
        }

        Task Serve(IHttpContext context, Func<Task> next)
        {
            string prefix = context.Request.RequestParameters[0];

            if (prefix == "tiles")
            {
                int z = Convert.ToInt32(context.Request.RequestParameters[1]);
                int x = Convert.ToInt32(context.Request.RequestParameters[2]);
                int y = Convert.ToInt32(context.Request.RequestParameters[3]);
                y = (int)(Math.Pow(2, z) - y - 1);

                return ServeTile(context, next, x, y, z);
            }
            else if (prefix == "glyphs")
            {
                var components = context.Request.RequestParameters
                    .Skip(1)
                    .Select(p => Uri.UnescapeDataString(p))
                    .ToArray();
                var path = System.IO.Path.Combine(components);
                return ServeGlyph(context, next, path);
            }
            else
            {
                context.Response = new HttpResponse(HttpResponseCode.NotFound, "404 Not Found.", null, true, null);
                return Task.Factory.GetCompleted();
            }
        }

        bool keepAlive = true;

        Task ServeTile(IHttpContext context, Func<Task> next, int x, int y, int z)
        {
            var tileStream = GetTile(x, y, z);

            if (tileStream != null)
            {
                tileStream = unzipStream(tileStream);
                context.Response = new HttpResponse(HttpResponseCode.Ok, "application/octet-stream", tileStream, keepAlive, corsHeaders);
                // TODO close stream somewhere
            }
            else
                context.Response = new HttpResponse(HttpResponseCode.NotFound, "application/octet-stream", null, keepAlive, corsHeaders);


            return Task.Factory.GetCompleted();
        }

        Task ServeGlyph(IHttpContext context, Func<Task> next, string path)
        {
            var fullPath = System.IO.Path.Combine(GlyphsPath, path);

            if (System.IO.File.Exists(fullPath))
            {
                var glyphStream = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                context.Response = new HttpResponse(HttpResponseCode.Ok, "application/x-protobuf", glyphStream, keepAlive, corsHeaders);
                // TODO close stream somewhere

            }
            else
            {
                context.Response = new HttpResponse(HttpResponseCode.NotFound, "application/octet-stream", null, keepAlive, corsHeaders);
            }


            return Task.Factory.GetCompleted();
        }
    }
}
