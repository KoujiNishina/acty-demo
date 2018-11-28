using BeaconReceiverConnectorXamarin.IoTHub.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverXamarin.Data
{
    /**
     * ビーコン情報
     *
     * Created by chiho.adachi on 2016/08/09.
     */
    public class Beacon : BeaconBase
    {
        /**
         * コンストラクタ
         * TouchData からの参照
         */
        public Beacon() : base()
        {
        }

        /**
            * コンストラクタ
            * @param manufacturerSpecificData
            */
        public Beacon(byte[] manufacturerSpecificData) : base()
        {
            if (manufacturerSpecificData.Length >= 25)
            {
                String id = String.Format("{0,0:x2}{1,0:x2}{2,0:x2}{3,0:x2}-" +
                    "{4,0:x2}{5,0:x2}-" +
                    "{6,0:x2}{7,0:x2}-" +
                    "{8,0:x2}{9,0:x2}-" +
                    "{10,0:x2}{11,0:x2}{12,0:x2}{13,0:x2}{14,0:x2}{15,0:x2}",
                    manufacturerSpecificData[4], manufacturerSpecificData[5], manufacturerSpecificData[6], manufacturerSpecificData[7],
                    manufacturerSpecificData[8], manufacturerSpecificData[9],
                    manufacturerSpecificData[10], manufacturerSpecificData[11],
                    manufacturerSpecificData[12], manufacturerSpecificData[13],
                    manufacturerSpecificData[14], manufacturerSpecificData[15], manufacturerSpecificData[16], manufacturerSpecificData[17], manufacturerSpecificData[18], manufacturerSpecificData[19]);

                this.major = !BitConverter.IsLittleEndian ? (manufacturerSpecificData[21] & 0xff) << 8 | (manufacturerSpecificData[20] & 0xff) : (manufacturerSpecificData[20] & 0xff) << 8 | (manufacturerSpecificData[21] & 0xff);
                this.minor = !BitConverter.IsLittleEndian ? (manufacturerSpecificData[23] & 0xff) << 8 | (manufacturerSpecificData[22] & 0xff) : (manufacturerSpecificData[22] & 0xff) << 8 | (manufacturerSpecificData[23] & 0xff);
                this.measured_power = (manufacturerSpecificData[24] & 0b10000000) == 0b10000000 ? (manufacturerSpecificData[24] - 256) : manufacturerSpecificData[24];
                this.uuid = Guid.Parse(id).ToString();
                this.nickname = null;
            }
        }
    }
}
