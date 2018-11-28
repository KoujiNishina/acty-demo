using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverConnectorXamarin.IoTHub.Data
{
    /**
     * 位置情報のベース
     * <p>本クラスは、接触データのベースクラス{@link TouchParamBase}の要素である。</p>
     */
    public class LocationBase
    {

        /**
         * 	ビーコンから電波を受信した時の緯度
         */
        public double? latitude = null;
        /**
         * ビーコンから電波を受信した時の経度
         */
        public double? longitude = null;
        /**
         * 緯度・経度が確定した日時
         */
        public String fixed_at = null;
    }
}
