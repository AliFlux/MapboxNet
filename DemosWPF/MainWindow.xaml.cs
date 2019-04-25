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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DemosWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string accessToken = "";

        public MainWindow()
        {
            InitializeComponent();

            // Write your access token here...
            // or set `AccessToken` property on each map control
            // Go to http://mapbox.com to register and get your token

            accessToken = "";

            accessToken = "pk.eyJ1IjoiYWxpYXNocmFmIiwiYSI6ImNqbmlrbThoYjBuamIzcG8zdXl6NG1qNm4ifQ.XPbRBbMekHi2L9aJ_H3Yqw";
            
            if (accessToken == "")
            {
                MessageBox.Show("Please write your access token in the code to enable all demos.");

                helloWorldButton.IsEnabled = false;
                threeDButton.IsEnabled = false;
                styleSwitchButton.IsEnabled = false;
                geoJsonButton.IsEnabled = false;
                dataBindingButton.IsEnabled = false;
                xamlProjectionButton.IsEnabled = false;
                videoOverlayButton.IsEnabled = false;
            }
        }

        private void helloWorldButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new HelloWorldWindow(accessToken);
            window.Show();
        }

        private void threeDButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new _3DBuildingsWindow(accessToken);
            window.Show();
        }

        private void styleSwitchButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new StyleSwitchWindow(accessToken);
            window.Show();
        }

        private void geoJsonButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new GeoJsonWindow(accessToken);
            window.Show();
        }

        private void dataBindingButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new DataBindingWindow(accessToken);
            window.Show();
        }

        private void bingButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new BingStyleWindow();
            window.Show();
        }

        private void customRasterButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new LocalRasterWindow();
            window.Show();
        }

        private void customVectorButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new LocalVectorWindow();
            window.Show();
        }

        private void xamlProjectionButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new XamlProjectionWindow(accessToken);
            window.Show();
        }

        private void videoOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new VideoOverlayWindow(accessToken);
            window.Show();
        }
    }
}
