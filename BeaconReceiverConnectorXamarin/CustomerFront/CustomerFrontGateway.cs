using BeaconReceiverConnectorXamarin.CustomerFront.Data;
using BeaconReceiverConnectorXamarin.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BeaconReceiverConnectorXamarin.CustomerFront
{
    public class CustomerFrontGateway
    {
        static readonly string TAG = typeof(CustomerFrontGateway).Name;
        private CustomerFrontAuthorization mAuthorization;
        /**
         * コンストラクタ.
         *
         * @param authorization    認証情報(BASIC認証)
         */
        public CustomerFrontGateway(CustomerFrontAuthorization authorization)
        {

            mAuthorization = authorization;
        }
        /**
         * 受信機登録（登録情報無し）.
         *
         * @param requestUrl    カスタマーフロントURL
         * @param type   受信機登録コールバックで取得する登録データのクラスオブジェクト
         * @throws MalformedURLException カスタマーフロントURLが無効な書式の場合にthrowされる
         */
        public async Task<RegisterResponseBase> register(String requestUrl, RegisterResponseBase typeObj)
        {
            var result = await register(requestUrl, typeObj, null);
            return result;
        }
        public async Task<RegisterResponseBase> register(String requestUrl, RegisterResponseBase typeObj, RegisterParamBase registerParam)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "register start requestUrl:" + requestUrl, LogLevel.I);
            if (requestUrl == null) throw new ArgumentException("requestUrl cannot be null.");
            Uri requestUri;
            // requestURL syntax validation check
            if (!Uri.TryCreate(requestUrl, UriKind.Absolute, out requestUri) ||
                   (requestUri.Scheme != Uri.UriSchemeHttp && requestUri.Scheme != Uri.UriSchemeHttps))
            {
                throw new UriFormatException("requestUrl is not valid.");
            }

            String id = null;
            String password = null;
            if (mAuthorization != null)
            {

                id = mAuthorization.id;
                password = mAuthorization.password;
            }
            String[] parameters = { requestUrl, id, password };

            String requestJsonBody = null;
            if (registerParam != null)
            {

                requestJsonBody = JsonConvert.SerializeObject(registerParam);
            }

            HttpBaseAsyncTask task = new RegisterAsyncTask(requestJsonBody);
            HttpBaseAsyncTaskResult result = await task.Execute(parameters);
            RegisterResponseBase registered = null;
            if (result.httpStatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                registered = (RegisterResponseBase)JsonConvert.DeserializeObject(result.responseJsonBody, typeObj.GetType());
            }
            else
            {
                registered = (RegisterResponseBase)JsonConvert.DeserializeObject("{}", typeObj.GetType());
            }
            registered.httpStatusCode = result.httpStatusCode;
            DebugMessageUtils.GetInstance().WriteLog(TAG, "register end httpStatusCode:" + result.httpStatusCode + " responseJsonBody:" + result.responseJsonBody, LogLevel.I);
            return registered;
        }
        public async Task<bool> IsReachable(String requestUrl)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "IsReachable start." + requestUrl, LogLevel.D);
            bool ret = false;
            if (requestUrl == null) throw new ArgumentException("requestUrl cannot be null.");
            Uri requestUri;
            // requestURL syntax validation check
            if (!Uri.TryCreate(requestUrl, UriKind.Absolute, out requestUri) ||
                   (requestUri.Scheme != Uri.UriSchemeHttp && requestUri.Scheme != Uri.UriSchemeHttps))
            {
                throw new UriFormatException("requestUrl is not valid.");
            }
            String id = null;
            String password = null;
            if (mAuthorization != null)
            {

                id = mAuthorization.id;
                password = mAuthorization.password;
            }
            String[] parameters = { requestUrl, id, password };

            HttpBaseAsyncTask task = new WebSettingsAsyncTask();
            try
            {
                HttpBaseAsyncTaskResult result = await task.Execute(parameters);
                ret = (result.httpStatusCode == (int)System.Net.HttpStatusCode.OK);
                DebugMessageUtils.GetInstance().WriteLog(TAG, "IsReachable responseJsonBody." + result.responseJsonBody, LogLevel.D);
            }
            catch (HttpRequestException e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "IsReachable failed. requestUrl:" + requestUrl, e, LogLevel.I);
                return ret;
            }
            catch (TaskCanceledException e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "IsReachable failed. requestUrl:" + requestUrl, e, LogLevel.I);
                return ret;
            }
            DebugMessageUtils.GetInstance().WriteLog(TAG, "IsReachable end." + requestUrl, LogLevel.D);
            return ret;
        }
        /**
         * Web設定取得.
         *
         * @param requestUrl    カスタマーフロントURL
         * @param type   Web設定コールバックで取得するWeb設定レスポンスデータのクラスオブジェクト
         * @throws MalformedURLException カスタマーフロントURLが無効な書式の場合にthrowされる
         */
        public async Task<WebSettingsBase> fetchWebSettings(String requestUrl, WebSettingsBase typeObj)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "fetchWebSettings start requestUrl:" + requestUrl, LogLevel.I);
            if (requestUrl == null) throw new ArgumentException("requestUrl cannot be null.");
            Uri requestUri;
            // requestURL syntax validation check
            if (!Uri.TryCreate(requestUrl, UriKind.Absolute, out requestUri) ||
                   (requestUri.Scheme != Uri.UriSchemeHttp && requestUri.Scheme != Uri.UriSchemeHttps))
            {
                throw new UriFormatException("requestUrl is not valid.");
            }
            String id = null;
            String password = null;
            if (mAuthorization != null)
            {

                id = mAuthorization.id;
                password = mAuthorization.password;
            }
            String[] parameters = { requestUrl, id, password };
            HttpBaseAsyncTask task = new WebSettingsAsyncTask();
            HttpBaseAsyncTaskResult result = await task.Execute(parameters);
            WebSettingsBase setting = null;
            if (result.httpStatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                setting = (WebSettingsBase)JsonConvert.DeserializeObject(result.responseJsonBody, typeObj.GetType());
            }
            else
            {
                setting = (WebSettingsBase)JsonConvert.DeserializeObject("{}", typeObj.GetType());
            }
            setting.httpStatusCode = result.httpStatusCode;
            DebugMessageUtils.GetInstance().WriteLog(TAG, "fetchWebSettings end httpStatusCode:" + result.httpStatusCode + " responseJsonBody:" + result.responseJsonBody, LogLevel.I);
            return setting;
        }
    }
}
