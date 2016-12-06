using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace Adventure_Works.Rome
{
    public abstract class ConnectedServiceEventArgs
    {
        public bool Handled { get; set; } = false;
        public ValueSet ResponseMessage { get; set; }
    }


}
