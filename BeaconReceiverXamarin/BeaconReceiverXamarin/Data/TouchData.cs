using BeaconReceiverConnectorXamarin.IoTHub.Data;
using BeaconReceiverXamarin.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverXamarin.Data
{
    /**
     * IoTHubへの送信データ格納Info
     *
     * Created by chiho.adachi on 2016/08/09.
     */
    public class TouchData : TouchParamBase
    {
        public TouchData() :base()
        {

            this.receiver = new Receiver();
            this.beacon = new Beacon();
            this.location = new Location();
            this.code = CommonConstants.JSON_CODE_VALUE;
        }
    }

}
