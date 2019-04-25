using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CefSharp;
using CefSharp.Wpf;
using MapboxNetCore;

namespace MapboxNetWPF
{
    /// <summary>
    /// Interaction logic for Map.xaml
    /// </summary>
    public partial class Map : UserControl, IMap
    {
        public event EventHandler Ready;
        public event EventHandler Styled;
        public event EventHandler Render;
        public event EventHandler CenterChanged;
        public event EventHandler ZoomChanged;
        public event EventHandler PitchChanged;
        public event EventHandler BearingChanged;
        public event EventHandler Reloading;


        public string AccessToken
        {
            get => (string)GetValue(AccessTokenProperty);
            set => SetValue(AccessTokenProperty, value);
        }

        public static readonly DependencyProperty AccessTokenProperty = DependencyProperty.Register(nameof(AccessToken), typeof(string), typeof(Map), new PropertyMetadata("", updateAccessToken));

        static void updateAccessToken(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var map = obj as Map;
            map.initializeMap();
        }


        public object MapStyle
        {
            get => GetValue(MapStyleProperty);
            set => SetValue(MapStyleProperty, value);
        }

        public static readonly DependencyProperty MapStyleProperty = DependencyProperty.Register(nameof(MapStyle), typeof(object), typeof(Map), new PropertyMetadata("mapbox://styles/mapbox/streets-v11", updateMapStyle));

        static void updateMapStyle(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var map = obj as Map;
            map.initializeMap();

        }

        public bool RemoveAttribution
        {
            get => (bool)GetValue(RemoveAttributionProperty);
            set => SetValue(RemoveAttributionProperty, value);
        }

        public static readonly DependencyProperty RemoveAttributionProperty = DependencyProperty.Register(nameof(RemoveAttribution), typeof(bool), typeof(Map), new PropertyMetadata(false, updateAttribution));

        static void updateAttribution(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var map = obj as Map;
            if (map.IsReady && !map._supressChangeEvents)
                if (map.RemoveAttribution && !map._supressChangeEvents)
                    map.SoftExecute("map.getContainer().classList.add('no-attrib');");
                else
                    map.SoftExecute("map.getContainer().classList.remove('no-attrib');");
        }

        public GeoLocation Center
        {
            get => (GeoLocation)GetValue(CenterProperty);
            set => SetValue(CenterProperty, value);
        }

        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(nameof(Center), typeof(GeoLocation), typeof(Map), new PropertyMetadata(new GeoLocation(), new PropertyChangedCallback(updateCenter)));

        static void updateCenter(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var map = obj as Map;
            if (map.IsReady && !map._supressChangeEvents)
                map.SoftInvoke.SetCenter(new { lon = map.Center.Longitude, lat = map.Center.Latitude });
        }
        
