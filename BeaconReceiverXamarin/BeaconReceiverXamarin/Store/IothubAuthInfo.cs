using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverXamarin.Store
{
    /// <summary>
    /// IoTHub受信機情報
    /// </summary>
    public class IothubAuthInfo
    {

        /** 受信機名 */
        public String name { get; set; }
        /** 受信機パスワード */
        public String password { get; set; }

        public IothubAuthInfo()
        {
            name = null;
            password = null;
        }
    }
}
