using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MapboxNetWPF
{
    class Relay
    {
        Dispatcher dispatcher;
        Action<string> callback;

        public Relay(Action<string> callback, Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            this.callback = callback;
        }

        public void notify(string json)
        {
            dispatcher.BeginInvoke((Action)(() =>
            {
                callback(json);
            }));
        }
    }
}
