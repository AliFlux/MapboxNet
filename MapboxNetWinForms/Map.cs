using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp.WinForms;
using CefSharp;
using System.IO;
using Newtonsoft.Json;
using MapboxNetCore;
using System.Drawing.Imaging;

namespace MapboxNetWinForms
{
    public partial class Map: UserControl, IMap
    {
        public event EventHandler Ready;
        public event EventHandler Styled;
        public event EventHandler Render;
        public event EventHandler CenterChanged;
        public event EventHandler ZoomChanged;
        public event EventHandler PitchChanged;
        public event EventHandler BearingChanged;
        public event EventHandler Reloading;


        string _accessToken;
        public string AccessToken
        {
            get => _accessToken;

            set {
                _accessToken = value;
                updateAccessToken();
            }
        }

        void updateAccessToken()
        {
            initializeMap();
        }


        object _mapStyle = "mapbox://styles/mapbox/streets-v11";
        public object MapStyle
        {
            get => _mapStyle;

            set
            {
                _mapStyle = value;
                updateMapStyle();
            }
        }

        void updateMapStyle()
        {
            initializeMap();
        }

        bool _removeAttribution;
        public bool RemoveAttribution
        {
            get => _removeAttribution;

            set
            {
                _removeAttribution = value;
                updateAttribution();
            }
        }

        void updateAttribution()
        {
            if (IsReady && !_supressChangeEvents)
                if (RemoveAttribution && !_supressChangeEvents)
                    SoftExecute("map.getContainer().classList.add('no-attrib');");
                else
                    SoftExecute("map.getContainer().classList.remove('no-attrib');");
        }

        GeoLocation _center = new GeoLocation();
        public GeoLocation Center
        {
            get => _center;

            set
            {
                _center = value;
                updateCenter();
            }
        }

        void updateCenter()
        {
            if (IsReady && !_supressChangeEvents)
                SoftInvoke.SetCenter(new { lon = Center.Longitude, lat = Center.Latitude });
        }

        double _zoom;
        public double Zoom
        {
            get => _zoom;

            set
            {
                _zoom = value;
                updateZoom();
            }
        }

        void updateZoom()
        {
            if (IsReady && !_supressChangeEvents)
                SoftInvoke.SetZoom(Zoom);
        }

        double _pitch;
        public double Pitch
        {
            get => _pitch;

            set
            {
                _pitch = value;
                updatePitch();
            }
        }

        void updatePitch()
        {
            if (IsReady && !_supressChangeEvents)
                SoftInvoke.SetPitch(Pitch);
        }

        double _bearing;
        public double Bearing
        {
            get => _bearing;

            set
            {
                _bearing = value;
                updateBearing();
            }
        }

        void updateBearing()
        {
            if (IsReady && !_supressChangeEvents)
                SoftInvoke.SetBearing(Bearing);
        }

        bool _supressChangeEvents = false;
        bool _arePropertiesUpdated = false;

        bool _isReady;
        public bool IsReady
        {
            get => _isReady;
            set => _isReady = value;
        }

        public dynamic Invoke
        {
            get
            {
                var expressionBuilder = new ExpressionBuilder("map");
                expressionBuilder.Execute = Execute;
                expressionBuilder.TransformToken = Core.ToLowerCamelCase;
                return expressionBuilder;
            }
        }

        public dynamic SoftInvoke
        {
            get
            {
                var expressionBuilder = new ExpressionBuilder("map");
                expressionBuilder.Execute = SoftExecute;
                expressionBuilder.TransformToken = Core.ToLowerCamelCase;
                return expressionBuilder;
            }
        }

        //List<ExpressionBuilder> lazyExpressions = new List<ExpressionBuilder>();

        public dynamic LazyInvoke
        {
            get
            {
                var expressionBuilder = new ExpressionBuilder("map");
                expressionBuilder.Execute = Execute;
                expressionBuilder.TransformToken = Core.ToLowerCamelCase;
                expressionBuilder.ExecuteKey = "Eval";
                //lazyExpressions.Add(expressionBuilder);
                return expressionBuilder;
            }
        }

        CefSharp.WinForms.ChromiumWebBrowser webView;

        public Map()
        {
            InitializeComponent();

            if (!Cef.IsInitialized)
            {
                CefSettings settings = new CefSettings();
                //settings.CefCommandLineArgs.Add("disable-surfaces", "1");
                //settings.CefCommandLineArgs.Add("disable-gpu-compositing", "1");
                //settings.CefCommandLineArgs.Add("enable-begin-frame-scheduling", "1");

                Cef.Initialize(settings);
            }
        }

