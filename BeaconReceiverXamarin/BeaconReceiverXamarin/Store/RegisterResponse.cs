using BeaconReceiverConnectorXamarin.CustomerFront.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverXamarin.Store
{
    public class RegisterResponse : RegisterResponseBase
    {
        public List<ReceiversInfo> items { get; set; }
    }
}
