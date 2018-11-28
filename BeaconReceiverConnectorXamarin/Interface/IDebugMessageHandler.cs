using BeaconReceiverConnectorXamarin.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverConnectorXamarin.Interface
{
    public interface IDebugMessageHandler
    {
        void ShowMessage(string tag, string message, LogLevel logLevel);
        void ShowMessage(string tag, string message, Exception ex, LogLevel logLevel);
        void ShowToast(string message);
        void WriteLog(string tag, string message, LogLevel logLevel);
        void WriteLog(string tag, string message, Exception ex, LogLevel logLevel);
    }
}
