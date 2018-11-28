using System;
using Android.OS;
using Android.Util;
using Android.Widget;
using BeaconReceiverXamarin.Droid.Service;
using BeaconReceiverXamarin.Interface;
using BeaconReceiverConnectorXamarin.Utils;
using Xamarin.Forms;
using Java.Lang;
using BeaconReceiverXamarin.Constants;

[assembly: Dependency(typeof(MessageWriter_Droid))]
namespace BeaconReceiverXamarin.Droid.Service
{
    public class MessageWriter_Droid : IMessageWriter
    {
        private HandlerThread handlerThread = new HandlerThread("MessageWriter_Droid");
        private Handler handler;
        public MessageWriter_Droid()
        {
            handlerThread.Start();
            handler = new Handler(handlerThread.Looper);
        }

        public string GetIOFileDirPath()
        {
            return Android.OS.Environment.ExternalStorageDirectory.Path + "/SurechigaiReceiver";
        }
        public void ShowToast(string message)
        {
            handler.Post(() => { Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long).Show(); });
        }

        public void WriteLog(string tag, string message, System.Exception ex, LogLevel logLevel)
        {
            if (AppConstants.ShowMemoryUsageLogLevel <= logLevel)
                message += GetMemoryUsage();
            switch (logLevel)
            {
                case LogLevel.V:

                    if (ex == null)
                        Log.Verbose(tag, message);
                    else
                    {
                        Log.Verbose(tag, message + '\n' + ex.Message + '\n' + ex.StackTrace);
                        if (ex.InnerException != null)
                            Log.Verbose(tag, message + " cause: \n" + ex.InnerException.Message + '\n' + ex.InnerException.StackTrace);
                    }
                    break;
                case LogLevel.D:
                    if (ex == null)
                        Log.Debug(tag, message);
                    else
                    {
                        Log.Debug(tag, message + '\n' + ex.Message + '\n' + ex.StackTrace);
                        if (ex.InnerException != null)
                            Log.Debug(tag, message + " cause: \n" + ex.InnerException.Message + '\n' + ex.InnerException.StackTrace);
                    }
                    break;
                case LogLevel.I:
                    if (ex == null)
                        Log.Info(tag, message);
                    else
                    {
                        Log.Info(tag, message + '\n' + ex.Message + '\n' + ex.StackTrace);
                        if (ex.InnerException != null)
                            Log.Info(tag, message + " cause: \n" + ex.InnerException.Message + '\n' + ex.InnerException.StackTrace);
                    }
                    break;
                case LogLevel.W:
                    if (ex == null)
                        Log.Warn(tag, message);
                    else
                    {
                        Log.Warn(tag, message + '\n' + ex.Message + '\n' + ex.StackTrace);
                        if (ex.InnerException != null)
                            Log.Warn(tag, message + " cause: \n" + ex.InnerException.Message + '\n' + ex.InnerException.StackTrace);
                    }
                    break;
                case LogLevel.E:
                    if (ex == null)
                        Log.Error(tag, message);
                    else
                    {
                        Log.Error(tag, message + '\n' + ex.Message + '\n' + ex.StackTrace);
                        if (ex.InnerException != null)
                            Log.Error(tag, message + " cause: \n" + ex.InnerException.Message + '\n' + ex.InnerException.StackTrace);
                    }
                    break;
            }
        }
        private string GetMemoryUsage()
        {
            long total = Runtime.GetRuntime().TotalMemory();
            long free = Runtime.GetRuntime().FreeMemory();
            long used = total - free;
            long max = Runtime.GetRuntime().MaxMemory();
            StringBuffer sb = new StringBuffer();
            sb.Append(" total => " + total / 1024 + "KB");
            sb.Append(" free  => " + free / 1024 + "KB");
            sb.Append(" used  => " + (total - free) / 1024 + "KB");
            sb.Append(" max   => " + max / 1024 + "KB");
            return sb.ToString();
        }
    }
}