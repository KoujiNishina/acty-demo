using BeaconReceiverConnectorXamarin.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BeaconReceiverConnectorXamarin.IoTHub
{
    class IotHubHttpAsyncTask {

        private const String TAG = "IotHubHttpAsyncTask";

        private const String REQUEST_METHOD_POST = "POST";

        private EventCallback mCallback;
        private Object mCallbackContext;

        public IotHubHttpAsyncTask(EventCallback callback, Object callbackContext)
        {

            mCallback = callback;
            mCallbackContext = callbackContext;
        }
        private HttpClient client = new HttpClient();
        // {String... params} specification:
        // params[0]:IoT Hub connection string
        // params[1]:d2c message string
        public async void DoInBackground(String[] parameters)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "DoInBackground start d2cMessage.Length:" + parameters[1].Length + " d2cMessage:" + parameters[1], LogLevel.I);
            int statusCode = SendStatusCode.INTERNAL_STATUS_CONNECTIONSTRING_FAILED;

            String connectionString = parameters[0];
            String d2cMessage = parameters[1];

            try {
                IotHubDeviceConfig config = new IotHubDeviceConfig(connectionString);

                var eventUrl = "https://" + config.getEventUriString();
                //if (DebugUtils.GetInstance().CurrentSendMode == IotHubTransaction.SendMode.NORMAL)
                //    eventUrl = "https://dummy.axwetadfasdfa.co.jp"; //★★★テスト用eventUrl =
                //long timeElapsed = (long)DebugUtils.GetInstance().TimeElapsed.TotalMilliseconds; //★★★テスト用
                //if (eventUrl.Contains("s01-ih11.azure-devices.net") && timeElapsed > 1000 * 30 && timeElapsed < 1000 * 100) eventUrl = "https://dummy.axwetadfasdfa.co.jp"; //★★★テスト用
                //if (timeElapsed > 1000 * 0 && timeElapsed < 1000 * 240) eventUrl = "https://dummy.axwetadfasdfa.co.jp"; //★★★テスト用

                client.DefaultRequestHeaders.Add("User-Agent", config.getUserAgent());
                //connection.setRequestProperty("User-Agent", config.getUserAgent());

                //// Shared Access Signature 設定
                client.DefaultRequestHeaders.Add("authorization", config.getSasToken());
                //connection.setRequestProperty("authorization", config.getSasToken());

                client.DefaultRequestHeaders.Add("iothub-to", config.getEventUriPath());
                //connection.setRequestProperty("iothub-to", config.getEventUriPath());
                //client.DefaultRequestHeaders.Add("content-type", config.getContentType());
                //connection.setRequestProperty("content-type", config.getContentType());
                client.Timeout = TimeSpan.FromMilliseconds(config.getReadTimeoutMillis());  //HttpClientのTimeoutプロパティはReadTimeoutとConnectTimeoutの合計値。ReadTimeoutだけ指定する方法はない。
                //client.Timeout = TimeSpan.FromMilliseconds(5);  //★★★テスト用
                //connection.setReadTimeout(config.getReadTimeoutMillis());
                StringContent content = new StringContent("");
                if (d2cMessage != null) {
                    content = new StringContent(d2cMessage, Encoding.UTF8, "application/json");
                }

                HttpResponseMessage response = await client.PostAsync(eventUrl, content);

                statusCode = (int)response.StatusCode;
                DebugMessageUtils.GetInstance().WriteLog(TAG, "DoInBackground end statusCode:" + statusCode, LogLevel.I);
            }
            catch (HttpRequestException e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "IoTHub HttpRequestException", e, LogLevel.E);
                if (e.InnerException != null && e.InnerException is WebException && e.InnerException.Message.StartsWith("リモート名を解決できませんでした。"))
                    statusCode = SendStatusCode.INTERNAL_STATUS_UNKNOWN_HOST;
                if (e.InnerException != null)
                    DebugMessageUtils.GetInstance().WriteLog(TAG, "IoTHub HttpRequestException(Cause)", e.InnerException, LogLevel.E);
            }
            catch (TaskCanceledException e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "IoTHub request timed out", e, LogLevel.E);
            }
            catch (UriFormatException e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "IoTHub UriFormatException", e, LogLevel.E);
                throw e;
            }
            catch (FormatException e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "IoTHub FormatException(probably device key is not valid base64 string)", e, LogLevel.E);
                throw e;
            }

            //return statusCode;
            onPostExecute(statusCode);
        }

        public void onPostExecute(int httpStatusCode)
        {

            if (mCallback != null)
            {

                SendStatusCodeEnum sendStatusCode = SendStatusCode.getSendStatusCode(httpStatusCode);

                mCallback.execute(sendStatusCode, mCallbackContext);
            }
        }

        public interface EventCallback
        {

            void execute(SendStatusCodeEnum status, Object callbackContext);
        }
    }
}
