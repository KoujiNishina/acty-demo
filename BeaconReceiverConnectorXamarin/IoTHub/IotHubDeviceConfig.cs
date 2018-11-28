using IoTHubJavaClientRewrittenInDotNet;
using IoTHubJavaClientRewrittenInDotNet.Auth;
using IoTHubJavaClientRewrittenInDotNet.Net;
using IoTHubJavaClientRewrittenInDotNet.Transport;
using IoTHubJavaClientRewrittenInDotNet.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace BeaconReceiverConnectorXamarin.IoTHub
{
    class IotHubDeviceConfig
    {

        private const String HOSTNAME_ATTRIBUTE = "HostName=";
        private const String DEVICE_ID_ATTRIBUTE = "DeviceId=";
        private const String SHARED_ACCESS_KEY_ATTRIBUTE = "SharedAccessKey=";
        //private const String CONNECTION_STRING_CHARSET = "UTF-8";
        private const String CONTENT_TYPE = "application/json; charset=UTF-8";

        private DeviceClientConfig deviceClientConfig;

        public IotHubDeviceConfig(String connectionString)
        {

            if (connectionString == null) {

                throw new ArgumentException("IoT Hub connection string cannot be null.");
            }

            String[] connStringAttrs = connectionString.Split(";"[0]);
            String hostname = null;
            String deviceId = null;
            String sharedAccessKey = null;
            foreach (String attr in connStringAttrs) {

                if (attr.StartsWith(HOSTNAME_ATTRIBUTE)) {

                    hostname = attr.Substring(HOSTNAME_ATTRIBUTE.Length);
                } else if (attr.StartsWith(DEVICE_ID_ATTRIBUTE)) {

                    String urlEncodedDeviceId = attr.Substring(DEVICE_ID_ATTRIBUTE.Length);
                    
                    deviceId = HttpUtility.UrlDecode(urlEncodedDeviceId, Encoding.UTF8);
                    
                } else if (attr.StartsWith(SHARED_ACCESS_KEY_ATTRIBUTE)) {

                    sharedAccessKey = attr.Substring(SHARED_ACCESS_KEY_ATTRIBUTE.Length);
                }
            }

            if (hostname == null) {

                throw new ArgumentException("IoT Hub hostname cannot be null.");
            }
            if (deviceId == null) {

                throw new ArgumentException("Device ID cannot be null.");
            }
            if (sharedAccessKey == null) {

                throw new ArgumentException("Device key cannot be null.");
            }

            deviceClientConfig = new DeviceClientConfig(hostname, deviceId, sharedAccessKey);
        }

        public int getReadTimeoutMillis()
        {
            return deviceClientConfig.getReadTimeoutMillis();
        }

        public String getEventUriString()
        {

            IotHubEventUri eventUri = new IotHubEventUri(deviceClientConfig.getIotHubHostname(), deviceClientConfig.getDeviceId());

            return eventUri.ToString();
        }

        public String getEventUriPath()
        {

            IotHubEventUri eventUri = new IotHubEventUri(deviceClientConfig.getIotHubHostname(), deviceClientConfig.getDeviceId());

            return eventUri.getPath();
        }

        public String getContentType()
        {

            return CONTENT_TYPE;
        }

        public String getUserAgent()
        {

            return TransportUtils.javaDeviceClientIdentifier + TransportUtils.clientVersion;
        }

        public String getSasToken()
        {

            long expiryTime = (DateTimeUtils.GetCurrentTimeMillis()) / 1000 + deviceClientConfig.getTokenValidSecs() + 1;
            String scopeUri = IotHubUri.getResourceUri(deviceClientConfig.getIotHubHostname(), deviceClientConfig.getDeviceId());
            IotHubSasToken sasToken = new IotHubSasToken(scopeUri, deviceClientConfig.getDeviceKey(), expiryTime);

            return sasToken.ToString();
        }
    }
}
