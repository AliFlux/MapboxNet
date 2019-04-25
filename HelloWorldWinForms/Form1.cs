using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HelloWorldWinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();


            // Write your access token here...
            // or set `AccessToken` property on each map control
            // Go to http://mapbox.com to register and get your token

            var accessToken = "";

            if (accessToken == "")
            {
                MessageBox.Show("Please write your access token in the code to run this demo.");
                return;
            }

            map.AccessToken = accessToken;
            map.Center = new MapboxNetCore.GeoLocation(59.9058906, 10.7190694);
            map.Zoom = 12;
        }
    }
}
