using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverConnectorXamarin.IoTHub.Data
{
    /**
     * ビーコン情報のベース.
     * <p>本クラスは、接触データのベースクラス{@link TouchParamBase}の要素である。</p>
     */
    public class BeaconBase
    {

        /**
         * ビーコン名
         */
        public String name = null;
        /**
         * ビーコンから通知されたUUID
         */
        public String uuid = null;
        /**
         * ビーコンから通知されたMajor
         */
        public int? major = null;
        /**
         * ビーコンから通知されたMinor
         */
        public int? minor = null;
        /**
         * ビーコンのニックネーム
         */
        public String nickname = null;
        /**
         * ビーコンのメジャードパワー
         */
        public int? measured_power = null;
        /**
         * ビーコンのバッテリー残量
         */
        public double? battery = null;
        /**
         * ビーコンの周囲温度
         */
        public double? temperature = null;
        /**
         * ビーコンが起動してからのアドバタイズの回数
         */
        public int? adv_count = null;
        /**
         * ビーコンが起動してからの経過時間
         */
        public double? uptime = null;
    }
}
