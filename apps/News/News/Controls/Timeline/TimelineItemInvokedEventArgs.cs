using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News.Controls
{
    public class TimelineItemInvokedEventArgs : EventArgs
    {
        public TimelineItem Item { get; set; }
    }
}
