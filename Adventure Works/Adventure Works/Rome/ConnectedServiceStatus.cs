using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure_Works.Rome
{
    public enum ConnectedServiceStatus
    {
        IdleBackground,
        IdleForeground,
        HostingNotConnected,
        HostingConnected,
        RemoteConnected
    }
}
