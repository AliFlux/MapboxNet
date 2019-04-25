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
    /// Interaction logic for LocalVectorWindow.xaml
    /// </summary>
    public partial class LocalVectorWindow : Window
    {
        public LocalVectorWindow()
        {
            InitializeComponent();

            // Starting the MapServer on a random port, that serves the vector tiles stored in .mbtiles format

            var server = new MapboxNetCore.MapServer(@"tiles\zurich.mbtiles");
            server.GlyphsPath = @"fonts\";
            server.Start();

            // pulling the style json stored as an embedded resource in this project, and decoding it into C# dynamic object

            var json = MapboxNetCore.Core.GetEmbeddedResource(this.GetType(), "DemosWPF.aliflux-style.json");
            dynamic style = MapboxNetCore.Core.DecodeJsonPlain(json);

            // modifying the style a bit so that it uses our tile server

            style.sources.openmaptiles.tiles.Add(server.TilesURL);
            style.glyphs = server.GlyphsURL;

            // NOTE: this style (aliflux-style.json) is using the openmaptiles spec, not the mapbox spec
            // It is specifically compatable with the zurich.mbtiles vector tiles

            Map.MapStyle = style;
        }
    }
}
