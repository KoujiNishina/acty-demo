using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace BeaconReceiverXamarin.Droid.Receivers
{
    /// <summary>
    /// 端末シャットダウンイベントを受けてmonitoringプリファレンスをfalseにするレシーバー。
    /// 
    /// MainServiceが異常終了した場合と、MainService実行中に端末がシャットダウンされた場合は、何もしなければmonitoringがtrueのままになるが、
    /// このレシーバーにより後者の場合はmonitoringがfalseになるので、monitoringがtrueのままならば、MainServiceが前回異常終了したと判定できる。
    /// 
    /// ⇒あまり重要な機能ではないです。起動時に「前回異常終了しました」とエラーログ出力・LED表示する用途にしか使っていない。
    /// </summary>
    [BroadcastReceiver]
    [IntentFilter(new[] { Intent.ActionShutdown })]
    public class ShutdownReceiver : BroadcastReceiver
    {
        static readonly string TAG = typeof(ShutdownReceiver).Name;
        public ShutdownReceiver() : base()
        {
        }
        public override void OnReceive(Context context, Intent intent)
        {
            Log.Debug(TAG, "OnReceive");
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            var editor = prefs.Edit();
            editor.PutBoolean("monitoring", false);
            editor.Apply();
        }
    }
}