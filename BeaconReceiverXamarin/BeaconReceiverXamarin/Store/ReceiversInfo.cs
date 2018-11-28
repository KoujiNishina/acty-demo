using BeaconReceiverConnectorXamarin.CustomerFront.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverXamarin.Store
{
    public class ReceiversInfo : RegisterResponseBase
    {
        /** 受信機名 */
        public String name { get; set; }
        /** 受信機パスワード */
        public String password { get; set; }
        /** 稼働IoTHubのURL */
        public String active_host { get; set; }
        /** 待機IoTHubのURL */
        public String standby_host { get; set; }

        public ReceiversInfo()
        {
            name = null;
            password = null;
            active_host = null;
            standby_host = null;
        }
    }
}
