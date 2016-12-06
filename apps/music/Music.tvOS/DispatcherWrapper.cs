using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music
{
    public class DispatcherWrapper : PCL.IDispatcher
    {
        NSObject _host;

        public DispatcherWrapper(NSObject host)
        {
            _host = host;
        }

        public void BeginInvoke(Action action)
        {
            _host.InvokeOnMainThread(action);
        }
    }
}
