using System;
using System.Windows;

namespace DemosWPF
{
    /// <summary>
    /// Interaction logic for XamlProjectionWindow.xaml
    /// </summary>
    public partial class XamlProjectionWindow : Window
    {
        public XamlProjectionWindow(string accessToken)
        {
            InitializeComponent();
            Map.AccessToken = accessToken;
        }

        private void Map_Render(object sender, EventArgs e)
        {
            // Render event is called whenever the map is redrawn
            // basically when style/location/pitch/rotation/etc is changed

            // We convert a specific lat/lon into pixels
            var boxLocation = new MapboxNetCore.GeoLocation(61.767785, 4.898183);
            var pointOnScreen = Map.Project(boxLocation);

            // then set the box position to those pixels
            Box.Margin = new Thickness(pointOnScreen.X - Box.ActualWidth/2, pointOnScreen.Y - Box.ActualHeight/2, 0, 0);
        }
    }
}
