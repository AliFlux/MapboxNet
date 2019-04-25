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
using MapboxNetCore;

namespace DemosWPF
{
    /// <summary>
    /// Interaction logic for LocalRasterWindow.xaml
    /// </summary>
    public partial class LocalRasterWindow : Window
    {
        public LocalRasterWindow()
        {
            InitializeComponent();
            
            // Starting a tile server on a random port

            var server = new MapboxNetCore.MapServer(@"tiles\zurich-raster.mbtiles");
            server.GlyphsPath = @"fonts\";
            server.Start();

            // Creating a custom style that takes our MapServer tiles as a raster source

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
                            server.TilesURL, // using tile URL here
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
                ["glyphs"] = server.GlyphsURL,
                ["id"] = "14004506-1129-4d81-9889-800854993041",
            };
            
            // You can create a hybrid satellite view as well by using a style that takes both Vector Tiles as Raster Tiles

            Map.MapStyle = style;
        }
    }
}
