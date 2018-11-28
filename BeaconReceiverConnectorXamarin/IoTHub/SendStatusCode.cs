using System;
using System.Collections.Generic;
using System.Text;
using IoTHubJavaClientRewrittenInDotNet;

namespace BeaconReceiverConnectorXamarin.IoTHub
{
    public enum SendStatusCodeEnum
    {
        OK, OK_EMPTY, BAD_FORMAT, UNAUTHORIZED, TOO_MANY_DEVICES,
        HUB_OR_DEVICE_ID_NOT_FOUND,
        PRECONDITION_FAILED, REQUEST_ENTITY_TOO_LARGE, THROTTLED,
        INTERNAL_SERVER_ERROR, SERVER_BUSY, ERROR, MESSAGE_EXPIRED,
        /**
         * 接続先ホストが不明
         */
        UNKNOWN_HOST,
        /**
         * IoT Hub接続文字列の要素が不足しているか、誤りがある
         */
        CONNECTIONSTRING_FAILED
    }
    public class SendStatusCode
    {


        public const int INTERNAL_STATUS_UNKNOWN_HOST = -900;
        public const int INTERNAL_STATUS_CONNECTIONSTRING_FAILED = -901;
        public static SendStatusCodeEnum getSendStatusCode(int statusCode) {
            SendStatusCodeEnum sendStatus;

            switch( statusCode ) {

                case INTERNAL_STATUS_UNKNOWN_HOST:
                    sendStatus = SendStatusCodeEnum.UNKNOWN_HOST;
                    break;
                case INTERNAL_STATUS_CONNECTIONSTRING_FAILED:
                    sendStatus = SendStatusCodeEnum.CONNECTIONSTRING_FAILED;
                    break;
                default:
                    sendStatus = getSendStatusCode(IotHubStatusCode.getIotHubStatusCode(statusCode));
                    break;
            }

            return sendStatus;
        }

        /**
         * Returns the send status code referenced by the Microsoft Azure IoT Hub status code.
         *
         * @param statusCode the Microsoft Azure IoT Hub status code.
         * @return the corresponding send status code.
         */
        static SendStatusCodeEnum getSendStatusCode(IotHubStatusCodeEnum statusCode)
        {
            SendStatusCodeEnum iotHubStatus;
            switch (statusCode)
            {
                case IotHubStatusCodeEnum.OK:
                    iotHubStatus = SendStatusCodeEnum.OK;
                    break;
                case IotHubStatusCodeEnum.OK_EMPTY:
                    iotHubStatus = SendStatusCodeEnum.OK_EMPTY;
                    break;
                case IotHubStatusCodeEnum.BAD_FORMAT:
                    iotHubStatus = SendStatusCodeEnum.BAD_FORMAT;
                    break;
                case IotHubStatusCodeEnum.UNAUTHORIZED:
                    iotHubStatus = SendStatusCodeEnum.UNAUTHORIZED;
                    break;
                case IotHubStatusCodeEnum.TOO_MANY_DEVICES:
                    iotHubStatus = SendStatusCodeEnum.TOO_MANY_DEVICES;
                    break;
                case IotHubStatusCodeEnum.HUB_OR_DEVICE_ID_NOT_FOUND:
                    iotHubStatus = SendStatusCodeEnum.HUB_OR_DEVICE_ID_NOT_FOUND;
                    break;
                case IotHubStatusCodeEnum.PRECONDITION_FAILED:
                    iotHubStatus = SendStatusCodeEnum.PRECONDITION_FAILED;
                    break;
                case IotHubStatusCodeEnum.REQUEST_ENTITY_TOO_LARGE:
                    iotHubStatus = SendStatusCodeEnum.REQUEST_ENTITY_TOO_LARGE;
                    break;
                case IotHubStatusCodeEnum.THROTTLED:
                    iotHubStatus = SendStatusCodeEnum.THROTTLED;
                    break;
                case IotHubStatusCodeEnum.INTERNAL_SERVER_ERROR:
                    iotHubStatus = SendStatusCodeEnum.INTERNAL_SERVER_ERROR;
                    break;
                case IotHubStatusCodeEnum.SERVER_BUSY:
                    iotHubStatus = SendStatusCodeEnum.SERVER_BUSY;
                    break;
                case IotHubStatusCodeEnum.MESSAGE_EXPIRED:
                    iotHubStatus = SendStatusCodeEnum.MESSAGE_EXPIRED;
                    break;
                default:
                    iotHubStatus = SendStatusCodeEnum.ERROR;
                    break;
            }

            return iotHubStatus;
        }
    }
}
