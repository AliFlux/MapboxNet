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
    /// Interaction logic for _3DBuildingsWindow.xaml
    /// </summary>
    public partial class _3DBuildingsWindow : Window
    {
        public _3DBuildingsWindow(string accessToken)
        {
            InitializeComponent();
            Map.AccessToken = accessToken;
        }

        private void Map_Styled(object sender, EventArgs e)
        {

            // Extruding the building polygons to create a 3D map view

            // Converted using the JSON to C# anonymous type converter
            // source: https://docs.mapbox.com/mapbox-gl-js/example/3d-buildings/
            // converter: https://jsfiddle.net/aliashrafx/c7pxomjb/39/

            var layers = Map.Invoke.getStyle().layers;
            var labelLayerId = "";

            // firstly, we get the text layer

            foreach (var layer in layers)
                if (layer.type == "" || (layer.ContainsKey("layout") && layer.layout.ContainsKey("text-field")))
                {
                    labelLayerId = layer.id;
                    break;
                }
            
            // then we create a new extrusion layer

            var obj = new Dictionary<string, object>
            {
                ["id"] = "3d-buildings",
                ["source"] = "composite",
                ["source-layer"] = "building",
                ["filter"] = new [] { "==", "extrude", "true" },
                ["type"] = "fill-extrusion",
                ["minzoom"] = 15,
                ["paint"] = new Dictionary<string, object>
                {
                    ["fill-extrusion-color"] = "#dfe4ea",
                    ["fill-extrusion-height"] = new object[] {
                        "interpolate" ,
                        new [] { "linear" },
                        new [] { "zoom" },
                        14, 0, 15.05,
                        new [] { "get", "height" }
                    },
                    ["fill-extrusion-base"] = new object[] {
                        "interpolate",
                        new [] { "linear" },
                        new [] { "zoom" },
                        14, 0, 15.05,
                        new [] { "get", "min_height" }
                    },
                    ["fill-extrusion-opacity"] = .6
                }
            };

            // now we add the new layer after text layer

            Map.Invoke.AddLayer(obj, labelLayerId);

        }
    }
}
