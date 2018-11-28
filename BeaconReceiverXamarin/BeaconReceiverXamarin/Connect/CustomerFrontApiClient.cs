using BeaconReceiverConnectorXamarin;
using BeaconReceiverConnectorXamarin.CustomerFront;
using BeaconReceiverXamarin.Constants;
using BeaconReceiverConnectorXamarin.Interface;
using BeaconReceiverXamarin.Store;
using BeaconReceiverConnectorXamarin.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;

namespace BeaconReceiverXamarin.Connect
{
    public class CustomerFrontApiClient
    {
        const string TAG = "CustomerFrontApiClient";
        /// <summary>
        /// Web設定更新
        /// ActyG1は端末起動直後はネットワーク接続に失敗することがあるので、リトライ処理を入れる。
        /// </summary>
        /// <param name="maxRetryCount"></param>
        /// <param name="retryInterval"></param>
        /// <returns></returns>
        public static async Task<bool> fetchSettings(int maxRetryCount, int retryInterval)
        {
            // 現時点のPreferencesファイルの設定値取得
            SettingsData data = SetupDataStore.getSettingsDataFromPreferences();
            String url = data.web_settings_url + CommonConstants.WEB_SETTINGS_URL;
            String id = data.customer_front_auth_info.id;
            String password = data.customer_front_auth_info.password;
            CustomerFrontAuthorization authorization = new CustomerFrontAuthorization(id, password);
            CustomerFrontGateway customerFrontGateway = new CustomerFrontGateway(authorization);
            SettingsData fetchedSetting = null;
            int retryCount = 0;
            for (retryCount = 0; retryCount <= maxRetryCount; retryCount++)
            {
                try
                {
                    fetchedSetting = (SettingsData)await customerFrontGateway.fetchWebSettings(url, new SettingsData());
                    break;
                }
                //catch (ArgumentException e)
                //{
                //    DebugMessageUtils.GetInstance().WriteLog(TAG, "Web設定ファイルダウンロード失敗. ArgumentException", e, LogLevel.E);
                //    return false;
                //}
                catch (UriFormatException e)
                {
                    DebugMessageUtils.GetInstance().WriteLog(TAG, "Web設定ファイルダウンロード失敗. UriFormatException", e, LogLevel.E);
                    return false;
                }
                catch (JsonReaderException e)
                {
                    DebugMessageUtils.GetInstance().WriteLog(TAG, "Web設定ファイルダウンロード失敗. JsonReaderException", e, LogLevel.E);
                    return false;
                }
                catch (HttpRequestException e)
                {
                    DebugMessageUtils.GetInstance().WriteLog(TAG, "Web設定ファイルダウンロード失敗 url:" + url, e, LogLevel.W);
                    await Task.Delay(retryInterval);
                }
                catch (TaskCanceledException e)
                {
                    DebugMessageUtils.GetInstance().WriteLog(TAG, "Web設定ファイルダウンロード失敗 url:" + url, e, LogLevel.W);
                    await Task.Delay(retryInterval);
                }
            }
            if (retryCount > maxRetryCount)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "Web設定ファイルダウンロード失敗回数が上限を超えました。", LogLevel.E);
                return false;
            }
            if (fetchedSetting != null && fetchedSetting.httpStatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                SetupDataStore.updateWebSettingsPreferences(fetchedSetting);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 受信機登録
        /// ActyG1は端末起動直後はネットワーク接続に失敗することがあるので、リトライ処理を入れる。
        /// </summary>
        /// <param name="maxRetryCount"></param>
        /// <param name="retryInterval"></param>
        /// <returns></returns>
        public static async Task<bool> register(int maxRetryCount, int retryInterval)
        {
            SettingsData data = SetupDataStore.getSettingsDataFromPreferences();
            // 接続情報
            String url = data.customer_front_url + CommonConstants.RECEIVERS_URL;
            String id = data.customer_front_auth_info.id;
            String password = data.customer_front_auth_info.password;

            CustomerFrontAuthorization authorization = new CustomerFrontAuthorization(id, password);
            CustomerFrontGateway customerFrontGateway = new CustomerFrontGateway(authorization);
            RegisterResponse response = null;
            int retryCount = 0;
            for (retryCount = 0; retryCount <= maxRetryCount; retryCount++)
            {
                try
                {
                    response = (RegisterResponse)await customerFrontGateway.register(url, new RegisterResponse());
                    break;
                }
                //catch (ArgumentException e)
                //{
                //    DebugMessageUtils.GetInstance().WriteLog(TAG, "受信機登録失敗. ArgumentException", e, LogLevel.E);
                //    return false;
                //}
                catch (UriFormatException e)
                {
                    DebugMessageUtils.GetInstance().WriteLog(TAG, "受信機登録失敗. UriFormatException", e, LogLevel.E);
                    return false;
                }
                catch (JsonReaderException e)
                {
                    DebugMessageUtils.GetInstance().WriteLog(TAG, "受信機登録失敗. JsonReaderException", e, LogLevel.E);
                    return false;
                }
                catch (HttpRequestException e)
                {
                    DebugMessageUtils.GetInstance().WriteLog(TAG, "受信機登録失敗 url:" + url, e, LogLevel.W);
                    await Task.Delay(retryInterval);
                }
                catch (TaskCanceledException e)
                {
                    DebugMessageUtils.GetInstance().WriteLog(TAG, "受信機登録失敗 url:" + url, e, LogLevel.W);
                    await Task.Delay(retryInterval);
                }
            }
            if (retryCount > maxRetryCount)
            {
                DebugMessageUtils.GetInstance().ShowMessage(TAG, "受信機登録失敗回数が上限を超えました。", LogLevel.E);
                return false;
            }
            if (response != null && response.httpStatusCode == (int)System.Net.HttpStatusCode.OK && response.items != null && response.items.Count == 1)
            {
                SetupDataStore.updateRegisterPreferences(response.items[0]);
                DebugMessageUtils.GetInstance().ShowMessage(TAG, "受信機登録成功。", LogLevel.I);
                return true;
            }
            return false;
        }
        /// <summary>
        /// ActyG1は端末起動してからインターネット接続可能になるまで時間がかかるため、カスタマーフロントへの要求が成功するまで待つ。
        /// </summary>
        /// <param name="retryInterval"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static async Task WaitUntilNetworkIsReacable(int retryInterval, Action<bool, int> callback)
        {
            SettingsData data = SetupDataStore.getSettingsDataFromPreferences();
            String url = data.customer_front_url + CommonConstants.RECEIVERS_URL + "?number=1";   //受信機一覧取得API
            String id = data.customer_front_auth_info.id;
            String password = data.customer_front_auth_info.password;

            CustomerFrontAuthorization authorization = new CustomerFrontAuthorization(id, password);
            CustomerFrontGateway customerFrontGateway = new CustomerFrontGateway(authorization);
            int retryCount = 0;
            for (retryCount = 0; ; retryCount++)
            {
                var isReachable = await customerFrontGateway.IsReachable(url);
                if (!isReachable)
                {
                    DebugMessageUtils.GetInstance().ShowMessage(TAG, "インターネット接続失敗。再試行します", LogLevel.W);
                    callback(false, retryCount + 1);
                    await Task.Delay(retryInterval);
                }
                else
                {
                    callback(true, retryCount);
                    break;
                }
                
            }
            return;
        }
    }
}
