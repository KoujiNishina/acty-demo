// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Rewritten in C#.net (originally in Java) by NTT PC Communications.

using System;
using System.Collections.Generic;
using System.Text;

namespace IoTHubJavaClientRewrittenInDotNet.Auth
{
    /** Grants device access to an IoT Hub for the specified amount of time. */
    public class IotHubSasToken
    {
        /**
         * The SAS token format. The parameters to be interpolated are, in order:
         * the signature, the expiry time, the key name (device ID), and the
         * resource URI.
         */
        public const String TOKEN_FORMAT = "SharedAccessSignature sig={0}&se={1}&sr={2}";

        /** Components of the SAS token. */
        protected String signature;
        /** The time, as a UNIX timestamp, before which the token is valid. */
        protected long expiryTime;
        /**
         * The URI for a connection from a device to an IoT Hub. Does not include a
         * protocol.
         */
        protected String scope;

    /**
     * Constructor. Generates a SAS token that grants access to an IoT Hub for
     * the specified amount of time.
     *
     * @param scope the resource URI.
     * @param key the device key.
     * @param expiryTime the time, as a UNIX timestamp, after which the token
     * will become invalid.
     */
    public IotHubSasToken(String scope, String key, long expiryTime)
        {
            // Tests_SRS_IOTHUBSASTOKEN_11_002: [**The constructor shall save all input parameters to member variables.**]
            this.scope = scope;
            this.expiryTime = expiryTime;

            // Codes_SRS_IOTHUBSASTOKEN_11_005: [The signature shall be correctly computed and set.]
            Signature sig = new Signature(this.scope, this.expiryTime, key);
            this.signature = sig.ToString();
        }

        /**
         * Returns the string representation of the SAS token.
         *
         * @return the string representation of the SAS token.
         */
        public override String ToString()
        {
            // Tests_SRS_IOTHUBSASTOKEN_11_006: [The function shall return the string representation of the SAS token.]
            return buildSasToken();
        }

        protected String buildSasToken()
        {
            // Codes_SRS_IOTHUBSASTOKEN_11_001: [The SAS token shall have the format "SharedAccessSignature sig=<signature >&se=<expiryTime>&sr=<resourceURI>". The params can be in any order.]
            return String.Format(TOKEN_FORMAT, this.signature, this.expiryTime, this.scope);
        }

        protected IotHubSasToken()
        {
            this.signature = null;
            this.expiryTime = 0l;
            this.scope = null;
        }
    }
}
