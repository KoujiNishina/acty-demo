using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverConnectorXamarin.CustomerFront.Data
{
    /**
     * Web設定レスポンスデータのベース
     * <p>カスタマーフロントのWeb設定レスポンス仕様に合わせて、本クラスをextendsする。
     * 本クラスではJSON文字列のみをサポートしている。
     * </p>
     * <p>カスタマーフロントのWeb設定JSONデータの仕様（サーバー側実装）例：
     * </p>
     * <pre class="prettyprint">
     * {
     *     "failover": 3,
     *     "failback": 30,
     *     "allowed_min_rssi": -40,
     *     "customer_front_url": "https://xxxxx.yyyy.zzz.jp",
     *     "customer_front_auth_info": {
     *         "id": "myid",
     *         "password": "mypassword"
     *     }
     * }
     * </pre>
     * <p>上記の仕様に対する、Web設定レスポンスデータの実装は、以下のように行う。
     * </p>
     * <pre class="prettyprint">
     * class CustomerFrontAuthInfo {
     *
     *     public String id = null;
     *     public String password = null;
     * }
     * class SampleWebSettings extends WebSettingsBase {
     *
     *     public Integer failover = null;
     *     public Integer failback = null;
     *     public Integer allowed_min_rssi = null;
     *     public String customer_front_url = null;
     *     public CustomerFrontAuthInfo customer_front_auth_info = new CustomerFrontAuthInfo();
     * }
     * </pre>
     */
    public class WebSettingsBase
    {
        public int httpStatusCode;
    }
}