        public double Zoom
        {
            get => (double)GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(nameof(Zoom), typeof(double), typeof(Map), new PropertyMetadata((double)0, new PropertyChangedCallback(updateZoom)));
        
        static void updateZoom(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var map = obj as Map;
            if (map.IsReady && !map._supressChangeEvents)
                map.SoftInvoke.SetZoom(map.Zoom);
        }
        
        public double Pitch
        {
            get => (double)GetValue(PitchProperty);
            set => SetValue(PitchProperty, value);
        }

        public static readonly DependencyProperty PitchProperty = DependencyProperty.Register(nameof(Pitch), typeof(double), typeof(Map), new PropertyMetadata((double)0, new PropertyChangedCallback(updatePitch)));

        static void updatePitch(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var map = obj as Map;
            if (map.IsReady && !map._supressChangeEvents)
                map.SoftInvoke.SetPitch(map.Pitch);
        }
        
        public double Bearing
        {
            get => (double)GetValue(BearingProperty);
            set => SetValue(BearingProperty, value);
        }

        public static readonly DependencyProperty BearingProperty = DependencyProperty.Register(nameof(Bearing), typeof(double), typeof(Map), new PropertyMetadata((double)0, new PropertyChangedCallback(updateBearing)));


        static void updateBearing(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var map = obj as Map;
            if (map.IsReady && !map._supressChangeEvents)
                map.SoftInvoke.SetBearing(map.Bearing);
        }

        bool _supressChangeEvents = false;
        bool _arePropertiesUpdated = false;
        
        public bool IsReady
        {
            get => (bool)GetValue(IsReadyProperty);
            private set => SetValue(IsReadyProperty, value);
        }

        public static readonly DependencyProperty IsReadyProperty = DependencyProperty.Register(nameof(IsReady), typeof(bool), typeof(Map), new PropertyMetadata(false));

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

        CefSharp.Wpf.ChromiumWebBrowser webView;

        public Map()
        {
            InitializeComponent();

            if(!Cef.IsInitialized)
            {
                CefSettings settings = new CefSettings();
                settings.CefCommandLineArgs.Add("disable-surfaces", "1");
                settings.CefCommandLineArgs.Add("disable-gpu-compositing", "1");
                settings.CefCommandLineArgs.Add("enable-begin-frame-scheduling", "1");
                //settings.SetOffScreenRenderingBestPerformanceArgs();

                Cef.Initialize(settings);
            }
        }

        void initializeMap()
        {
            if (webView != null)
            {
                MainGrid.Children.Remove(webView);
                webView = null;
                Reloading?.Invoke(this, null);
            }

            BrowserSettings browserSettings = new BrowserSettings();
            browserSettings.WindowlessFrameRate = 60;

            webView = new CefSharp.Wpf.ChromiumWebBrowser();
            webView.IsBrowserInitializedChanged += WebView_IsBrowserInitializedChanged;
            webView.BrowserSettings = browserSettings;
            MainGrid.Children.Add(webView);
        }

        private void WebView_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (webView.IsBrowserInitialized)
            {
                //webView.ShowDevTools();
                var script = Core.GetFrameScript(AccessToken, MapStyle);
                webView.LoadHtml(script, "http://MapboxNet/");
                webView.JavascriptObjectRepository.Register("relay", new Relay(notify, this.Dispatcher), true);
            }
        }


        void ready()
        {
            IsReady = true;
            updateCenter(this, new DependencyPropertyChangedEventArgs());
            updateZoom(this, new DependencyPropertyChangedEventArgs());
            updatePitch(this, new DependencyPropertyChangedEventArgs());
            updateBearing(this, new DependencyPropertyChangedEventArgs());
            updateAttribution(this, new DependencyPropertyChangedEventArgs());
            _arePropertiesUpdated = true;
            Ready?.Invoke(this, null);
            return;

        }

        private void WebView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

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
            //else if (data.type == "mouseDown")
            //{
            //    RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            //    {
            //        RoutedEvent = Mouse.MouseDownEvent,
            //        Source = this,
            //    });
            //}
            //else if (data.type == "mouseMove")
            //{
            //    RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
            //    {
            //        RoutedEvent = Mouse.MouseMoveEvent,
            //        Source = this,
            //    });
            //}
            //else if (data.type == "mouseUp")
            //{
            //    RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            //    {
            //        RoutedEvent = Mouse.MouseUpEvent,
            //        Source = this,
            //    });
            //}
            //else if (data.type == "mouseEnter")
            //{
            //    RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
            //    {
            //        RoutedEvent = Mouse.MouseEnterEvent,
            //        Source = this,
            //    });
            //}
            //else if (data.type == "mouseLeave")
            //{
            //    RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
            //    {
            //        RoutedEvent = Mouse.MouseLeaveEvent,
            //        Source = this,
            //    });
            //}
            //else if (data.type == "doubleClick")
            //{

            //}
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
                } catch(Exception e)
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
            } catch(Exception e)
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

        public void AddImage(string id, BitmapSource bitmapSource)
        {
            using (MemoryStream bmp = new MemoryStream())
            {
                var pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                pngEncoder.Save(bmp);
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
