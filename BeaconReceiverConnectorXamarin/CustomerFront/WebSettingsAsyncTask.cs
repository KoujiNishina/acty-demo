using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverConnectorXamarin.CustomerFront
{
    class WebSettingsAsyncTask : HttpBaseAsyncTask
    {
        protected override String getRequestMethod()
        {

            return REQUEST_METHOD_GET;
        }
    }
}
