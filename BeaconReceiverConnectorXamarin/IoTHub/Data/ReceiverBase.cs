using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverConnectorXamarin.IoTHub.Data
{
    /**
     * 受信機（レシーバー）情報のベース
     * <p>本クラスは、接触データのベースクラス{@link TouchParamBase}の要素である。</p>
     */
    public class ReceiverBase
    {

        /**
         * カスタマーフロントから取得するユニーク識別子
         */
        public String name = null;
        /**
         * 受信機のニックネーム
         */
        public String nickname = null;
    }
}
