using System;
using System.Collections.Generic;
using System.Text;

namespace IoTHubJavaClientRewrittenInDotNet.Util
{
    public class DateTimeUtils
    {
        private static readonly DateTime Jan1st1970 = new DateTime
                (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long GetCurrentTimeMillis()
        {
            return GetTimeMillisFrom1970(DateTime.UtcNow);
        }
        public static string GetCurrentTimeString()
        {
            return ConvertIsoDatetimeTimeZone(DateTime.UtcNow);
        }
        public static long GetTimeMillisFrom1970(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - Jan1st1970).TotalMilliseconds;
        }
        public static DateTime GetDateTimeFromMillis(long millis)
        {
            return Jan1st1970.AddMilliseconds(millis);
        }
        /**
         * ISO 8601 formatter for date-time with time zone.
         * The format used is {@code yyyy-MM-dd'T'HH:mm:ssZZ}.
         */
        public const string ISO_DATETIME_TIME_ZONE_FORMAT = "yyyy-MM-ddTHH:mm:sszzz";
        /**
         * 日付文字列(yyyy-MM-dd'T'HH:mm:ssZZ)へ変換
         *
         * @param dateMillis
         * @return
         */
        public static String ConvertIsoDatetimeTimeZone(long dateMillis)
        {
            return ConvertIsoDatetimeTimeZone(GetDateTimeFromMillis(dateMillis));
        }
        public static String ConvertIsoDatetimeTimeZone(DateTime dateTime)
        {
            return dateTime.ToLocalTime().ToString(ISO_DATETIME_TIME_ZONE_FORMAT);
        }
        public static long ConvertIsoDatetimeTimeZone(string dateString)
        {
            DateTime parsedDateTime;
            DateTime.TryParseExact(dateString, ISO_DATETIME_TIME_ZONE_FORMAT, null, System.Globalization.DateTimeStyles.None, out parsedDateTime);
            return GetTimeMillisFrom1970(parsedDateTime);
        }
    }
}
