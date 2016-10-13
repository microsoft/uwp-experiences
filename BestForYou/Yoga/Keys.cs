using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoga
{
    public static class Keys
    {
        // found under Event Hub -> Messaging section in Azure Portal
        public static string EventHubCompatibleEndpoint = "";
        public static string EventHubCompatibleName = "";

        // found under IoT Hub -> Settings -> Shared access policies in Azure Portal
        public static string IoTHubAccessPolicyName = "";
        public static string IoTHubAccessPolicyPrimaryKey = "";
    }
}
