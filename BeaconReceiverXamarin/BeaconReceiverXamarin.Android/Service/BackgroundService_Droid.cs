
using Android.Content;
using BeaconReceiverXamarin.Droid.Service;
using BeaconReceiverXamarin.Droid.Services;
using BeaconReceiverXamarin.Interface;
using Xamarin.Forms;

[assembly: Dependency(typeof(BackgroundService_Droid))]
namespace BeaconReceiverXamarin.Droid.Service
{
    /// <summary>
    /// サービスの起動、停止、計測開始などの要求を出す。
    /// </summary>
    public class BackgroundService_Droid : IBackgroundService
    {
        const string TAG = "BackgroundService_Droid";
        public const string ACTION_START_SERVICE = "action.surechigai.receiver.startservice";
        public const string ACTION_STOP_SERVICE = "action.surechigai.receiver.stopservice";

        public bool IsMainServiceRunning()
        {
            return MainService.IsServiceRunning();
        }

        public void StartMainSerivce()
        {
            Intent broadcastIntent = new Intent(ACTION_START_SERVICE);
            Android.App.Application.Context.SendBroadcast(broadcastIntent);
        }
        public void StopMainService()
        {
            Intent broadcastIntent = new Intent(ACTION_STOP_SERVICE);
            Android.App.Application.Context.SendBroadcast(broadcastIntent);
        }
    }
}