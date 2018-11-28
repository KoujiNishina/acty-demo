using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverConnectorXamarin.CustomerFront.Data
{
    /**
     * 受信機登録リクエスト情報のベース.
     *
     * <p>本クラスをextendsして、任意のリクエスト情報を設定する。</p>
     * <p>サンプル実装例：</p>
     * <pre class="prettyprint">
     * public class SampleRegisterParam extends RegisterParamBase {
     *
     *     public String name = null;
     *     public Integer type = null;
     * }
     *         ・
     *         ・
     * SampleRegisterParam registerParam = new SampleRegisterParam();
     * registerParam.name = "register001";
     * registerParam.type = 1;
     * </pre>
     * <p>上記のサンプル実装 registerParamを{@link jp.co.nttpc.surechigai.receiver.connector.customerfront.CustomerFrontGateway#register(String, RegisterParamBase, Class, RegisterCallback) CustomerFrontGatewayクラスのregisterメソッド}
     * のパラメーターとして利用すると、以下のJSON文字列がリクエストボディデータとして生成される。</p>
     * <pre class="prettyprint">
     * {
     *     "name": "register001",
     *     "type": 1
     * }
     * </pre>
     */
    public class RegisterParamBase
    {
    }
}
