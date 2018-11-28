using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BeaconReceiverXamarin.Interface
{
    public interface IMainTaskCallback
    {
        /// <summary>
        /// サービス停止を行う
        /// </summary>
        /// <returns></returns>
        void PostStopSelf();

        /// <summary>
        /// コマンド入力を待つ
        /// </summary>
        /// <returns></returns>
        ServiceCommand WaitForCommand();
    }

    public enum ServiceCommand
    {
        Void,
        Stop
    }
}
