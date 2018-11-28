using BeaconReceiverConnectorXamarin.Utils;
using System;

namespace BeaconReceiverXamarin.Interface
{
    public interface IMessageWriter
    {
        /// <summary>
        /// ログファイルなどのディレクトリの絶対パスを返す
        /// </summary>
        /// <returns></returns>
        string GetIOFileDirPath();
        /// <summary>
        /// トーストを表示する
        /// </summary>
        /// <returns></returns>
        void ShowToast(string message);
        /// <summary>
        /// コンソールログを出力
        /// </summary>
        /// <returns></returns>
        void WriteLog(string tag, string message, Exception ex, LogLevel logLevel);
    }
}
