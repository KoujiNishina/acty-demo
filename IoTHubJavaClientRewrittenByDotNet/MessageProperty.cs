// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Rewritten in C#.net (originally in Java) by NTT PC Communications.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace IoTHubJavaClientRewrittenInDotNet
{
    /** An IoT Hub message property. */
    public class MessageProperty
    {
        /**
         * A set of reserved property names. The reserved property names are
         * interpreted in a meaningful way by the device and the IoT Hub.
         */
        public static HashSet<String> RESERVED_PROPERTY_NAMES;

        static MessageProperty()
        {
            HashSet<String> reservedPropertyNames = new HashSet<String>();
            reservedPropertyNames.Add("message-id");
            reservedPropertyNames.Add("iothub-enqueuedtime");
            reservedPropertyNames.Add("iothub-messagelocktoken");
            reservedPropertyNames.Add("iothub-sequencenumber");
            reservedPropertyNames.Add("to");
            reservedPropertyNames.Add("absolute-expiry-time");
            reservedPropertyNames.Add("correlation-id");
            reservedPropertyNames.Add("user-id");
            reservedPropertyNames.Add("iothub-operation");
            reservedPropertyNames.Add("iothub-partition-key");
            reservedPropertyNames.Add("iothub-ack");
            reservedPropertyNames.Add("iothub-connection-device-id");
            reservedPropertyNames.Add("iothub-connection-auth-method");
            reservedPropertyNames.Add("iothub-connection-auth-generation-id");
            reservedPropertyNames.Add("content-type");
            reservedPropertyNames.Add("content-encoding");

            RESERVED_PROPERTY_NAMES = reservedPropertyNames;
        }

        /** The property name. */
        protected String name;
        /** The property value. */
        protected String value;

        /**
         * Constructor.
         *
         * @param name The IoT Hub message property name.
         * @param value The IoT Hub message property value.
         *
         * @throws IllegalArgumentException if the name and value constitute an
         * invalid IoT Hub message property. A message property can only contain
         * US-ASCII printable chars, with some exceptions as specified in RFC 2047.
         * A message property name cannot be one of the reserved property names.
         */
        public MessageProperty(String name, String value)
        {
            if (name == null)
            {
                throw new ArgumentException("Property argument 'name' cannot be null.");
            }

            if (value == null)
            {
                throw new ArgumentException("Property argument 'value' cannot be null.");
            }

            // Codes_SRS_MESSAGEPROPERTY_11_002: [If the name contains a character that is not in US-ASCII printable characters or is one of: ()<>@,;:\"/[]?={} (space) (horizontal tab), the function shall throw an IllegalArgumentException.]
            if (!usesValidChars(name))
            {
                String errMsg = String.Format("{0} is not a valid IoT Hub message property name.\n", name);
                throw new ArgumentException(errMsg);
            }

            // Codes_SRS_MESSAGEPROPERTY_11_008: [If the name is a reserved property name, the function shall throw an IllegalArgumentException.]
            if (RESERVED_PROPERTY_NAMES.Contains(name))
            {
                String errMsg = String.Format("{0} is a reserved IoT Hub message property name.\n", name);
                throw new ArgumentException(errMsg);
            }

            // Codes_SRS_MESSAGEPROPERTY_11_003: [If the value contains a character that is not in US-ASCII printable characters or is one of: ()<>@,;:\"/[]?={} (space) (horizontal tab), the function shall throw an IllegalArgumentException.]
            if (!usesValidChars(value))
            {
                String errMsg = String.Format("%s is not a valid IoT Hub message property value.\n", value);
                throw new ArgumentException(errMsg);
            }

            // Codes_SRS_MESSAGEPROPERTY_11_001: [The constructor shall save the property name and value.]
            this.name = name;
            this.value = value;
        }

        /**
         * Returns the property name.
         *
         * @return the property name.
         */
        public String getName()
        {
            // Codes_SRS_MESSAGEPROPERTY_11_004: [The function shall return the property name.]
            return this.name;
        }

        /**
         * Returns the property value.
         *
         * @return the property value.
         */
        public String getValue()
        {
            // Codes_SRS_MESSAGEPROPERTY_11_005: [The function shall return the property value.]
            return this.value;
        }

        /**
         * Equivalent to property.getName().equalsIgnoreCase(name).
         *
         * @param name the property name.
         *
         * @return true if the given name is the property name.
         */
        public bool hasSameName(String name)
        {
            bool nameMatches = false;

            // Codes_SRS_MESSAGEPROPERTY_11_006: [The function shall return true if and only if the property has the given name, where the names are compared in a case-insensitive manner.]
            if (this.getName().ToLower() == name.ToLower())
            {
                nameMatches = true;
            }

            return nameMatches;
        }

        /**
         * Returns whether the property is a valid application property. The
         * property is valid if it is not one of the reserved properties, only uses
         * US-ASCII printable chars, and does not contain: <code>()&lt;&gt;@,;:\"/[]?={}</code> (space)
         * (horizontal tab).
         *
         * @param name the property name.
         * @param value the property value.
         *
         * @return whether the property is a valid application property.
         */
        public static bool isValidAppProperty(String name, String value)
        {
            bool propertyIsValid = false;

            // Codes_SRS_MESSAGEPROPERTY_11_007: [The function shall return true if and only if the name and value only use characters in: US-ASCII printable characters, excluding ()<>@,;:\"/[]?={} (space) (horizontal tab), and the name is not a reserved property name.]
            if (!RESERVED_PROPERTY_NAMES.Contains(name)
                    && usesValidChars(name)
                    && usesValidChars(value))
            {
                propertyIsValid = true;
            }

            return propertyIsValid;
        }

        /**
         * Returns true if the string only uses US-ASCII printable chars and does
         * not contain: <code>()&lt;&gt;@,;:\"/[]?={}</code> (space) (horizontal tab)
         *
         * @param s the string.
         *
         * @return whether the string only uses US-ASCII printable chars and does
         * not contain: <code>()&lt;&gt;@,;:\"/[]?={}</code> (space) (horizontal tab)
         */
        protected static bool usesValidChars(String s)
        {
            bool isValid = false;

            if (Regex.IsMatch(s, "^[\\P{Cc}]+$")
                    && Regex.IsMatch(s,
                    "^[^()<>@,;:\\\\\"/\\[\\]\\?=\\{\\}\u0040\u0011]+$"))
            {
                isValid = true;
            }

            return isValid;
        }

        protected MessageProperty()
        {
            this.name = null;
            this.value = null;
        }
    }
}
