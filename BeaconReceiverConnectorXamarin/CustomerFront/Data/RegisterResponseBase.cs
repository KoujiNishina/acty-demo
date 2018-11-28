using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverConnectorXamarin.CustomerFront.Data
{
    /**
     * 受信機登録レスポンスデータのベース.
     * <p>カスタマーフロントの受信機登録レスポンス仕様に合わせて、本クラスをextendsする。
     * 本クラスではJSON文字列のみをサポートしている。
     * </p>
     * <p>カスタマーフロントの受信機登録JSONデータの仕様（サーバー側実装）例：
     * </p>
     * <pre class="prettyprint">
     * {
     *     "active_host": "xxxxx-ih01.azure-devices.net",
     *     "standby_host": "xxxxx-ih02.azure-devices.net",
     *     "name": "sampleDevice001",
     *     "password": "Ajl+dalihgaoihfvhHFuahfklajfoifahoihacalkADF"
     * }
     * </pre>
     * <p>上記の仕様に対する、レスポンスデータの実装は、以下のように行う。
     * </p>
     * <pre class="prettyprint">
     * public class SampleRegisterResponse extends RegisterResponseBase {
     *
     *     public String name = null;
     *     public String password = null;
     *     public String active_host = null;
     *     public String standby_host = null;
     * }
     * </pre>
     */
    public class RegisterResponseBase
    {
        public int httpStatusCode;
    }
}
