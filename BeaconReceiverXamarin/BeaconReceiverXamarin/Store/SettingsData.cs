using BeaconReceiverConnectorXamarin.CustomerFront.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverXamarin.Store
{
    /// <summary>
    /// 設定ファイル用Info
    /// </summary>
    public class SettingsData : WebSettingsBase
    {
        /** 稼働IoTHubのURL */
        public String active_host { get; set; }
        /** 待機IoTHubのURL */
        public String standby_host { get; set; }
        /** IoTHub認証情報 */
        public IothubAuthInfo iothub_auth_info { get; set; }
        /** 分析間隔 */
        public int? analysis_interval { get; set; }
        /** 送信間隔 */
        public int? send_interval { get; set; }
        /** フェイルオーバー条件 */
        public int? failover { get; set; }
        /** フェイルバック条件 */
        public int? failback { get; set; }
        /** 受信許可電波強度 */
        public int? allowed_min_rssi { get; set; }
        /** Web設定ファイルダウンロードURL */
        public String web_settings_url { get; set; }
        /** カスタマーフロント送信先 */
        public String customer_front_url { get; set; }
        /** カスタマーフロント認証情報 */
        public CustomerFrontAuthInfo customer_front_auth_info { get; set; }
        /** UUID対象ホワイトリスト */
        public List<String> uuid_white_list { get; set; }
        /** 検索条件とする受信機識別子 */
        public String receiver_nickname { get; set; }

        public SettingsData()
        {
            active_host = null;
            standby_host = null;
            iothub_auth_info = new IothubAuthInfo();
            analysis_interval = null;
            send_interval = null;
            failover = null;
            failback = null;
            allowed_min_rssi = null;
            web_settings_url = null;
            customer_front_url = null;
            customer_front_auth_info = new CustomerFrontAuthInfo();
            uuid_white_list = null;
            receiver_nickname = null;
        }
    }
}
