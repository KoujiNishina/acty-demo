using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BeaconReceiverConnectorXamarin.Utils;
using BeaconReceiverXamarin.Droid.Service;
using BeaconReceiverXamarin.Interface;
using BeaconReceiverXamarin.Status;
using Java.Lang;
using Java.Lang.Reflect;
using Xamarin.Forms;

[assembly: Dependency(typeof(StatusService_Droid))]
namespace BeaconReceiverXamarin.Droid.Service
{
    /// <summary>
    /// LEDでアプリの状態を表示する
    /// </summary>
    public class StatusService_Droid : IStatusService
    {
        const string TAG = "StatusService_Droid";
        private bool[] isLedOn = new bool[4] { false, false, false, false };

        enum LedType
        {
            RightRed = 1,   //非推奨
            RightGreen = 3, //非推奨
            LeftRed = 2,
            LeftGreen = 4
        }
        private PowerManager pm;
        private Method setLedStatusMethod;
        public StatusService_Droid()
        {
            pm = (PowerManager)Forms.Context.GetSystemService(Context.PowerService);
            Class clazz = null;
            try
            {
                clazz = Class.ForName("android.os.PowerManager");
                setLedStatusMethod = clazz.GetMethod("setLedStatus", new Class[] { Java.Lang.Boolean.Type, Java.Lang.Integer.Type });
            }
            catch (System.Exception e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "StatusService_Droid instantiation failed.", e, LogLevel.I);
            }
        }
        private Timer ledBlinkTimer;
        public void ShowStatus(AppStatusEnum appStatus)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "ShowStatus status:" + appStatus, LogLevel.D);
            switch (appStatus)
            {
                case AppStatusEnum.Starting:
                    if (ledBlinkTimer != null)
                    {
                        ledBlinkTimer.Stop();
                        ledBlinkTimer = null;
                    }
                    ToggleLed(true, LedType.LeftGreen);
                    ToggleLed(false, LedType.LeftRed);
                    break;
                case AppStatusEnum.Failover:
                    if (ledBlinkTimer != null)
                    {
                        ledBlinkTimer.Stop();
                        ledBlinkTimer = null;
                    }
                    ToggleLed(true, LedType.LeftGreen);
                    ToggleLed(false, LedType.LeftRed);
                    ledBlinkTimer = new Timer();
                    ledBlinkTimer.Elapsed += (sender, e) => { ToggleLed(!isLedOn[((int)LedType.LeftGreen) - 1], LedType.LeftGreen); ToggleLed(!isLedOn[((int)LedType.LeftRed) - 1], LedType.LeftRed); };
                    ledBlinkTimer.Interval = 500;
                    // タイマーを開始
                    ledBlinkTimer.Start();
                    break;
                case AppStatusEnum.Running:
                    if (ledBlinkTimer != null)
                    {
                        ledBlinkTimer.Stop();
                        ledBlinkTimer = null;
                    }
                    ToggleLed(false, LedType.LeftGreen);
                    ToggleLed(false, LedType.LeftRed);
                    ledBlinkTimer = new Timer();
                    ledBlinkTimer.Elapsed += (sender, e) => { ToggleLed(!isLedOn[((int)LedType.LeftGreen) - 1], LedType.LeftGreen); };
                    ledBlinkTimer.Interval = 500;

                    // タイマーを開始
                    ledBlinkTimer.Start();
                    break;
                case AppStatusEnum.Error:
                    if (ledBlinkTimer != null)
                    {
                        ledBlinkTimer.Stop();
                        ledBlinkTimer = null;
                    }
                    ToggleLed(false, LedType.LeftGreen);
                    ToggleLed(false, LedType.LeftRed);
                    ledBlinkTimer = new Timer();
                    ledBlinkTimer.Elapsed += (sender, e) => { ToggleLed(!isLedOn[((int)LedType.LeftRed) - 1], LedType.LeftRed); };
                    ledBlinkTimer.Interval = 500;

                    // タイマーを開始
                    ledBlinkTimer.Start();
                    break;
                case AppStatusEnum.ShuttedDownByError:
                    if (ledBlinkTimer != null)
                    {
                        ledBlinkTimer.Stop();
                        ledBlinkTimer = null;
                    }
                    ToggleLed(false, LedType.LeftGreen);
                    ToggleLed(true, LedType.LeftRed);
                    break;
                case AppStatusEnum.Stopped:
                    if (ledBlinkTimer != null)
                    {
                        ledBlinkTimer.Stop();
                        ledBlinkTimer = null;
                    }
                    ToggleLed(false, LedType.LeftGreen);
                    ToggleLed(false, LedType.LeftRed);
                    break;
            }
        }
        private void ToggleLed(bool onoff, LedType redType)
        {
            //Android.Util.Log.Verbose(TAG, "ToggleLed onoff:" + onoff + " redType:" + redType);
            if (setLedStatusMethod != null)
            {
                try
                {
                    setLedStatusMethod.Invoke(pm, onoff, (int)redType);
                    isLedOn[((int)redType) - 1] = onoff;
                }
                catch (System.Exception e)
                {
                    DebugMessageUtils.GetInstance().WriteLog(TAG, "ToggleLed failed. onoff:" + onoff + " redType:" + redType, e, LogLevel.I);
                }
            }
        }
    }
}