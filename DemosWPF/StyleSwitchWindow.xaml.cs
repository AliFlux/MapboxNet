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
    /// Interaction logic for StyleSwitchWindow.xaml
    /// </summary>
    public partial class StyleSwitchWindow : Window
    {
        public StyleSwitchWindow(string accessToken)
        {
            InitializeComponent();
            Map.AccessToken = accessToken;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var button = sender as RadioButton;
            var style = button.Tag.ToString();

            Map.Invoke.SetStyle("mapbox://styles/mapbox/" + style);

            // MapStyle property can also be used but it reloads the whole map component
            //Map.MapStyle = "mapbox://styles/mapbox/" + style;
        }
    }
}
