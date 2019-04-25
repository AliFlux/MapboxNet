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
    /// Interaction logic for OfflineWindow.xaml
    /// </summary>
    public partial class OfflineWindow : Window
    {
        public OfflineWindow(string accessToken)
        {
            InitializeComponent();

            var style = new Dictionary<string, object>
            {
                ["version"] = 8,
                ["name"] = "Custom style",
                ["sources"] = new Dictionary<string, object>
                {
                    ["satellite-mediumres"] = new Dictionary<string, object>
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
                        ["id"] = "satellite-mediumres",
                        ["type"] = "raster",
                        ["source"] = "satellite-mediumres",
                        ["minzoom"] = 0,
                        ["maxzoom"] = 22,
                        ["layout"] = new Dictionary<string, object> {
                        },
                        ["paint"] = new Dictionary<string, object> {
                        },
                    },
                },
                ["glyphs"] = "https://maps.tilehosting.com/fonts/{fontstack}/{range}.pbf?key=yd4rAVOD6ZdfBCcbKnIE",
                ["id"] = "44004506-1129-4d81-9889-800854993041",
            };

            Map.MapStyle = style;
        }

        private void Map_Ready(object sender, EventArgs e)
        {
        }

        private void Map_Styled(object sender, EventArgs e)
        {

        }
    }
}
