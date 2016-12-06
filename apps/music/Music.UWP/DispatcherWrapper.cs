using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Music
{
    public class DispatcherWrapper : PCL.IDispatcher
    {
        protected CoreDispatcher _dispatcher { get; private set; }

        public DispatcherWrapper(CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void BeginInvoke(Action action)
        {
            var t = _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
        }
    }
}
