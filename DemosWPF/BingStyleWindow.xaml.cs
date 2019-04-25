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
    /// Interaction logic for BingStyleWindow.xaml
    /// </summary>
    public partial class BingStyleWindow : Window
    {
        public BingStyleWindow()
        {
            InitializeComponent();

            // Creating a custom style that takes bing satellite tile source as the raster layer

            var style = new Dictionary<string, object>
            {
                ["version"] = 8,
                ["name"] = "Custom style",
                ["sources"] = new Dictionary<string, object>
                {
                    ["satellite"] = new Dictionary<string, object>
                    {
                        ["type"] = "raster",
                        ["tileSize"] = 256,
                        ["tiles"] = new object[] {
                            "http://ecn.t0.tiles.virtualearth.net/tiles/h{quadkey}.jpeg?g=129&mkt=en&stl=H",
                        },
                    },
                },
                ["layers"] = new object[] {
                    new Dictionary<string, object> {
                        ["id"] = "satellite",
                        ["type"] = "raster",
                        ["source"] = "satellite",
                        ["minzoom"] = 0,
                        ["maxzoom"] = 22,
                        ["layout"] = new Dictionary<string, object> {
                        },
                        ["paint"] = new Dictionary<string, object> {
                        },
                    },
                },
                ["glyphs"] = "http://glfonts.lukasmartinelli.ch/fonts/{fontstack}/{range}.pbf",
                ["id"] = "44004506-1129-4d81-9889-800854993041",
            };

            Map.MapStyle = style;
        }
    }
}
