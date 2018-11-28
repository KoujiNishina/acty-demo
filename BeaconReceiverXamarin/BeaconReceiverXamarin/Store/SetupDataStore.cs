using BeaconReceiverXamarin.Constants;
using BeaconReceiverXamarin.Interface;
using BeaconReceiverConnectorXamarin.Interface;
using BeaconReceiverXamarin.Resource;
using BeaconReceiverConnectorXamarin.Utils;
using Newtonsoft.Json;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace BeaconReceiverXamarin.Store
{
    /// <summary>
    /// 設定に関するクラス
    /// </summary>
    public class SetupDataStore
    {
        const string TAG = "SetupDataStore";
        public SetupDataStore()
        {
        }

        /// <summary>
        /// 設定ファイルからデータを読み込む
        /// </summary>
        /// <returns>SettingsData</returns>

        public static SettingsData getSettingsDataFromPreferences()
        {
            ISettings settings = CrossSettings.Current;
            SettingsData data = new SettingsData();

            //  ファイルの内容をセット
            data.active_host = settings.GetValueOrDefault(AppResource.setting_active_host_key, null);
            data.standby_host = settings.GetValueOrDefault(AppResource.setting_standby_host_key, null);

            IothubAuthInfo iothubAuthInfo = new IothubAuthInfo();
            iothubAuthInfo.name = settings.GetValueOrDefault(AppResource.setting_iothub_auth_info_name_key, null);
            iothubAuthInfo.password = settings.GetValueOrDefault(AppResource.setting_iothub_auth_info_password_key, null);
            data.iothub_auth_info = iothubAuthInfo;

            data.analysis_interval = 
                    GetInt(settings, AppResource.setting_analysis_interval_key, int.Parse(AppResource.analysis_interval_min));
            data.send_interval = GetInt(settings, AppResource.setting_send_interval_key, int.Parse(AppResource.send_interval_min));
            data.failover = GetInt(settings, AppResource.setting_failover_key, 0);
            data.failback = GetInt(settings, AppResource.setting_failback_key, 0);
            data.allowed_min_rssi = GetInt(settings, AppResource.setting_allowed_min_rssi_key, int.Parse(AppResource.allowed_min_rssi_min));
            data.web_settings_url = settings.GetValueOrDefault(AppResource.setting_web_settings_url_key, null);
            data.customer_front_url = settings.GetValueOrDefault(AppResource.setting_customer_front_url_key, null);

            CustomerFrontAuthInfo customerFrontAuthInfo = new CustomerFrontAuthInfo();
            customerFrontAuthInfo.id = settings.GetValueOrDefault(AppResource.setting_customer_front_auth_info_id_key, null);
            customerFrontAuthInfo.password = settings.GetValueOrDefault(AppResource.setting_customer_front_auth_info_password_key, null);
            data.customer_front_auth_info = customerFrontAuthInfo;
            var uuid_white_list = getUuidWhiteListRaw();
            data.uuid_white_list = string.IsNullOrEmpty(uuid_white_list) ? new List<string>() : new List<string>(uuid_white_list.Split(","[0]));

            data.receiver_nickname = settings.GetValueOrDefault(AppResource.setting_receiver_nickname_key, null);

            DebugMessageUtils.GetInstance().WriteLog(TAG, "getSettingsDataFromPreferences data.analysis_interval:" + data.analysis_interval
                + " data.allowed_min_rssi:" + data.allowed_min_rssi
                + " data.uuid_white_list:" + uuid_white_list, LogLevel.D);
            return data;
        }

        /// <summary>
        /// 設定ファイル更新（Web設定）
        /// </summary>
        /// <param name="data">設定データ ※nullの場合はローカル更新とする</param>
        public static void updateWebSettingsPreferences(SettingsData data)
        {

            ISettings settings = CrossSettings.Current;
            // Web設定ローカル更新の場合
            if (data == null)
            {
                // ローカルのJSON設定ファイルからデータ取得
                data = getLocalSettings();
            }

            // データがない場合は処理中断
            if (data == null)
            {
                return;
            }

            editorPut(settings, AppResource.setting_active_host_key, data.active_host);
            editorPut(settings, AppResource.setting_standby_host_key, data.standby_host);
            editorPut(settings, AppResource.setting_iothub_auth_info_name_key, data.iothub_auth_info.name);
            editorPut(settings, AppResource.setting_iothub_auth_info_password_key, data.iothub_auth_info.password);
            editorPut(settings, AppResource.setting_analysis_interval_key, data.analysis_interval);
            editorPut(settings, AppResource.setting_send_interval_key, data.send_interval);
            editorPut(settings, AppResource.setting_failover_key, data.failover);
            editorPut(settings, AppResource.setting_failback_key, data.failback);
            editorPut(settings, AppResource.setting_allowed_min_rssi_key, data.allowed_min_rssi);
            editorPut(settings, AppResource.setting_web_settings_url_key, data.web_settings_url);
            editorPut(settings, AppResource.setting_customer_front_url_key, data.customer_front_url);
            editorPut(settings, AppResource.setting_customer_front_auth_info_id_key, data.customer_front_auth_info.id);
            editorPut(settings, AppResource.setting_customer_front_auth_info_password_key, data.customer_front_auth_info.password);
            editorPut(settings, AppResource.setting_receiver_nickname_key, data.receiver_nickname);

            List<String> uuiWhiteList = data.uuid_white_list;
            if (uuiWhiteList != null && uuiWhiteList.Count >= 0)
            {
                // List形式の項目はカンマ区切りの文字列に置換してセット（最初と最後の「[」「]」は削除する）
                StringBuilder strUuidWhiteList = new StringBuilder();
                foreach(var uuid in uuiWhiteList)
                {
                    if (strUuidWhiteList.Length > 0)
                        strUuidWhiteList.Append(",");
                    strUuidWhiteList.Append(uuid);
                }
                editorPut(settings, AppResource.setting_uuid_white_list_key, strUuidWhiteList.ToString());
            }

            // 以降初回処理が行われないようフラグをfalseにする
            //settings.AddOrUpdateValue(CommonConstants.FIRST_FLG, false);
            return;
        }

        /// <summary>
        /// 設定ファイル更新（受信機登録情報）
        /// </summary>
        /// <param name="data"></param>
        public static void updateRegisterPreferences(ReceiversInfo data)
        {
            ISettings settings = CrossSettings.Current;
            editorPut(settings, AppResource.setting_iothub_auth_info_name_key, data.name);
            editorPut(settings, AppResource.setting_iothub_auth_info_password_key, data.password);
            editorPut(settings, AppResource.setting_active_host_key, data.active_host);
            editorPut(settings, AppResource.setting_standby_host_key, data.standby_host);

        }

        /// <summary>
        /// 初回起動判定
        /// </summary>
        /// <returns>true:初回 false:初回以降</returns>
        public static bool isFirst()
        {
            ISettings settings = CrossSettings.Current;
            if (settings.GetValueOrDefault(CommonConstants.FIRST_FLG, true))
            {
                // 以降初回処理が行われないようフラグをfalseにする
                settings.AddOrUpdateValue(CommonConstants.FIRST_FLG, false);

                // 送信RSSI種別の初期値登録
                settings.AddOrUpdateValue(AppResource.setting_rssi_type_key, SendRssiTypeConstants.RssiType.RAW.getId());
                DebugMessageUtils.GetInstance().WriteLog(TAG, "初回起動。", LogLevel.I);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 受信機登録判定
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>true:登録済み false:未登録</returns>
        public static bool isRegistered()
        {
            SettingsData data = getSettingsDataFromPreferences();

            if (string.IsNullOrEmpty(data.active_host)
                    || string.IsNullOrEmpty(data.standby_host)
                    || string.IsNullOrEmpty(data.iothub_auth_info.name)
                    || string.IsNullOrEmpty(data.iothub_auth_info.password))
            {

                return false;
            }

            return true;
        }


        /// <summary>
        /// key指定プリファレンス設定値取得（String）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static String getString(String key, String defaultValue)
        {
            ISettings settings = CrossSettings.Current;
            return settings.GetValueOrDefault(key, defaultValue);
        }

        /// <summary>
        /// IoTHub受信機情報取得
        /// </summary>
        /// <returns></returns>
        public static IothubAuthInfo getIothubAuthInfo()
        {
            ISettings settings = CrossSettings.Current;
            IothubAuthInfo info = new IothubAuthInfo();
            info.name = settings.GetValueOrDefault(AppResource.setting_iothub_auth_info_name_key, null);
            info.password = settings.GetValueOrDefault(AppResource.setting_iothub_auth_info_password_key, null);

            return info;
        }
        /// <summary>
        /// カスタマーフロントの認証情報取得
        /// </summary>
        /// <returns></returns>
        public static CustomerFrontAuthInfo getCustomerFrontInfo()
        {
            ISettings settings = CrossSettings.Current;
            CustomerFrontAuthInfo info = new CustomerFrontAuthInfo();
            info.id = settings.GetValueOrDefault(AppResource.setting_customer_front_auth_info_id_key, null);
            info.password = settings.GetValueOrDefault(AppResource.setting_customer_front_auth_info_password_key, null);

            return info;
        }

        /// <summary>
        /// ホワイトリスト取得
        /// </summary>
        /// <returns></returns>
        public static List<String> getUuidWhiteList()
        {
            return new List<string>(getUuidWhiteListRaw().Split(","[0]));
        }
        public static string getUuidWhiteListRaw()
        {
            return CrossSettings.Current.GetValueOrDefault(AppResource.setting_uuid_white_list_key, "");
        }
        /// <summary>
        /// 送信状況判定
        /// </summary>
        /// <returns></returns>
        public static bool isMonitoring()
        {
            ISettings settings = CrossSettings.Current;

            //if (!isMonitoringProcessRemaining())
            //{
            //    return false;
            //}

            return settings.GetValueOrDefault(AppResource.monitoring, false);
        }
        /// <summary>
        /// 送信状況セット
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="value"></param>
        public static void setMonitoring(bool value)
        {
            ISettings settings = CrossSettings.Current;
            settings.AddOrUpdateValue(AppResource.monitoring, value);
        }

        /// <summary>
        /// ローカルのJSON設定ファイルからデータを読み込み設定Infoにセット
        /// 処理中エラーが発生した場合はnullを返却する
        /// </summary>
        /// <returns></returns>
        private static SettingsData getLocalSettings()
        {
            SettingsData data = new SettingsData();
            StringBuilder sbJson = new StringBuilder();
            try
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(SettingsData)).Assembly;
                Stream stream = assembly.GetManifestResourceStream("BeaconReceiverXamarin.config.json");
                // JSON設定ファイルを開く
                
                using (var reader = new System.IO.StreamReader(stream))
                {
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        sbJson.Append(line);
                        sbJson.Append(Environment.NewLine);
                    }
                }
            }
            catch (Exception e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "GetManifestResourceStream failed", e, LogLevel.E);
                throw e;
            }
            // JSON設定ファイルの内容が取得できなかった場合は処理中断
            if (sbJson.ToString().Length == 0)
            {
                throw new InvalidOperationException("config.json is empty.");
            }
            Debug.Write("config.json:" + sbJson.ToString());
            try
            {
                data = JsonConvert.DeserializeObject<SettingsData>(sbJson.ToString());
            }
            catch (JsonReaderException e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "DeserializeObject SettingsData failed", e, LogLevel.E);
                throw e;
            }
            return data;
        }
        /// <summary>
        /// 設定ファイル書き込み
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void editorPut(ISettings settings, String key, Object value)
        {
            if (value == null)
            {
                // 値がnullの項目は更新対象外とする
                return;
            }

            var result = settings.AddOrUpdateValue(key, value.ToString());
            DebugMessageUtils.GetInstance().WriteLog(TAG, "editorPut key:" + key + " value:" + value + " result:" + result, LogLevel.D);
        }

        /// <summary>
        /// 数値取得
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="key"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static int GetInt(ISettings settings, String key, int defValue)
        {
            String value = settings.GetValueOrDefault(key, defValue.ToString());

            try
            {
                int intValue = int.Parse(value);
                return intValue;
            }
            catch (Exception e)
            {
                return defValue;
            }
        }
    }
}
