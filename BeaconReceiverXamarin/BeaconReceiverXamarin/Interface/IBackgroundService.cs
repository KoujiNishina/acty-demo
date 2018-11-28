using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverXamarin.Interface
{
    public interface IBackgroundService
    {
        void StartMainSerivce();
        void StopMainService();
        bool IsMainServiceRunning();
    }
}
