using BeaconReceiverConnectorXamarin.IoTHub;
using BeaconReceiverConnectorXamarin.IoTHub.Data;
using BeaconReceiverXamarin.Connect;
using BeaconReceiverXamarin.Constants;
using BeaconReceiverXamarin.Data;
using BeaconReceiverXamarin.DB;
using BeaconReceiverConnectorXamarin.Interface;
using BeaconReceiverXamarin.Resource;
using BeaconReceiverXamarin.Tasks;
using BeaconReceiverConnectorXamarin.Utils;
using IoTHubJavaClientRewrittenInDotNet.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Forms;
using BeaconReceiverXamarin.Status;
using static BeaconReceiverConnectorXamarin.IoTHub.IotHubTransaction;

namespace BeaconReceiverXamarin.Tasks
{
    /**
     * Created by ryoma.saito on 16/08/23.
     */
    public class SendDeviceToCloudTask : SendCallback, SendSingleMessageCallback
    {
        private const String TAG = "SendDeviceToCloudTask";
        /**
         * Instance of Handler
         */
        private TaskManageHandler mHandler;
        /**
         * Instance of DatabaseAdapter
         */
        private DatabaseAdapter mDatabaseAdapter;
        /**
         * IotHubApiClient
         */
        private IotHubApiClient mClient;
        /**
         * 稼働系IoTHub
         */
        private String mActiveHostString;
        /**
         * 待機系IoTHub
         */
        private String mStandbyHostString;
        /**
         * Receiver
         */
        private Receiver mReceiver;
        private Timer mAnalysisTimer;
        /**
         * 分析間隔
         */
        private int mAnalysisInterval;
        private Timer mSendTimer;
        /**
         * 送信間隔
         */
        private int mSendInterval;
        /**
         * 分析基準時刻
         */
        private long criteriaTime;
        /**
         * Failover条件
         */
        private int mFailoverCondition;
        /**
         * Failback条件
         */
        private int mFailbackCondition;

        /**
         * RSSI値の種別
         */
        private int mRssiType = 0;

        public SendDeviceToCloudTask()
        {
            mDatabaseAdapter = DatabaseAdapter.getInstance();
        }

        public void setActiveHostString(String activeHostString)
        {
            mActiveHostString = activeHostString;
        }

        public void setStandbyHostString(String standbyHostString)
        {
            mStandbyHostString = standbyHostString;
        }

        public void setReceiver(Receiver receiver)
        {
            mReceiver = receiver;
        }

        public void setAnalysisInterval(int analysisInterval)
        {
            if ( analysisInterval < int.Parse(AppResource.analysis_interval_min))
                throw new ArgumentException(AppResource.setting_analysis_interval_label);

            mAnalysisInterval = analysisInterval* 1000;
        }

        public void setSendInterval(int sendInterval)
        {
            if ( sendInterval < int.Parse(AppResource.send_interval_min))
                throw new ArgumentException(AppResource.setting_send_interval_label);

            mSendInterval = sendInterval* 1000;
        }

        public void setFailoverCondition(int failoverCondition)
        {
            mFailoverCondition = failoverCondition;
        }

        public void setFailbackCondition(int failbackCondition)
        {
            mFailbackCondition = failbackCondition;
        }

        public void setRssiType(String rssiType)
        {
            mRssiType = int.Parse(rssiType);
        }

        /**
         * 定期実行タスク.
         * 分析間隔ごとに分析タスクを,送信間隔ごとに送信タスクをそれぞれキューに積む.
         */
        public void doTransaction() {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "doTransaction start", LogLevel.D);
            // 分析・送信タスク処理用スレッド開始.
            mHandler = new TaskManageHandler(this);
            mHandler.Start();

            // ApiClient取得.
            mClient = new IotHubApiClient(mActiveHostString, mStandbyHostString, mFailoverCondition, mFailbackCondition);

            // 基準時刻設定.
            criteriaTime = DateTimeUtils.GetCurrentTimeMillis();

            // 分析タスク追加.
            mAnalysisTimer = new Timer();
            mAnalysisTimer.Interval = mAnalysisInterval;
            mAnalysisTimer.Elapsed += (sender, e) => { addAnalysisMessage(); };
            mAnalysisTimer.Start();
            //mExecutor.scheduleAtFixedRate(
            //        mAddAnalysisRunnable, mAnalysisInterval, mAnalysisInterval, TimeUnit.MILLISECONDS);
            // 送信タスク追加.
            mSendTimer = new Timer();
            mSendTimer.Interval = mSendInterval;
            mSendTimer.Elapsed += (sender, e) => { addSendMessage(); };
            mSendTimer.Start();
            //mExecutor.scheduleAtFixedRate(
            //        mAddSendRunnable, mSendInterval, mSendInterval, TimeUnit.MILLISECONDS);
            DebugMessageUtils.GetInstance().WriteLog(TAG, "doTransaction end", LogLevel.D);
        }

