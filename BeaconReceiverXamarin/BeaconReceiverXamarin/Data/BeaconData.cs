using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverXamarin.Data
{
    /**
     * ビーコンからの受信データ格納Info
     *
     * Created by chiho.adachi on 2016/08/09.
     */
    public class BeaconData
    {

        /** ビーコン情報コンテナ */
        public Beacon beacon;
        /** イベント発生位置情報 */
        public Location location;
        /** 受信電波強度 */
        public int? rssi;
        /** イベント発生日時 */
        public String occurred_date;

        public BeaconData(Beacon beacon, Location location, int? rssi, String occurred_date)
        {
            this.beacon = beacon;
            this.location = location;
            this.rssi = rssi;
            this.occurred_date = occurred_date;
        }

    }
}