        void initializeMap()
        {
            if (webView != null)
            {
                this.Controls.Remove(webView);
                webView = null;
                Reloading?.Invoke(this, null);
            }

            BrowserSettings browserSettings = new BrowserSettings();
            //browserSettings.WindowlessFrameRate = 60;

            webView = new ChromiumWebBrowser();
            webView.IsBrowserInitializedChanged += WebView_IsBrowserInitializedChanged;
            webView.FrameLoadEnd += WebView_FrameLoadEnd;
            webView.BrowserSettings = browserSettings;
            this.Controls.Add(webView);
            webView.Dock = DockStyle.Fill;
        }

        private void WebView_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
        }

        private void WebView_IsBrowserInitializedChanged(object sender, CefSharp.IsBrowserInitializedChangedEventArgs e)
        {
            if (webView.IsBrowserInitialized)
            {
                //webView.ShowDevTools();
                var script = Core.GetFrameScript(AccessToken, MapStyle);
                webView.LoadHtml(script, "http://MapboxNet/");
                webView.JavascriptObjectRepository.Register("relay", new Relay(notify, this), true);
            }
        }


        void ready()
        {
            IsReady = true;
            updateCenter();
            updateZoom();
            updatePitch();
            updateBearing();
            updateAttribution();
            _arePropertiesUpdated = true;
            Ready?.Invoke(this, null);
            return;

        }

        void notify(string json)
        {
            dynamic data = Core.DecodeJsonPlain(json);

            if (data.type == "ready")
            {
                ready();
                Render?.Invoke(this, null);
            }
            else if (data.type == "load")
            {
                Styled?.Invoke(this, null);
            }


            if (!_arePropertiesUpdated)
                return;

            if (data.type == "move")
            {
                CenterChanged?.Invoke(this, null);
                Render?.Invoke(this, null);

                _supressChangeEvents = true;
                Center = new GeoLocation(data.center.lat, data.center.lng);
                _supressChangeEvents = false;
            }
            else if (data.type == "zoom")
            {
                ZoomChanged?.Invoke(this, null);
                Render?.Invoke(this, null);

                _supressChangeEvents = true;
                Zoom = data.zoom;
                _supressChangeEvents = false;
            }
            else if (data.type == "pitch")
            {
                PitchChanged?.Invoke(this, null);
                Render?.Invoke(this, null);

                _supressChangeEvents = true;
                Pitch = data.pitch;
                _supressChangeEvents = false;
            }
            else if (data.type == "bearing")
            {
                BearingChanged?.Invoke(this, null);
                Render?.Invoke(this, null);

                _supressChangeEvents = true;
                Bearing = data.bearing;
                _supressChangeEvents = false;
            }
            else if (data.type == "error")
            {

            }
        }

        public object SoftExecute(string expression)
        {
            try
            {
                return Execute(expression);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public object Execute(string expression)
        {
            try
            {

                var task = webView.EvaluateScriptAsync("exec", new object[] { expression });
                task.Wait();

                object result = null;
                JavascriptResponse response = task.Result;
                if (!task.IsFaulted && response.Success)
                {
                    result = response.Result;
                }
                else
                {
                    throw new Exception(response.Message);
                }

                try
                {
                    var obj = Core.DecodeJsonPlain(result.ToString());
                    return obj;
                }
                catch (Exception e)
                {
                    // TODO lodge exception when using ToString() on the result in certain cases
                    return null;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<object> ExecuteAsync(string expression)
        {
            try
            {
                var result = await webView.EvaluateScriptAsync("exec", new object[] { expression });
                return Core.DecodeJsonPlain(result.Result.ToString());
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //public void ExecuteLazy()
        //{
        //    try
        //    {
        //        var statements = lazyExpressions.Where(e => !e.Consumed).Select(e => e.Expression + ";");
        //        var code = string.Join("", statements);
        //        webView.InvokeScript("run", new string[] { code });
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}

        //public async Task ExecuteLazyAsync()
        //{
        //    var statements = lazyExpressions.Where(e => !e.Consumed).Select(e => e.Expression);
        //    var code = string.Join("\n\n", statements);
        //    await ExecuteAsync(code);
        //}

        public void AddImage(string id, System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream bmp = new MemoryStream())
            {
                bitmap.Save(bmp, ImageFormat.Png);
                bmp.Position = 0;
                AddImage(id, bmp);
            }
        }

        public void AddImage(string id, MemoryStream stream)
        {
            AddImage(id, Convert.ToBase64String(stream.GetBuffer()));
        }

        public void AddImage(string id, string base64)
        {
            var code = "addImage(" + JsonConvert.SerializeObject(id) + ", " + JsonConvert.SerializeObject(base64) + ");";
            Execute(code);
        }

        public Point2D Project(GeoLocation location)
        {
            var pointOnScreen = Invoke.Project(new[] { location.Longitude, location.Latitude });
            return new Point2D((double)pointOnScreen.x, (double)pointOnScreen.y);
        }

        public GeoLocation UnProject(Point2D point)
        {
            var location = Invoke.Unproject(new[] { point.X, point.Y });
            return new GeoLocation((double)location.lat, (double)location.lng);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyRaised(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
