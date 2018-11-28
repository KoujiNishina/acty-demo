using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverConnectorXamarin.IoTHub.Data
{
    /**
     * 接触データのベース.
     *
     * <p>本クラスでは、接触データの情報を規定している。
     * </p>
     * <p>本クラスをextendsして、任意の接触情報を追加する事が出来る。</p>
     * <p>サンプル実装例：</p>
     * <pre class="prettyprint">
     * class SampleTouchParam extends TouchParamBase {
     *     // 追加する接触情報
     *     String sample_extra_data = null;
     * }
     *         ・
     *         ・
     * TouchParamBase sampleTouch = new SampleTouchParam();
     * sampleTouch.beacon.uuid = "00000000-0123-0000-0000-000000000000";
     * sampleTouch.beacon.major = 1;
     * sampleTouch.beacon.minor = 2;
     * sampleTouch.beacon.name = "samplebeacon";
     *         ・
     *         ・
     * // 追加する接触情報
     * ((SampleTouchParam)sampleTouch).sample_extra_data = "sampleextradata";
     *
     * List＜TouchParamBase＞ touchList = new ArrayList＜＞();
     * touchList.add(sampleTouch);
     * </pre>
     * <p>上記のサンプル実装 touchListを{@link jp.co.nttpc.surechigai.receiver.connector.iothub.DeviceToCloudGateway#send(List, SendCallback) DeviceToCloudGatewayクラスのsendメソッド}
     * のパラメーターとして利用すると、以下のJSONキーと値のペアが本クラスのメンバーに追加される。
     * </p>
     * <pre class="prettyprint">
     *   "sample_extra_data": "sampleextradata"
     * </pre>
     */
    public class TouchParamBase
    {

        /**
         * 受信機（レシーバー）情報
         */
        public ReceiverBase receiver;
        /**
         * ビーコン情報
         */
        public BeaconBase beacon;
        /**
         * 位置情報
         */
        public LocationBase location;
        /**
         * 接触データの種別
         */
        public String code = null;
        /**
         * BLEの受信電波強度
         */
        public int? rssi = null;
        /**
         * rssiから計算したビーコンとレシーバーの推定距離[m]
         */
        public double? distance = null;
        /**
         * 接触データを受信した時の日時
         */
        public String occurred_at = null;
        /**
         * オプション
         */
        public String options = null;

        public TouchParamBase()
        {

            receiver = new ReceiverBase();
            beacon = new BeaconBase();
            location = new LocationBase();
        }
    }
}
