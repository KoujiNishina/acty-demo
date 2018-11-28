using BeaconReceiverConnectorXamarin.Utils;
using BeaconReceiverXamarin.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverXamarin.Status
{
    public enum AppStatusEnum
    {
        Stopped,    //停止中
        ShuttedDownByError,    //前回エラー終了
        Starting,   //起動中
        Running,    //正常動作中
        Error,  //ステータス異常
        Failover   //フェイルオーバー中
    }
    public class AppStatusManager
    {
        static readonly string TAG = typeof(AppStatusManager).Name;
        private static AppStatusManager sInstance = new AppStatusManager();

        public static AppStatusManager GetInstance()
        {
            return sInstance;
        }
        private AppStatusManager()
        {
            appStatus = AppStatusEnum.Stopped;
            statusService = Xamarin.Forms.DependencyService.Get<IStatusService>();
        }
        public AppStatusEnum appStatus { get; private set; }
        private IStatusService statusService;
        public void ChangeAppStatus(AppStatusEnum appStatus)
        {
            if (this.appStatus == appStatus)
                return;
            DebugMessageUtils.GetInstance().WriteLog(TAG, "ChangeAppStatus orgAppStatus:" + this.appStatus + " new appStatus:" + appStatus, LogLevel.I);
            this.appStatus = appStatus; 
            statusService.ShowStatus(appStatus);
        }
    }
}
