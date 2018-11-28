using BeaconReceiverConnectorXamarin.IoTHub;
using BeaconReceiverConnectorXamarin.IoTHub.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverXamarin.Connect
{
    /**
     * Created by ryoma.saito on 16/08/24.
     */
    public class IotHubApiClient
    {
        /** Gatewayインスタンス */
        private DeviceToCloudGateway mSendGateway;

        public IotHubApiClient(String activeHost, String standbyHost, int failoverCondition, int failbackCondition)
        {

            mSendGateway = new DeviceToCloudGateway(activeHost, standbyHost, failoverCondition, failbackCondition);

        }

        /**
         * TouchDataをIoTHubへ送信.
         * ライブラリレベルで死亡時にはfalseを返却.
         *
         * @param touchTableData
         * @return
         */
        public bool sendAnalyzedData(List<TouchParamBase> touchTableData, SendCallback callback, SendSingleMessageCallback singleMessageCallback)
        {

            return mSendGateway.send(touchTableData, callback, singleMessageCallback);

        }
    }
}
