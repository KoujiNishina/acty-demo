using IoTHubJavaClientRewrittenInDotNet.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverXamarin.Location
{
    /**
     * Created by ryoma.saito on 16/08/18.
     */
    public class LocationCache
    {
        /** Instance of LocationCache */
        private static LocationCache mInstance = new LocationCache();
        /** 緯度 */
        private double mLat = 0;
        /** 経度 */
        private double mLon = 0;
        /** 最終測位日時 */
        //private String mDate = DateTimeUtils.GetCurrentTimeMillis().ToString();
        private String mDate = DateTimeUtils.GetCurrentTimeString();

        /**
         *
         * @return
         */
        public static LocationCache getInstance()
        {
            return mInstance;
        }

        /**
         *
         * @param lat
         * @param lon
         */
        public void setLocation(double lat, double lon)
        {
            lock (this)
            {
                mLat = lat;
                mLon = lon;
                //mDate = DateTimeUtils.GetCurrentTimeMillis().ToString();
                mDate = DateTimeUtils.GetCurrentTimeString();
            }
        }

        /**
         *
         * returnは jp.co.nttpc.surechigai.receiver.demo.info.Location です.
         * (not java.lang.Object.Location)
         *
         * @return
         */
        public Data.Location getLocation()
        {
            lock (this)
            {
                Data.Location location = new Data.Location();
                location.latitude = mLat;
                location.longitude = mLon;
                location.fixed_at = mDate;
                return location;
            }
        }
    }
}
