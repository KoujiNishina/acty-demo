using System;
using System.Collections.Generic;
using System.Text;
using static BeaconReceiverConnectorXamarin.IoTHub.IotHubTransaction;

namespace BeaconReceiverConnectorXamarin.Utils
{
    public class DebugUtils
    {
        public DateTime TaskStartTime { private get; set; }
        //public SendMode CurrentSendMode { get { return mCurrentSendMode; } set { mCurrentSendMode = value; if (mCurrentSendMode == SendMode.FAILBACK) Failbacked = true; } }

        //private SendMode mCurrentSendMode;
        //public bool Failbacked { get; set; } = false;

        private static DebugUtils sInstance = new DebugUtils();
        public static DebugUtils GetInstance()
        {
            return sInstance;
        }
        public TimeSpan TimeElapsed { get { return (DateTime.Now - TaskStartTime); } }
    }
}
