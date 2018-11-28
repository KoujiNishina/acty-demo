using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverConnectorXamarin.CustomerFront
{
    public class RegisterAsyncTask : HttpBaseAsyncTask
    {
        private String mRequestJsonBody;

        public RegisterAsyncTask(String requestJsonBody)
        {

            mRequestJsonBody = requestJsonBody;
        }

        protected override String getRequestMethod()
        {

            return REQUEST_METHOD_POST;
        }

        protected override String getRequestJsonBody()
        {

            return mRequestJsonBody;
        }
    }
}
