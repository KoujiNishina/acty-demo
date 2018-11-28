using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverConnectorXamarin.IoTHub
{
    /**
     * Created by kenji.izumi on 2016/09/08.
     */
    class IotHubDeviceClient
    {

        public enum DestinationHost
        {

            ACTIVE,
            STANDBY,
        }

        private String mConnectionStringActiveHost;
        private String mConnectionStringStandbyHost;

        public IotHubDeviceClient(String connectionString)
        {

            mConnectionStringActiveHost = connectionString;
            mConnectionStringStandbyHost = null;
        }

        public IotHubDeviceClient(String connectionStringActiveHost, String connectionStringStandbyHost)
        {

            mConnectionStringActiveHost = connectionStringActiveHost;
            mConnectionStringStandbyHost = connectionStringStandbyHost;
        }

        public void sendEventAsync(DestinationHost destinationHost, String d2cMessage, IotHubHttpAsyncTask.EventCallback callback, Object callbackContext)
        {

            DestinationHost correctHost = destinationHost;

            if (mConnectionStringStandbyHost == null)
            {

                correctHost = DestinationHost.ACTIVE;
            }

            String connectionString;

            if (correctHost == DestinationHost.ACTIVE)
            {

                connectionString = mConnectionStringActiveHost;
            }
            else
            {

                connectionString = mConnectionStringStandbyHost;
            }

            String[] parameters = {connectionString, d2cMessage};

            IotHubHttpAsyncTask task = new IotHubHttpAsyncTask(callback, callbackContext);
            task.DoInBackground(parameters);
        }
    }
}
