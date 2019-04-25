using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DemosWPF
{
    /// <summary>
    /// Interaction logic for VideoOverlayWindow.xaml
    /// </summary>
    public partial class VideoOverlayWindow : Window
    {
        public VideoOverlayWindow(string accessToken)
        {
            InitializeComponent();
            Map.AccessToken = accessToken;
        }

        private void Map_Ready(object sender, EventArgs e)
        {

            // Making a simple style that shows satellite view
            // with video playing at specific coordinates

            // Converted using the JSON to C# anonymous type converter
            // source: https://docs.mapbox.com/mapbox-gl-js/example/video-on-a-map/
            // converter: https://jsfiddle.net/aliashrafx/c7pxomjb/39/

            var videoStyle = new Dictionary<string, object>
            {
                ["version"] = 8,
                ["sources"] = new Dictionary<string, object>
                {
                    ["satellite"] = new Dictionary<string, object>
                    {
                        ["type"] = "raster",
                        ["url"] = "mapbox://mapbox.satellite",
                        ["tileSize"] = 512,
                    },
                    ["video"] = new Dictionary<string, object>
                    {
                        ["type"] = "video",
                        ["urls"] = new object[] {
                            "https://static-assets.mapbox.com/mapbox-gl-js/drone.mp4",
                            "https://static-assets.mapbox.com/mapbox-gl-js/drone.webm",
                        },
                        ["coordinates"] = new object[] {
                            new object[] {
                                -122.51596391201019, 37.56238816766053,
                            },
                            new object[] {
                                -122.51467645168304, 37.56410183312965,
                            },
                            new object[] {
                                -122.51309394836426, 37.563391708549425,
                            },
                            new object[] {
                                -122.51423120498657, 37.56161849366671,
                            },
                        },
                    },
                },
                ["layers"] = new object[] {
                    new Dictionary<string, object> {
                        ["id"] = "background",
                        ["type"] = "background",
                        ["paint"] = new Dictionary<string, object> {
                            ["background-color"] = "rgb(4,7,14)",
                        },
                    },
                    new Dictionary<string, object> {
                        ["id"] = "satellite",
                        ["type"] = "raster",
                        ["source"] = "satellite",
                    },
                    new Dictionary<string, object> {
                        ["id"] = "video",
                        ["type"] = "raster",
                        ["source"] = "video",
                    },
                },
            };

            Map.Invoke.SetStyle(videoStyle);
        }
    }
}
