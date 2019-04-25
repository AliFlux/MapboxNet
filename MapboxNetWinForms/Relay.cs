using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapboxNetWinForms
{
    class Relay
    {
        Control control;
        Action<string> callback;

        public Relay(Action<string> callback, Control control)
        {
            this.control = control;
            this.callback = callback;
        }

        public void notify(string json)
        {
            control.Invoke((MethodInvoker)delegate {
                callback(json);
            });
        }
    }
}
