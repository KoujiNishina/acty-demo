// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Rewritten in C#.net (originally in Java) by NTT PC Communications.

using System;
using System.Collections.Generic;
using System.Text;

namespace IoTHubJavaClientRewrittenInDotNet.Transport.Https
{
    /**
     * An HTTPS message. An HTTPS message is distinguished from a plain IoT Hub
     * message by its property names, which are prefixed with 'iothub-app-';
     * and by the explicit specification of a content-type.
     */
    public abstract class HttpsMessage
    {
        /** The prefix to be added to an HTTPS application-defined property. */
        protected static String HTTPS_APP_PROPERTY_PREFIX = "iothub-app-";

        /**
         * Gets the message body.
         * @return Returns the message body.
         */
        public abstract byte[] GetBody();

        /** Gets the message content type.
         * @return Returns the message content-type. */
        public abstract String GetContentType();

        /**Gets the collection of message properties.
         * @return Returns the message properties. */
        public abstract MessageProperty[] GetProperties();
    }
}
