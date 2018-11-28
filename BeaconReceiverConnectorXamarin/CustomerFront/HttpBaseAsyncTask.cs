using BeaconReceiverConnectorXamarin.Resource;
using BeaconReceiverConnectorXamarin.Utils;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BeaconReceiverConnectorXamarin.CustomerFront
{
    public abstract class HttpBaseAsyncTask
    {
        static readonly string TAG = typeof(HttpBaseAsyncTask).Name;

        protected const String REQUEST_METHOD_POST = "POST";
        protected const String REQUEST_METHOD_GET = "GET";

        private const int HTTP_READ_TIMEOUT = 25000;
        private const int HTTP_CONNECT_TIMEOUT = 5000;
        private HttpClient client;
        public HttpBaseAsyncTask()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            };
            client = new HttpClient(handler);
        }

        public async Task<HttpBaseAsyncTaskResult> Execute(string[] parameters)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "Execute parameters.Length:" + parameters.Length + " parameters:" + parameters, LogLevel.D);

            if (bool.Parse(ConnectResource.UseStagingEnvironment) && parameters[0].StartsWith("https://"))
            {
                //ステージング環境ではオレオレ証明書を許容する
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                {
                    return true;
                };
            }
            string url = parameters[0];
            // Basic認証設定
            String idPassword = parameters[1] + ":" + parameters[2];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(idPassword), Base64FormattingOptions.None));
            
            ///★★★HttpClientではトータルのタイムアウト時間しか指定できない。
            ///★★★HttpWebRequestならばReadWriteTimeoutプロパティでレスポンス受信のタイムアウト時間を設定できるが、どちらにしても接続タイムアウト時間は設定できない。
            client.Timeout = TimeSpan.FromMilliseconds(HTTP_CONNECT_TIMEOUT + HTTP_READ_TIMEOUT);

            String requestMethod = getRequestMethod();
            String requestJsonBody = getRequestJsonBody();
            HttpResponseMessage response = null;
            String responseJsonBody = null;
            int statusCode = -1;
            try
            {
                if (requestMethod == REQUEST_METHOD_POST)
                {
                    var content = new StringContent("");
                    if (requestJsonBody != null)
                    {
                        content = new StringContent(requestJsonBody, Encoding.UTF8, "application/json");
                    }
                    response = await client.PostAsync(url, content);
                    statusCode = (int)response.StatusCode;
                }
                else
                {
                    response = await client.GetAsync(parameters[0]);
                    statusCode = (int)response.StatusCode;
                }
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                        responseJsonBody = response.Content.ReadAsStringAsync().Result;
                        break;
                    default:
                        DebugMessageUtils.GetInstance().WriteLog(TAG, "response.Content:" + response.Content.ReadAsStringAsync().Result, LogLevel.D);
                        break;
                }
            }
            catch (HttpRequestException ex)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "Execute failed", ex, LogLevel.I);
                throw ex;
            }
            catch (TaskCanceledException ex)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "Execute timeout", ex, LogLevel.I);
                throw ex;
            }
            HttpBaseAsyncTaskResult result = new HttpBaseAsyncTaskResult();
            result.httpStatusCode = statusCode;
            
            if (responseJsonBody != null)
            {

                result.responseJsonBody = responseJsonBody;
            }

            return result;
        }
        abstract protected String getRequestMethod();
        protected virtual String getRequestJsonBody()
        {

            return null;
        }
    }
}
