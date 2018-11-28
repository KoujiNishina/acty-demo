// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Rewritten in C#.net (originally in Java) by NTT PC Communications.

using System;
using System.Collections.Generic;
using System.Text;

namespace IoTHubJavaClientRewrittenInDotNet.Transport.Https
{
    public class HttpsBatchMessage : HttpsMessage
    {
        // Note: this limit is defined by the IoT Hub.
        public const int SERVICEBOUND_MESSAGE_MAX_SIZE_BYTES = 255 * 1024 - 1;

        /**
         * The value for the "content-type" header field in a batched HTTPS
         * request.
         */
        public static String HTTPS_BATCH_CONTENT_TYPE = "application/vnd.microsoft.iothub.json";

        /**
         * The charset used to encode IoT Hub messages. The server will interpret
         * the JSON array using UTF-8 by default according to RFC4627.
         */
        public static Encoding BATCH_CHARSET = Encoding.UTF8;

        /** The current batched message body. */
        protected String batchBody;
        /** The current number of messages in the batch. */
        protected int numMsgs;

        /** Constructor. Initializes the batch body as an empty JSON array. */
        public HttpsBatchMessage()
        {
            // Codes_SRS_HTTPSBATCHMESSAGE_11_001: [The constructor shall initialize the batch message with the body as an empty JSON array.]
            this.batchBody = "[]";
            this.numMsgs = 0;
        }

        /**
         * Adds a message to the batch.
         *
         * @param msg the message to be added.
         *
         * @throws SizeLimitExceededException if adding the message causes the
         * batched message to exceed 256 kb in size. The batched message will remain
         * as if the message was never added.
         */
        public void addMessage(HttpsSingleMessage msg)
        {
            String jsonMsg = msgToJson(msg);
            // Codes_SRS_HTTPSBATCHMESSAGE_11_002: [The function shall add the message as a JSON object appended to the current JSON array.]
            String newBatchBody = addJsonObjToArray(jsonMsg, this.batchBody);

            // Codes_SRS_HTTPSBATCHMESSAGE_11_008: [If adding the message causes the batched message to exceed 256 kb in size, the function shall throw a SizeLimitExceededException.]
            // Codes_SRS_HTTPSBATCHMESSAGE_11_009: [If the function throws a SizeLimitExceedException, the batched message shall remain as if the message was never added.]
            byte[] newBatchBodyBytes = BATCH_CHARSET.GetBytes(newBatchBody);

            if (newBatchBodyBytes.Length > SERVICEBOUND_MESSAGE_MAX_SIZE_BYTES) {
                String errMsg = String.Format("Service-bound message size ({0} bytes) cannot exceed {1} bytes.\n",
                        newBatchBodyBytes.Length, SERVICEBOUND_MESSAGE_MAX_SIZE_BYTES);
                throw new Exception(errMsg);
            }

            this.batchBody = newBatchBody;
            this.numMsgs++;
        }

        /**
         * Returns the current batch body as a UTF-8 encoded byte array.
         *
         * @return the current batch body as a UTF-8 encoded byte array.
         */
        public override byte[] GetBody()
        {
            // Codes_SRS_HTTPSBATCHMESSAGE_11_006: [The function shall return the current batch message body.]
            // Codes_SRS_HTTPSBATCHMESSAGE_11_007: [The batch message body shall be encoded using UTF-8.]
            return BATCH_CHARSET.GetBytes(this.batchBody);
        }

        /**
         * Returns the message content-type as 'application/vnd.microsoft.iothub.json'.
         *
         * @return the message content-type as 'application/vnd.microsoft.iothub.json'.
         */
        public override String GetContentType()
        {
            // Codes_SRS_HTTPSBATCHMESSAGE_11_011: [The function shall return 'application/vnd.microsoft.iothub.json'.]
            return HTTPS_BATCH_CONTENT_TYPE;
        }

        /**
         * Returns an empty list of properties for the batched message.
         *
         * @return an empty list of properties for the batched message.
         */
        public override MessageProperty[] GetProperties()
        {
            // Codes_SRS_HTTPSBATCHMESSAGE_11_012: [The function shall return an empty array.]
            return new MessageProperty[0];
        }

        /**
         * Returns the number of messages currently in the batch.
         *
         * @return the number of messages currently in the batch.
         */
        public int numMessages()
        {
            // Codes_SRS_HTTPSBATCHMESSAGE_11_010: [The function shall return the number of messages currently in the batch.]
            return this.numMsgs;
        }

        /**
         * Converts a service-bound message to a JSON object with the correct
         * format.
         *
         * @param msg the message to be converted to a corresponding JSON object.
         *
         * @return the JSON string representation of the message.
         */
        protected static String msgToJson(HttpsSingleMessage msg)
        {
            StringBuilder jsonMsg = new StringBuilder("{");
            // Codes_SRS_HTTPSBATCHMESSAGE_11_003: [The JSON object shall have the field "body" set to the raw message.]
            jsonMsg.Append("\"body\":");
            jsonMsg.Append("\"" + msg.getBodyAsString() + "\",");
            // Codes_SRS_HTTPSBATCHMESSAGE_11_004: [The JSON object shall have the field "base64Encoded" set to whether the raw message was Base64-encoded.]
            jsonMsg.Append("\"base64Encoded\":");
            jsonMsg.Append(msg.isBase64Encoded().ToString().ToLower());
            // Codes_SRS_HTTPSBATCHMESSAGE_11_005: [The JSON object shall have the field "properties" set to a JSON object which has the field "content-type" set to the content type of the raw message.]
            MessageProperty[] properties = msg.GetProperties();
            int numProperties = properties.Length;
            if (numProperties > 0)
            {
                jsonMsg.Append(",");
                jsonMsg.Append("\"properties\":");
                jsonMsg.Append("{");
                for (int i = 0; i < numProperties - 1; ++i)
                {
                    MessageProperty property = properties[i];
                    jsonMsg.Append("\"" + property.getName() + "\":");
                    jsonMsg.Append("\"" + property.getValue() + "\",");
                }
                if (numProperties > 0)
                {
                    MessageProperty property = properties[numProperties - 1];
                    jsonMsg.Append("\"" + property.getName() + "\":");
                    jsonMsg.Append("\"" + property.getValue() + "\"");
                }
                jsonMsg.Append("}");
            }
            jsonMsg.Append("}");

            return jsonMsg.ToString();
        }

        /**
         * Adds a JSON object to a JSON array.
         *
         * @param jsonObj the object to be added to the JSON array.
         * @param jsonArray the JSON array.
         *
         * @return the JSON string representation of the JSON array with the object
         * added.
         */
        protected static String addJsonObjToArray(String jsonObj, String jsonArray)
        {
            if (jsonArray == "[]")
            {
                return "[" + jsonObj + "]";
            }

            // removes the closing brace of the JSON array.
            String openJsonArray = jsonArray.Substring(0, jsonArray.Length - 1);

            return openJsonArray + "," + jsonObj + "]";
        }
    }
}
