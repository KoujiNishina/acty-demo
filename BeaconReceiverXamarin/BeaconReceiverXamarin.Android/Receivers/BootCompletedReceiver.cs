using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using BeaconReceiverXamarin.Droid.Services;
using BeaconReceiverXamarin.Droid.Service;

namespace BeaconReceiverXamarin.Droid.Receivers
{
    [BroadcastReceiver]
    [IntentFilter(new[] { Intent.ActionBootCompleted,
                          "android.intent.action.QUICKBOOT_POWERON",
                          "com.htc.intent.action.QUICKBOOT_POWERON"
                          ,BackgroundService_Droid.ACTION_START_SERVICE
    })]
    public class BootCompletedReceiver : BroadcastReceiver
    {
        const string TAG = "BootCompletedReceiver";
        public BootCompletedReceiver() : base()
        {
        }
        public override void OnReceive(Context context, Intent intent)
        {
            //Intent activityIntent = null;
            Intent serviceIntent = null;

            Log.Debug(TAG, "OnReceive");

            if (!MainService.IsServiceRunning())
            {
                //サービスを起動
                serviceIntent = new Intent(context, typeof(MainService));
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop &&
                            Build.VERSION.SdkInt <= BuildVersionCodes.LollipopMr1)
                {
                    //Android5 Lollipop対応
                    string packageName = context.PackageManager.GetPackageInfo(context.PackageName, 0).PackageName;
                    serviceIntent.SetPackage(packageName);
                }
                else
                {
                    serviceIntent.AddFlags(ActivityFlags.NewTask);
                }
                context.StartService(serviceIntent);
            }
            if (!ForceStopHandlingService.IsServiceRunning())
            {
                //サービスを起動
                serviceIntent = new Intent(context, typeof(ForceStopHandlingService));
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop &&
                            Build.VERSION.SdkInt <= BuildVersionCodes.LollipopMr1)
                {
                    //Android5 Lollipop対応
                    string packageName = context.PackageManager.GetPackageInfo(context.PackageName, 0).PackageName;
                    serviceIntent.SetPackage(packageName);
                }
                else
                {
                    serviceIntent.AddFlags(ActivityFlags.NewTask);
                }
                context.StartService(serviceIntent);
            }
        }
    }
}