using BeaconReceiverXamarin.Status;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverXamarin.Interface
{
    /// <summary>
    /// アプリ状態を(LEDなどで)ユーザーに通知する
    /// </summary>
    public interface IStatusService
    {
        void ShowStatus(AppStatusEnum status);
    }
}