        /**
         * サービス停止時MonitoringServiceから呼び出し.
         * 分析処理を中断し、タスクを安全に終了する
         */
        public void stopTransaction(bool sendRemainingData)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "stopTransaction", LogLevel.D);
            // 定期実行タスクを終了.
            mAnalysisTimer.Stop();
            mSendTimer.Stop();

            // タスク終了を送信.
            Message message = new Message();
            message.what = TaskManageHandler.STOP_HANDLER_TASK;
            message.obj = sendRemainingData;
            mHandler.sendMessage(message);
            DebugMessageUtils.GetInstance().WriteLog(TAG, "stopTransaction end", LogLevel.D);
        }

        /**
         * IoTHub送信コールバック
         */
        public void onSendResult(SendStatusCodeEnum statusCode, List<TouchParamBase> retryData)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "sendResultCode: " + statusCode.ToString(), LogLevel.I);

            switch (statusCode)
            {
                case SendStatusCodeEnum.OK:
                case SendStatusCodeEnum.OK_EMPTY:
                    //AppStatusManager.GetInstance().ChangeAppStatus(AppStatusEnum.Running);
                    break;

                // retry処理.
                default:
                    //AppStatusManager.GetInstance().ChangeAppStatus(AppStatusEnum.Error);
                    saveFailedData(retryData);
                    break;
            }

        }


        /**
         * 分析タスク追加
         */
        private void addAnalysisMessage () {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "addAnalysisMessage", LogLevel.D);
            mHandler.sendEmptyMessage(TaskManageHandler.ADD_ANALYSIS_TASK);

        }


        /**
         * 送信タスク追加
         */
        private void addSendMessage () {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "addSendMessage", LogLevel.D);
            mHandler.sendEmptyMessage(TaskManageHandler.ADD_SEND_TASK);
        }

        class Message
        {
            public int what;
            public object obj;
        }
        /**
         * 分析・送信タスク実行
         */
        private class TaskManageHandler
        {
            private bool stopped = true;
            // 分析タスク用.
            public const int ADD_ANALYSIS_TASK = 1;
            // 送信タスク用.
            public const int ADD_SEND_TASK = 2;
            // 停止用.
            public const int STOP_HANDLER_TASK = 3;
            private SendDeviceToCloudTask mSendDeviceToCloudTask;
            public TaskManageHandler(SendDeviceToCloudTask sendDeviceToCloudTask)
            {
                mSendDeviceToCloudTask = sendDeviceToCloudTask;
            }
            public void Start()
            {
                stopped = false;
            }
            public void sendEmptyMessage(int what)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "sendEmptyMessage what:" + what, LogLevel.D);
                Device.BeginInvokeOnMainThread(() => { handleMessage(new Message() { what = what }); });
                //mMessageQueue.AddLast(what);
            }
            public void sendMessage(Message message)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "sendMessage message.what:" + message.what, LogLevel.D);
                Device.BeginInvokeOnMainThread(() => { handleMessage(message); });
                //mMessageQueue.AddLast(what);
            }

            public void handleMessage(Message msg)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "handleMessage msg.what:" + msg.what, LogLevel.I);
                if (stopped) return;
                switch (msg.what)
                {

                    // 分析処理.
                    case ADD_ANALYSIS_TASK:
                        mSendDeviceToCloudTask.executeAnalysis();
                        break;

                    // 送信処理.
                    case ADD_SEND_TASK:
                        mSendDeviceToCloudTask.executeSend(false);
                        break;

                    case STOP_HANDLER_TASK:
                        bool sendRemainingData = (Boolean)msg.obj;
                        if (sendRemainingData)
                        {

                            mSendDeviceToCloudTask.executeSend(true);
                        }
                        stopped = true;
                        //mHandlerThread.quitSafely();
                        break;
                }
            }
        }

        /**
         * 分析実行メイン.
         */
        private void executeAnalysis()
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "executeAnalysis start", LogLevel.I);
            List<TouchData> analyzedRssiData = new List<TouchData>();

            // MaxRssiとCenterRssi切り替え.
            int rssi_type_max = int.Parse(SendRssiTypeConstants.RssiType.MAX.getId());
            int rssi_type_center = int.Parse(SendRssiTypeConstants.RssiType.CENTER.getId());
            int rssi_type_raw = int.Parse(SendRssiTypeConstants.RssiType.RAW.getId());

            if (mRssiType == rssi_type_max)
            {

                analyzedRssiData = mDatabaseAdapter.getMaxRssiTouchDataInMemory(
                        criteriaTime, criteriaTime + mAnalysisInterval, mReceiver);
            }
            else if (mRssiType == rssi_type_center)
            {

                analyzedRssiData = mDatabaseAdapter.getCenterRssiTouchDataInMemory(
                        criteriaTime, criteriaTime + mAnalysisInterval, mReceiver);
            }
            else if (mRssiType == rssi_type_raw)
            {
                analyzedRssiData = mDatabaseAdapter.getRawRssiTouchDataInMemory(
                        criteriaTime, criteriaTime + mAnalysisInterval, mReceiver);
            }

            criteriaTime = DateTimeUtils.GetCurrentTimeMillis();
            if (analyzedRssiData == null || analyzedRssiData.Count <= 0)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "no data to analize", LogLevel.V);
                return;
            }

            // 永続化テーブル登録.
            foreach (TouchData data in analyzedRssiData)
            {
                mDatabaseAdapter.addTouchData(data);
            }

            // 基準時刻変更.
            //criteriaTime += mAnalysisInterval;    //このやり方だとmAnalysisIntervalが小さい場合にずれが大きくなる…
            //criteriaTime = DateTimeUtils.GetCurrentTimeMillis();  //この位置だと0件だった時に更新されない…

            // インメモリテーブル削除.
            mDatabaseAdapter.deleteBeforeTouchInMemory(criteriaTime);
            DebugMessageUtils.GetInstance().WriteLog(TAG, "executeAnalysis end", LogLevel.D);
        }

        /**
         * 送信実行メイン.
         */
        private void executeSend(bool stopTask)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "executeSend start", LogLevel.I);
            // TouchTable内のデータをTouchBaseリストに入れる.
            List<TouchParamBase> touchDatas = mDatabaseAdapter.getAllDataInTouch(mReceiver);
            // データ取得後削除.
            mDatabaseAdapter.deleteAllTouch();
            if (stopTask)
            {
                mDatabaseAdapter.closePersistenceDatabase();
            }
            // データがなにもなければ送信しない.(DBがclosedの場合はnullが返る)
            if (touchDatas == null || touchDatas.Count <= 0)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "no data to send", LogLevel.V);
                return;
            }

            // ライブラリ返却保管用.
            bool result;

            // 引いてきたデータをIoTHubApiClientに回す.
            // ライブラリreturn false時処理.
            //try
            //{
            result = mClient.sendAnalyzedData((List<TouchParamBase>)touchDatas, this, this);    //メッセージをキューに入れるだけなので、I/O例外は発生しえない
            DebugMessageUtils.GetInstance().WriteLog(TAG, "executeSend end. result:" + result, LogLevel.D);
            //}
            //catch (IOException e)
            //{
            //    result = false;
            //    DebugMessageUtils.GetInstance().ShowMessage(TAG, "sendAnalyzedData failed", e, LogLevel.W);
            //}

            if (!result)
            {
                AppStatusManager.GetInstance().ChangeAppStatus(AppStatusEnum.Error);
                DebugMessageUtils.GetInstance().WriteLog(TAG, "sendAnalyzedData result false", LogLevel.W);
                saveFailedData(touchDatas);
            }
        }

        /**
         * 送信失敗データ保存
         */
        private void saveFailedData(List<TouchParamBase> failedDatas)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "saveFailedData start", LogLevel.I);
            foreach (TouchParamBase data in failedDatas)
            {
                mDatabaseAdapter.addTouchData((TouchData)data);
            }
            DebugMessageUtils.GetInstance().WriteLog(TAG, "saveFailedData end", LogLevel.I);
        }

        /**
         * エラーごとのメッセージを取得する.
         * @param errorMessage
         * @return
         */
        private String GetErrorMessage(String errorMessage)
        {
            if (errorMessage == AppResource.msg_error_iothub_sdk_auth_failure)
            {

                return AppResource.msg_error_iothub_auth_failure;
            }

            return AppResource.msg_error_iothub_connection_failure;
        }

        public void onSendSingleMessageResult(SendMode currentSendMode, SendStatusCodeEnum statusCode)
        {
            if (AppStatusManager.GetInstance().appStatus == AppStatusEnum.Stopped)  //停止済みの状態でレスポンスを受信してもステータスは変えない
                return;
            if (statusCode == SendStatusCodeEnum.OK)
            {
                if (currentSendMode == SendMode.FAILOVER)
                    AppStatusManager.GetInstance().ChangeAppStatus(AppStatusEnum.Failover);
                else
                    AppStatusManager.GetInstance().ChangeAppStatus(AppStatusEnum.Running);
            }
            else
                AppStatusManager.GetInstance().ChangeAppStatus(AppStatusEnum.Error);
        }
    }
}
