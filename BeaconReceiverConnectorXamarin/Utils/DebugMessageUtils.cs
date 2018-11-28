using BeaconReceiverConnectorXamarin.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BeaconReceiverConnectorXamarin.Utils
{
    public class DebugMessageUtils : IDebugMessageHandler
    {
        private static DebugMessageUtils sInstance = new DebugMessageUtils();
        public static DebugMessageUtils GetInstance()
        {
            return sInstance;
        }
        private IDebugMessageHandler mInnerDebugMessageHandler = new DefaultDebugMessageHandler();
        public static void SetDebugMessageHandler(IDebugMessageHandler debugMessageHandler)
        {
            sInstance.mInnerDebugMessageHandler = debugMessageHandler;
        }
        public void ShowMessage(string tag, string message, LogLevel logLevel)
        {
            mInnerDebugMessageHandler.ShowMessage(tag, message, logLevel);
        }

        public void ShowMessage(string tag, string message, Exception ex, LogLevel logLevel)
        {
            mInnerDebugMessageHandler.ShowMessage(tag, message, ex, logLevel);
        }

        public void ShowToast(string message)
        {
            mInnerDebugMessageHandler.ShowToast(message);
        }

        public void WriteLog(string tag, string message, LogLevel logLevel)
        {
            mInnerDebugMessageHandler.WriteLog(tag, message, logLevel);
        }

        public void WriteLog(string tag, string message, Exception ex, LogLevel logLevel)
        {
            mInnerDebugMessageHandler.WriteLog(tag, message, ex, logLevel);
        }
        private class DefaultDebugMessageHandler : IDebugMessageHandler
        {
            public void ShowMessage(string tag, string message, LogLevel logLevel)
            {
                ShowMessage(tag, message, null, logLevel);
            }

            public void ShowMessage(string tag, string message, Exception ex, LogLevel logLevel)
            {
                Debug.WriteLine(tag + " - " + message);
                if (ex != null)
                {
                    Debug.WriteLine(tag + " - " + ex.Message);
                    Debug.WriteLine(tag + " - " + ex.StackTrace);
                }
            }

            public void ShowToast(string message)
            {
            }

            public void WriteLog(string tag, string message, LogLevel logLevel)
            {
                ShowMessage(tag, message, null, logLevel);
            }

            public void WriteLog(string tag, string message, Exception ex, LogLevel logLevel)
            {
                ShowMessage(tag, message, ex, logLevel);
            }
        }
    }

    public enum LogLevel
    {
        V, D, I, W, E
    }

}
