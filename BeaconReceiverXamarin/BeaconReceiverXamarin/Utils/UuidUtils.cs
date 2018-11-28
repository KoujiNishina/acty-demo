using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BeaconReceiverXamarin.Utils
{
    public class UuidUtils
    {
        //public static Regex UUID_WITH_OR_WITHOUT_MAJOR_AND_MINOR = new Regex("^(?<uuid>[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}?)(?<major>-[0-9]{1,5}?)?(?<minor>-[0-9]{1,5}?)?$");
        public static Regex UUID_WITH_OR_WITHOUT_MAJOR_AND_MINOR = new Regex("^[0-9a-f-]+$");
        public static bool IsValidUuid(string uuid)
        {
            var match = UUID_WITH_OR_WITHOUT_MAJOR_AND_MINOR.Match(uuid);
            if (!match.Success)
            {
                return false;
            }
            //var sUuid = match.Groups["uuid"].Value;
            //try
            //{
            //    Guid.Parse(sUuid);
            //}
            //catch (FormatException ex)
            //{
            //    return false;
            //}
            return true;
        }
        public static int[] GetMajorAndMinor(string uuid)
        {
            int[] aMajorAndMinor = { -1, -1 };
            
            if (uuid.Length <= 36)  //major,minorなし
            {
                return aMajorAndMinor;
            }
            var sMajorAndMinor = uuid.Substring(36 + 1);  //UUID本体部とその後ろの'-'を除く

            string[] saMajorAndMinor = sMajorAndMinor.Split('-');
            if (saMajorAndMinor.Length >= 2) //minorあり
            {
                int.TryParse(saMajorAndMinor[0], out aMajorAndMinor[0]);
                int.TryParse(saMajorAndMinor[1], out aMajorAndMinor[1]);
            }
            else //majorのみ
            {
                int.TryParse(sMajorAndMinor, out aMajorAndMinor[0]);
            }
            return aMajorAndMinor;
        }
    }
}
