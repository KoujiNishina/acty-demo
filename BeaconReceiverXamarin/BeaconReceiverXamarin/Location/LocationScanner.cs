using BeaconReceiverConnectorXamarin.Utils;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace BeaconReceiverXamarin.Location
{
    /**
     * Created by ryoma.saito on 16/08/18.
     */
    public class LocationScanner
    {
        private const String TAG = "LocationScanner";
        /** 位置情報保管用 */
        private LocationCache mLocationCache;

        /**
         * コンストラクタ
         * @param context
         */
        public LocationScanner()
        {
            CrossGeolocator.Current.PositionChanged += PositionChanged;
            CrossGeolocator.Current.PositionError += PositionError;

            mLocationCache = LocationCache.getInstance();
        }
        public bool IsEnabled
        {
            get
            {
                return CrossGeolocator.Current.IsGeolocationEnabled && CrossGeolocator.Current.IsGeolocationAvailable;
            }
        }

        /** 位置情報取得開始 */
        public async Task<bool> startScan()
        {
            if (CrossGeolocator.Current.IsListening)
                return false;

            var ret = await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(0), 0, true, new ListenerSettings
            {
                PauseLocationUpdatesAutomatically = false,
                AllowBackgroundUpdates = true
            });

            //mLocationManager.requestLocationUpdates(mProvider, 0, 0, this);
            DebugMessageUtils.GetInstance().WriteLog(TAG, "startScan", LogLevel.I);
            return ret;
        }

        /** 位置情報取得終了 */
        public async Task<bool> stopScan()
        {
            if (!CrossGeolocator.Current.IsListening)
                return true;

            var ret = await CrossGeolocator.Current.StopListeningAsync();

            CrossGeolocator.Current.PositionChanged -= PositionChanged;
            CrossGeolocator.Current.PositionError -= PositionError;
            DebugMessageUtils.GetInstance().WriteLog(TAG, "stopScan", LogLevel.I);
            return ret;
        }

        private void PositionChanged(object sender, PositionEventArgs e)
        {
            var lat = e.Position.Latitude;//緯度
            var lon = e.Position.Longitude;//経度

            mLocationCache.setLocation(lat, lon);
        }
        private void PositionError(object sender, PositionErrorEventArgs e)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "PositionError Error:" + e.Error, LogLevel.W);
        }
    }
}
