using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using BeaconReceiverXamarin.BackgroundTask;
using BeaconReceiverXamarin.Droid.Service;
using BeaconReceiverXamarin.Interface;

namespace BeaconReceiverXamarin.Droid.Services
{
    /// <summary>
    /// 参考) http://itblogdsi.blog.fc2.com/blog-entry-148.html
    /// </summary>
    //[Service(Name = "com.nttpc.surechigai.receiver.MainService")]
    [Service(Name = "com.nttpc.surechigai.receiver.MainService", Exported = false, Process = ":monitoringprocess")]  //別プロセスでサービスを開始する
    public class MainService : Android.App.Service, IMainTaskCallback
    {
        public const string TAG = "MainService";
        public Handler handler;
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        public override void OnCreate()
        {
            base.OnCreate();
            Log.Debug(TAG, "MainService OnCreate");
            handler = new Handler();
            global::Xamarin.Forms.Forms.Init(this, new Bundle());
        }

        private Task mMainTask;

        private MainServiceCommandReceiver commandReceiver;
        public ManualResetEvent waitHandle = new ManualResetEvent(false);
        public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
        {
            base.OnStartCommand(intent, flags, startId);
            Log.Debug(TAG, "OnStartCommand");
            mMainTask = new Task(() => {
                MonitoringTask mainTask = new MonitoringTask(this);
                mainTask.DoMainTask();
            });
            mMainTask.Start();

            commandReceiver = new MainServiceCommandReceiver(this);
            var filter = new IntentFilter();
            filter.AddAction(BackgroundService_Droid.ACTION_STOP_SERVICE);
            RegisterReceiver(commandReceiver, filter);

            return StartCommandResult.Sticky;   //リソース不足でシステムに強制終了されても再起動される設定
        }
        public ServiceCommand command = ServiceCommand.Void;

        private class MainServiceCommandReceiver : BroadcastReceiver
        {
            private MainService mainService;
            
            public MainServiceCommandReceiver(MainService mainService)
            {
                this.mainService = mainService;
            }
            public override void OnReceive(Context context, Intent intent)
            {
                Log.Debug(TAG, "OnReceive intent.Action:" + intent.Action);
                switch (intent.Action)
                {
                    case BackgroundService_Droid.ACTION_STOP_SERVICE:
                        if (mainService.waitingForCommand)
                        {
                            mainService.command = ServiceCommand.Stop;
                            mainService.waitHandle.Set();
                        }
                        break;
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Log.Debug(TAG, "OnDestroy");
            UnregisterReceiver(commandReceiver);
        }

        public void PostStopSelf()
        {
            handler.Post(() => { StopSelf(); });
        }

        private bool waitingForCommand = false;
        public ServiceCommand WaitForCommand()
        {
            Log.Debug(TAG, "WaitForCommand start");
            waitingForCommand = true;
            //bool isContinue = true;
            //while (isContinue)
            //{
            //    await Task.Delay(500);
            //    switch (command)
            //    {
            //        case ServiceCommand.Stop:
            //            isContinue = false;
            //            break;
            //    }
            //}
            waitHandle.WaitOne(Timeout.Infinite);
            waitingForCommand = false;
            Log.Debug(TAG, "WaitForCommand end");
            return command;
        }
        public static bool IsServiceRunning()
        {
            MainService bService = new MainService();
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