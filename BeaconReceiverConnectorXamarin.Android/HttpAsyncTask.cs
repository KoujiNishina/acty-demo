using Android.Util;
using BeaconReceiverConnectorXamarin.Android;
using BeaconReceiverConnectorXamarin.Interface;
using Java.IO;
using Java.Lang;
using Java.Net;
using Java.Security.Cert;
using Javax.Net.Ssl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xamarin.Forms;

[assembly: Dependency(typeof(HttpAsyncTask))]
namespace BeaconReceiverConnectorXamarin.Android
{
    public class HttpAsyncTask : IHttpAsyncTask
    {
        const string TAG = "HttpAsyncTask";
        public void DoInBackground(string[] parameters)
        {
            DataOutputStream dataOutputStream = null;
            InputStream inputStream = null;
            BufferedReader bufferedReader = null;
            Java.Lang.StringBuilder responseJsonBody = null;
            int statusCode = -1;

            SSLContext sslcontext = null;
            try
            {
                ITrustManager[] tm = { new MyX509TrustManager() };
                sslcontext = SSLContext.GetInstance("SSL");
                sslcontext.Init(null, tm, null);
                //ホスト名の検証ルール　何が来てもtrueを返す
                HttpsURLConnection.DefaultHostnameVerifier =
                        new MyHostnameVerifier();
                // アクセス先URL設定
                URL url = new URL(parameters[0]);
                HttpsURLConnection connection = (HttpsURLConnection)url.OpenConnection();
                connection.SSLSocketFactory = sslcontext.SocketFactory;
                // Basic認証設定
                string idPassword = parameters[1] + ":" + parameters[2];
                string encodedIdPassword = Base64.EncodeToString(System.Text.Encoding.UTF8.GetBytes(idPassword), Base64.NoWrap);
                connection.SetRequestProperty("Authorization", "Basic " + encodedIdPassword);

                connection.SetRequestProperty("Accept-Language", "jp");
                connection.ReadTimeout = (5000);
                connection.ConnectTimeout = (5000);


                HttpURLConnection httpUrlConnection = (HttpURLConnection) connection;
                string requestMethod = "POST";

                httpUrlConnection.RequestMethod = (requestMethod);
                string requestJsonBody = "{}";
                if (true) {

                    httpUrlConnection.DoOutput = (true);
                    if (requestJsonBody != null) {

                        httpUrlConnection.SetFixedLengthStreamingMode(2);
                        connection.SetRequestProperty("Content-Type", "application/json; charset=UTF-8");
                    }
                }

                connection.Connect();

                if (requestJsonBody != null) {

                    dataOutputStream = new DataOutputStream(httpUrlConnection.OutputStream);
                    dataOutputStream.Write(System.Text.Encoding.UTF8.GetBytes(requestJsonBody));
                    dataOutputStream.Flush();
                }

                statusCode = (int)httpUrlConnection.ResponseCode;

                switch (statusCode) {

                    case (int)HttpURLConnection.HttpOk:
                        StringBuffer sbJson = new StringBuffer();
                        using (var reader = new System.IO.StreamReader(connection.InputStream))
                        {
                            string line = null;
                            while ((line = reader.ReadLine()) != null)
                            {
                                sbJson.Append(line);
                                sbJson.Append(Environment.NewLine);
                            }
                        }
                        Log.Debug(TAG, "sbJson:" + sbJson);
                        break;
                }
            }
            catch (IOException e)
            {
                e.PrintStackTrace();
                Log.Error(TAG, "[LineNumber:" + Thread.CurrentThread().GetStackTrace()[0].LineNumber + "]" + e.Message);
            }
            finally
            {

                try
                {

                    if (dataOutputStream != null)
                        dataOutputStream.Close();

                    if (bufferedReader != null)
                        bufferedReader.Close();

                    if (inputStream != null)
                        inputStream.Close();
                }
                catch (IOException e)
                {
                }
            }
        }
    }
    class MyX509TrustManager : Java.Lang.Object, IX509TrustManager
    {
        public void CheckClientTrusted(Java.Security.Cert.X509Certificate[] chain, string authType)
        {
        }

        public void CheckServerTrusted(Java.Security.Cert.X509Certificate[] chain, string authType)
        {
        }

        Java.Security.Cert.X509Certificate[] IX509TrustManager.GetAcceptedIssuers()
        {
            return null;
        }
    }
    class MyHostnameVerifier : Java.Lang.Object, IHostnameVerifier
    {
        public bool Verify(string hostname, ISSLSession session)
        {
            return true;
        }
    }
}
