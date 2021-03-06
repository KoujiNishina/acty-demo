﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Rewritten in C#.net (originally in Java) by NTT PC Communications.

using System;
using System.Collections.Generic;
using System.Text;

namespace IoTHubJavaClientRewrittenInDotNet
{
    /**
     * A return value from a message callback that instructs an IoT Hub to
     * complete, abandon, or reject the message.
     */
    public enum IotHubMessageResult
    {
        COMPLETE, ABANDON, REJECT
    }
}
