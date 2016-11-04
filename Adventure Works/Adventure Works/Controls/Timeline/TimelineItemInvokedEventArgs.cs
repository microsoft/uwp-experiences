using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controls
{
    public class TimelineItemInvokedEventArgs : EventArgs
    {
        public TimelineItem Container { get; set; }
    }
}
