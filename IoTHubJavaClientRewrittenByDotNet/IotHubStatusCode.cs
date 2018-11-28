// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Rewritten in C#.net (originally in Java) by NTT PC Communications.

using System;
using System.Collections.Generic;

namespace IoTHubJavaClientRewrittenInDotNet
{
    public enum IotHubStatusCodeEnum
    {
        OK, OK_EMPTY, BAD_FORMAT, UNAUTHORIZED, TOO_MANY_DEVICES,
        HUB_OR_DEVICE_ID_NOT_FOUND,
        PRECONDITION_FAILED, REQUEST_ENTITY_TOO_LARGE, THROTTLED,
        INTERNAL_SERVER_ERROR, SERVER_BUSY, ERROR, MESSAGE_EXPIRED
    }
    /**
     * An IoT Hub status code. Included in a message from an IoT Hub to a device.
     */
    public class IotHubStatusCode
    {
        //private static IotHubStatusCode[] codes = {OK, OK_EMPTY, BAD_FORMAT, UNAUTHORIZED, TOO_MANY_DEVICES,
        //HUB_OR_DEVICE_ID_NOT_FOUND,
        //PRECONDITION_FAILED, REQUEST_ENTITY_TOO_LARGE, THROTTLED,
        //INTERNAL_SERVER_ERROR, SERVER_BUSY, ERROR, MESSAGE_EXPIRED};

        //public static int GetInt(IotHubStatusCode code)
        //{
        //    return new List<IotHubStatusCode>(codes).IndexOf(code);
        //}

        /**
         * Returns the IoT Hub status code referenced by the HTTPS status code.
         *
         * @param httpsStatus the HTTPS status code.
         *
         * @return the corresponding IoT Hub status code.
         *
         * @throws IllegalArgumentException if the HTTPS status code does not map to
         * an IoT Hub status code.
         */
        public static IotHubStatusCodeEnum getIotHubStatusCode(int httpsStatus)
        {
            // Codes_SRS_IOTHUBSTATUSCODE_11_001: [The function shall convert the given HTTPS status code to the corresponding IoT Hub status code.]
            IotHubStatusCodeEnum iotHubStatus;
            switch (httpsStatus)
            {
                case 200:
                    iotHubStatus = IotHubStatusCodeEnum.OK;
                    break;
                case 204:
                    iotHubStatus = IotHubStatusCodeEnum.OK_EMPTY;
                    break;
                case 400:
                    iotHubStatus = IotHubStatusCodeEnum.BAD_FORMAT;
                    break;
                case 401:
                    iotHubStatus = IotHubStatusCodeEnum.UNAUTHORIZED;
                    break;
                case 403:
                    iotHubStatus = IotHubStatusCodeEnum.TOO_MANY_DEVICES;
                    break;
                case 404:
                    iotHubStatus = IotHubStatusCodeEnum.HUB_OR_DEVICE_ID_NOT_FOUND;
                    break;
                case 412:
                    iotHubStatus = IotHubStatusCodeEnum.PRECONDITION_FAILED;
                    break;
                case 413:
                    iotHubStatus = IotHubStatusCodeEnum.REQUEST_ENTITY_TOO_LARGE;
                    break;
                case 429:
                    iotHubStatus = IotHubStatusCodeEnum.THROTTLED;
                    break;
                case 500:
                    iotHubStatus = IotHubStatusCodeEnum.INTERNAL_SERVER_ERROR;
                    break;
                case 503:
                    iotHubStatus = IotHubStatusCodeEnum.SERVER_BUSY;
                    break;
                default:
                    // Codes_SRS_IOTHUBSTATUSCODE_11_002: [If the given HTTPS status code does not map to an IoT Hub status code, the function return status code ERROR.]
                    iotHubStatus = IotHubStatusCodeEnum.ERROR;
                    break;
            }

            return iotHubStatus;
        }
    }
}
