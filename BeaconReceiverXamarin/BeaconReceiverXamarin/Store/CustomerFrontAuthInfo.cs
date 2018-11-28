using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverXamarin.Store
{
    /// <summary>
    /// カスタマーフロントの認証情報
    /// </summary>
    public class CustomerFrontAuthInfo
    {
        /** カスタマーフロントID */
        public String id { get; set; }
        /** Iカスタマーフロントパスワード */
        public String password { get; set; }

        public CustomerFrontAuthInfo()
        {
            id = null;
            password = null;
        }
    }
}
