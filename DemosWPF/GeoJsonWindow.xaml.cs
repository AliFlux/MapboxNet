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
    /// Interaction logic for GeoJsonWindow.xaml
    /// </summary>
    public partial class GeoJsonWindow : Window
    {
        public GeoJsonWindow(string accessToken)
        {
            InitializeComponent();
            Map.AccessToken = accessToken;
        }

        private void Map_Styled(object sender, EventArgs e)
        {
            // Making two layers, one is a geojson polygon, other is a set of pikachu markers/images

            // Converted using the JSON to C# anonymous type converter
            // source: https://docs.mapbox.com/mapbox-gl-js/example/geojson-polygon/
            // converter: https://jsfiddle.net/aliashrafx/c7pxomjb/39/

            var polygonLayer = new Dictionary<string, object>
            {
                ["id"] = "maine",
                ["type"] = "fill",
                ["source"] = new Dictionary<string, object>
                {
                    ["type"] = "geojson",
                    ["data"] = new Dictionary<string, object>
                    {
                        ["type"] = "Feature",
                        ["geometry"] = new Dictionary<string, object>
                        {
                            ["type"] = "Polygon",
                            ["coordinates"] = new object[] {
                                new object[] {
                                    new object[] {
                                        -67.13734351262877, 45.137451890638886,
                                    },
                                    new object[] {
                                        -66.96466, 44.8097,
                                    },
                                    new object[] {
                                        -68.03252, 44.3252,
                                    },
                                    new object[] {
                                        -69.06, 43.98,
                                    },
                                    new object[] {
                                        -70.11617, 43.68405,
                                    },
                                    new object[] {
                                        -70.64573401557249, 43.090083319667144,
                                    },
                                    new object[] {
                                        -70.75102474636725, 43.08003225358635,
                                    },
                                    new object[] {
                                        -70.79761105007827, 43.21973948828747,
                                    },
                                    new object[] {
                                        -70.98176001655037, 43.36789581966826,
                                    },
                                    new object[] {
                                        -70.94416541205806, 43.46633942318431,
                                    },
                                    new object[] {
                                        -71.08482, 45.3052400000002,
                                    },
                                    new object[] {
                                        -70.6600225491012, 45.46022288673396,
                                    },
                                    new object[] {
                                        -70.30495378282376, 45.914794623389355,
                                    },
                                    new object[] {
                                        -70.00014034695016, 46.69317088478567,
                                    },
                                    new object[] {
                                        -69.23708614772835, 47.44777598732787,
                                    },
                                    new object[] {
                                        -68.90478084987546, 47.184794623394396,
                                    },
                                    new object[] {
                                        -68.23430497910454, 47.35462921812177,
                                    },
                                    new object[] {
                                        -67.79035274928509, 47.066248887716995,
                                    },
                                    new object[] {
                                        -67.79141211614706, 45.702585354182816,
                                    },
                                    new object[] {
                                        -67.13734351262877, 45.137451890638886,
                                    },
                                },
                            },
                        },
                    },
                },
                ["paint"] = new Dictionary<string, object>
                {
                    ["fill-color"] = "#9b59b6",
                    ["fill-opacity"] = 0.4,
                    ["fill-outline-color"] = "#e74c3c",
                },
            };

            // inspired by: https://docs.mapbox.com/mapbox-gl-js/example/add-image/
            // converter: https://jsfiddle.net/aliashrafx/c7pxomjb/39/

            var pikachuLayer = new Dictionary<string, object>
            {
                ["id"] = "points",
                ["type"] = "symbol",
                ["source"] = new Dictionary<string, object>
                {
                    ["type"] = "geojson",
                    ["data"] = new Dictionary<string, object>
                    {
                        ["type"] = "FeatureCollection",
                        ["features"] = new object[] {
                            new Dictionary<string, object> {
                                ["type"] = "Feature",
                                ["properties"] = new Dictionary<string, object> {
                                    ["label"] = "Pika pika",
                                },
                                ["geometry"] = new Dictionary<string, object> {
                                    ["type"] = "Point",
                                    ["coordinates"] = new object[] {
                                        -74.0124907170679, 40.7052768300975
                                    },
                                },
                            },
                            new Dictionary<string, object> {
                                ["type"] = "Feature",
                                ["properties"] = new Dictionary<string, object> {
                                    ["label"] = "Pika chuuuu",
                                },
                                ["geometry"] = new Dictionary<string, object> {
                                    ["type"] = "Point",
                                    ["coordinates"] = new object[] {
                                        -78.6569, 37.4316
                                    },
                                },
                            },
                            new Dictionary<string, object> {
                                ["type"] = "Feature",
                                ["properties"] = new Dictionary<string, object> {
                                    ["label"] = "Pikaaa",
                                },
                                ["geometry"] = new Dictionary<string, object> {
                                    ["type"] = "Point",
                                    ["coordinates"] = new object[] {
                                        -79.3832, 43.6532
                                    },
                                },
                            },
                        },
                    },
                },
                ["layout"] = new Dictionary<string, object>
                {
                    ["text-field"] = "{label}",
                    ["text-anchor"] = "bottom",
                    ["icon-anchor"] = "top",
                    ["icon-image"] = "pikachu",
                    ["icon-size"] = 0.75,
                },
            };

            // You can add images to the map using bitmap, stream, or base64
            Map.AddImage("pikachu", pikachuImage.Source as BitmapSource);

            Map.Invoke.AddLayer(polygonLayer);
            Map.Invoke.AddLayer(pikachuLayer);

        }
    }
}
