// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Rewritten in C#.net (originally in Java) by NTT PC Communications.

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace IoTHubJavaClientRewrittenInDotNet.Net
{
    /**
     * A URI for a device to connect to an IoT Hub.
     */
    public class IotHubUri
    {
        /**
         * The device ID and specific IoT Hub method path will be interpolated into
         * the path.
         */
        public const String PATH_FORMAT = "/devices/{0}{1}";

        /** The API version will be passed as a param in the URI. */
        public const String API_VERSION = "api-version=2016-02-03";

        /** The charset used when URL-encoding the IoT Hub name and device ID. */
        public static Encoding IOTHUB_URL_ENCODING_CHARSET =
                Encoding.UTF8;

        /**
         * The IoT Hub resource URI is the hostname and path component that is
         * common to all IoT Hub communication methods between the given device and
         * IoT Hub.
         */
        protected String hostname;
        protected String path;
        protected String uri;

        /**
         * Constructor. Creates a URI to an IoT Hub method. The URI does not include
         * a protocol. The function will safely escape the given arguments.
         *
         * @param iotHubHostname the IoT Hub hostname.
         * @param deviceId the device ID.
         * @param iotHubMethodPath the path from the IoT Hub resource to the
         * method.
         * @param queryParams the URL query parameters. Can be null.
         */
        public IotHubUri(String iotHubHostname, String deviceId,
                String iotHubMethodPath, Dictionary<String, String> queryParams)
        {
            this.hostname = iotHubHostname;

            String rawPath = String.Format(
                    PATH_FORMAT, deviceId, iotHubMethodPath);
            // Codes_SRS_IOTHUBURI_11_011: [The constructor shall URL-encode the device ID.]
            // Codes_SRS_IOTHUBURI_11_012: [The constructor shall URL-encode the IoT Hub method path.]
            this.path = urlEncodePath(rawPath);

            // Codes_SRS_IOTHUBURI_11_008: [If queryParams is not empty, the constructor shall return a URI pointing to the address '[iotHubHostname]/devices/[deviceId]/[IoT Hub method path]? api-version=2016-02-03 &[queryFragment] '.]
            // Codes_SRS_IOTHUBURI_11_009: [If the queryParams is empty, the constructor shall return a URI pointing to the address '[iotHubHostname]/devices/[deviceId]/[IoT Hub method path]?api-version=2016-02-03'.]
            StringBuilder uriBuilder = new StringBuilder(this.hostname);
            uriBuilder.Append(this.path);
            uriBuilder.Append("?");
            uriBuilder.Append(API_VERSION);
            if (queryParams != null)
            {
                foreach (var entry in queryParams)
                {
                    uriBuilder.Append("&");
                    appendQueryParam(uriBuilder, entry.Key,
                        entry.Value);
                }
            }

            this.uri = uriBuilder.ToString();
        }

        /**
         * Constructor. Equivalent to {@code new IotHubUri(iotHubHostname, deviceId,
         * iotHubMethodPath, null)}.
         *
         * @param iotHubHostname the IoT Hub hostname.
         * @param deviceId the device ID.
         * @param iotHubMethodPath the path from the IoT Hub resource to the
         * method.
         */
        public IotHubUri(String iotHubHostname, String deviceId,
                String iotHubMethodPath) : this(iotHubHostname, deviceId, iotHubMethodPath, null)
        {
            // Codes_SRS_IOTHUBURI_11_007: [The constructor shall return a URI pointing to the address '[iotHubHostname] /devices/[deviceId]/[IoT Hub method path]?api-version=2016-02-03'.]
            // Codes_SRS_IOTHUBURI_11_015: [The constructor shall URL-encode the device ID.]
            // Codes_SRS_IOTHUBURI_11_016: [The constructor shall URL-encode the IoT Hub method path.]
        }

        /**
         * Returns the string representation of the IoT Hub URI.
         *
         * @return the string representation of the IoT Hub URI.
         */
        public override String ToString()
        {
            // Codes_SRS_IOTHUBURI_11_001: [The string representation of the IoT Hub URI shall be constructed with the format '[iotHubHostname]/devices/[deviceId]/[IoT Hub method path]?api-version=2016-02-03(&[queryFragment]) '.]
            return this.uri;
        }

        /**
         * Returns the string representation of the IoT Hub hostname.
         *
         * @return the string representation of the IoT Hub hostname.
         */
        public String getHostname()
        {
            // Codes_SRS_IOTHUBURI_11_005: [The function shall return the IoT hub hostname given in the constructor.]
            return this.hostname;
        }

        /**
         * Returns the string representation of the IoT Hub path.
         *
         * @return the string representation of the IoT Hub path.
         */
        public String getPath()
        {
            // Codes_SRS_IOTHUBURI_11_006: [The function shall return a URI with the format '/devices/[deviceId]/[IoT Hub method path]'.]
            return this.path;
        }

        /**
         * Returns the string representation of the IoT Hub resource URI. The IoT
         * Hub resource URI is the hostname and path component that is common to all
         * IoT Hub communication methods between the given device and IoT Hub.
         * Safely escapes the IoT Hub resource URI.
         *
         * @param iotHubHostname the IoT Hub hostname.
         * @param deviceId the device ID.
         *
         * @return the string representation of the IoT Hub resource URI.
         */
        public static String getResourceUri(String iotHubHostname, String deviceId)
        {
            // Codes_SRS_IOTHUBURI_11_002: [The function shall return a URI with the format '[iotHubHostname]/devices/[deviceId]'.]
            // Codes_SRS_IOTHUBURI_11_019: [The constructor shall URL-encode the device ID.]
            IotHubUri iotHubUri = new IotHubUri(iotHubHostname, deviceId, "");
            return iotHubUri.getHostname() + iotHubUri.getPath();
        }

        /**
         * URL-encodes each subdirectory in the path.
         *
         * @param path the path to be safely escaped.
         *
         * @return a path with each subdirectory URL-encoded.
         */
        protected static String urlEncodePath(String path)
        {
            String[] pathSubDirs = path.Split("/"[0]);
            StringBuilder urlEncodedPathBuilder = new StringBuilder();
            foreach (String subDir in pathSubDirs)
            {
                if (subDir.Length > 0)
                {
                    String urlEncodedSubDir = HttpUtility.UrlEncode(subDir, IOTHUB_URL_ENCODING_CHARSET);
                    urlEncodedPathBuilder.Append("/");
                    urlEncodedPathBuilder.Append(urlEncodedSubDir);
                }
            }

            return urlEncodedPathBuilder.ToString();
        }

        /**
         * URL-encodes the query param {@code name} and {@code value} using charset UTF-8 and
         * appends them to the URI.
         *
         * @param uriBuilder the URI.
         * @param name the query param name.
         * @param value the query param value.
         */
        protected static void appendQueryParam(StringBuilder uriBuilder,
                String name, String value)
        {
            String urlEncodedName = HttpUtility.UrlEncode(name, IOTHUB_URL_ENCODING_CHARSET);
            String urlEncodedValue = HttpUtility.UrlEncode(value, IOTHUB_URL_ENCODING_CHARSET);

            uriBuilder.Append(urlEncodedName);
            uriBuilder.Append("=");
            uriBuilder.Append(urlEncodedValue);
            
        }

        protected IotHubUri()
        {

        }
    }
}
