using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MapboxNetCore
{
    public static class Core
    {
        static string getEmbeddedResource(string name)
        {
            return GetEmbeddedResource(typeof(Core), nameof(MapboxNetCore) + "." + name);
        }

        public static string GetEmbeddedResource(Type assemblyType, string resourceName)
        {
            var assembly = assemblyType.GetTypeInfo().Assembly;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static string GetFrameScript(string accessToken, object style)
        {
            var mainScript = getEmbeddedResource("web.frame.js");
            var glScript = getEmbeddedResource("web.mapbox-gl.js");
            var css = getEmbeddedResource("web.mapbox-gl.css");

            var result = mainScript
                .Replace("{{mapbox-gl.js}}", glScript)
                .Replace("{{mapbox-gl.css}}", css)
                .Replace("{{style}}", JsonConvert.SerializeObject(style))
                .Replace("{{accessToken}}", JsonConvert.SerializeObject(accessToken));

            return result;
        }

        public static string ToLowerCamelCase(string s)
        {
            return Char.ToLowerInvariant(s[0]) + s.Substring(1);
        }

        public static string ToUpperCamelCase(string s)
        {
            return Char.ToUpperInvariant(s[0]) + s.Substring(1);
        }

        static object plainifyJson(JToken token)
        {
            if (token.Type == JTokenType.Object)
            {
                IDictionary<string, JToken> dict = token as JObject;
                
                dynamic expandos = dict.Aggregate(new EnhancedExpandoObeject() as IDictionary<string, Object>,
                            (a, p) => { a.Add(p.Key, plainifyJson(p.Value)); return a; });

                return expandos;
            }
            else if (token.Type == JTokenType.Array)
            {
                var array = token as JArray;
                return array.Select(item => plainifyJson(item)).ToList();
            }
            else
            {
                return token.ToObject<object>();
            }
        }

        public static object DecodeJsonPlain(string json)
        {
            JsonSerializerSettings config = new JsonSerializerSettings {
                FloatParseHandling = FloatParseHandling.Double
            };

            if (json.Trim() == "")
                json = "null";

            dynamic data = JsonConvert.DeserializeObject("{a:" + json + "}");
            var deserialized = data.a as JToken;
            return plainifyJson(deserialized);
        }
    }
}
