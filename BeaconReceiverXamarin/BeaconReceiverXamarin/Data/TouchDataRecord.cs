using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverXamarin.Data
{
    public class TouchDataRecord
    {
        [PrimaryKey, AutoIncrement]
        public int _id { get; set; }
        [NotNull]
        public string key_name { get; set; }
        public string uuid { get; set; }
        public int major { get; set; }
        public int minor { get; set; }
        public string nickname { get; set; }
        public int measured_power { get; set; }
        public long recv_beacon_date { get; set; }
        public double? latitude { get; set; }
        public double? lon { get; set; }
        public long? recv_location_date { get; set; }
        public int rssi { get; set; }
    }
}
