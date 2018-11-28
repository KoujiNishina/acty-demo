using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using BeaconReceiverXamarin.Droid.Service;
using BeaconReceiverXamarin.Status;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace BeaconReceiverXamarin.Droid.Services
{
    /// <summary>
    /// MainServiceがメモリ不足などで異常終了した場合にLEDを消灯するためのサービス。
    /// 
    /// ⇒あまり重要な機能じゃないです。期待通り動作しないことも多い。
    /// </summary>
    [Service(Name = "com.nttpc.surechigai.receiver.ForceStopHandlingService", Exported = false, Process = ":forcestophandlingprocess")]  //別プロセスでサービスを開始する
    public class ForceStopHandlingService : Android.App.Service
    {
        static readonly string TAG = typeof(ForceStopHandlingService).Name;
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        public override void OnCreate()
        {
            base.OnCreate();
            global::Xamarin.Forms.Forms.Init(this, new Bundle());
            Log.Debug(TAG, "ForceStopHandlingService OnCreate");
        }
        private MainServiceCommandReceiver commandReceiver;
        private bool isStopped = false;
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            base.OnStartCommand(intent, flags, startId);

            Log.Debug(TAG, "OnStartCommand");
            new Task(async () => {
                while (!isStopped)
                {
                    await Task.Delay(1000);
                    //そもそもプロセスが違うのにAppStatusManager.GetInstance().appStatusの値は正しくみられるのかしら
                    if (!MainService.IsServiceRunning())
                    {
                        AppStatusManager.GetInstance().ChangeAppStatus(AppStatusEnum.Stopped);
                    }
                }

            }).Start();

            commandReceiver = new MainServiceCommandReceiver(this);
            var filter = new IntentFilter();
            filter.AddAction(BackgroundService_Droid.ACTION_STOP_SERVICE);
            RegisterReceiver(commandReceiver, filter);

            return StartCommandResult.Sticky;   //リソース不足でシステムに強制終了されても再起動される設定
        }

        private class MainServiceCommandReceiver : BroadcastReceiver
        {
            private ForceStopHandlingService mainService;

            public MainServiceCommandReceiver(ForceStopHandlingService mainService)
            {
                this.mainService = mainService;
            }
            public override void OnReceive(Context context, Intent intent)
            {
                Log.Debug(TAG, "OnReceive intent.Action:" + intent.Action);
                switch (intent.Action)
                {
                    case BackgroundService_Droid.ACTION_STOP_SERVICE:
                        mainService.isStopped = true;
                        mainService.StopSelf();
                        break;
                }
            }
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            UnregisterReceiver(commandReceiver);

            Log.Debug(TAG, "OnDestroy");
        }
        public static bool IsServiceRunning()
        {
            ForceStopHandlingService bService = new ForceStopHandlingService();
            Java.Lang.Class nService = bService.Class;
            ActivityManager manager = (ActivityManager)Android.App.Application.Context.GetSystemService(Context.ActivityService);
            foreach (ActivityManager.RunningServiceInfo service in manager.GetRunningServices(int.MaxValue))
            {
                //Log.Debug(TAG, "service.Service.ClassName:" + service.Service.ClassName);
                if (nService.Name.Equals(service.Service.ClassName))
                {
                    Log.Debug(TAG, "IsServiceRunning=true");
                    return true;
                }
            }
            Log.Debug(TAG, "IsServiceRunning=false");
            return false;
        }
    }
}