using System.Windows;

namespace DemosWPF
{
    /// <summary>
    /// Interaction logic for HelloWorldWindow.xaml
    /// </summary>
    public partial class HelloWorldWindow : Window
    {
        public HelloWorldWindow(string accessToken)
        {
            InitializeComponent();
            Map.AccessToken = accessToken;

            // Check the XAML
        }
    }
}
