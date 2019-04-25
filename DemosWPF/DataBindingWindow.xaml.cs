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
    /// Interaction logic for DataBindingWindow.xaml
    /// </summary>
    public partial class DataBindingWindow : Window
    {
        public DataBindingWindow(string accessToken)
        {
            InitializeComponent();
            Map.AccessToken = accessToken;

            // Check the XAML for binding
        }
    }
}
