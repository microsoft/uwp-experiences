using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music.PCL
{
    public interface IDispatcher
    {
        void BeginInvoke(Action action);
    }
}
