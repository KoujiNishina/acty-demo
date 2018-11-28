using BeaconReceiverConnectorXamarin.IoTHub.Data;
using BeaconReceiverConnectorXamarin.Utils;
using IoTHubJavaClientRewrittenInDotNet.Util;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using static BeaconReceiverConnectorXamarin.IoTHub.IotHubDeviceClient;

namespace BeaconReceiverConnectorXamarin.IoTHub
{
    /**
     * Created by kenji.izumi on 2016/08/29.
     */
    public class IotHubTransaction : IotHubHttpAsyncTask.EventCallback
    {
        static readonly string TAG = typeof(IotHubTransaction).Name;
        public enum SendMode
        {

            NORMAL,
            RETRY,
            FAILOVER,
            FAILBACK,
        }

        private TransactionHandler mTransactionHandler;

        private bool mTransaction = false;

        private IotHubDeviceClient mDeviceClient;

        private int FAILOVER_CONDITION_RETRY_COUNT;
        private long FAILBACK_CONDITION_TIME_MILLIS;

        private int mSendRetryCount = 0;
        private long mFailBackStartTimeMillis;

        private SendMode mCurrentSendMode = SendMode.NORMAL;

        private SendCallback mSendCallback = null;
        private SendSingleMessageCallback mSendSingleMessageCallback = null;

        private List<TouchParamBase> mTouchList = null;

        private class TransactionQueueData
        {

            public String jsonBody;
            public int touchListIndex;

            public TransactionQueueData(String jsonBody, int touchListIndex)
            {

                this.jsonBody = jsonBody;
                this.touchListIndex = touchListIndex;
            }
        }

        private LinkedList<TransactionQueueData> mTransactionQueue = new LinkedList<TransactionQueueData>();

        public IotHubTransaction(String connectionStringActiveHost, String connectionStringStandbyHost, int failoverCondition, int failbackCondition)
        {
            //DebugUtils.GetInstance().CurrentSendMode = SendMode.NORMAL;
            mDeviceClient = new IotHubDeviceClient(connectionStringActiveHost, connectionStringStandbyHost);

            FAILOVER_CONDITION_RETRY_COUNT = failoverCondition;
            FAILBACK_CONDITION_TIME_MILLIS = failbackCondition* 1000;  // convert s -> ms
            mTransactionHandler = new TransactionHandler(this);
            //mTransactionHandler.Looper();
        }

        public bool isTransaction()
        {

            return mTransaction;
        }

        public void beginTransaction()
        {

            mTransaction = true;
        }

        public void setSendCallback(SendCallback sendCallback)
        {

            if (!mTransaction) throw new InvalidOperationException("Transaction not beginning.");

            mSendCallback = sendCallback;
        }

        public void setSendSingleMessageCallback(SendSingleMessageCallback sendSingleMessageCallback)
        {

            if (!mTransaction) throw new InvalidOperationException("Transaction not beginning.");

            mSendSingleMessageCallback = sendSingleMessageCallback;
        }
        public void setTouchList(List<TouchParamBase> touchList)
        {

            if (!mTransaction) throw new InvalidOperationException("Transaction not beginning.");

            mTouchList = touchList;
        }

        public void add(String jsonBody, int touchListIndex)
        {

            if (!mTransaction) throw new InvalidOperationException("Transaction not beginning.");

            TransactionQueueData queueData = new TransactionQueueData(jsonBody, touchListIndex);
            mTransactionQueue.AddLast(queueData);
        }
        public void commit()
        {

            if (!mTransaction) throw new InvalidOperationException("Transaction not beginning.");

            long currentTimeMillis = DateTimeUtils.GetCurrentTimeMillis();
            if (mCurrentSendMode == SendMode.FAILOVER &&
                    currentTimeMillis - mFailBackStartTimeMillis >= FAILBACK_CONDITION_TIME_MILLIS) {

                mCurrentSendMode = SendMode.FAILBACK;
            }

            mTransactionHandler.sendEmptyMessage(TransactionHandler.MSG_SEND_TO_IOTHUB);
        }

        public void fail()
        {

            mTransaction = false;
        }

        private class TransactionHandler
        {
            //public bool IsActive = true;
            //private LinkedList<int> mMessageQueue = new LinkedList<int>();
            public TransactionHandler(IotHubTransaction iotHubTransaction)
            {
                this.iotHubTransaction = iotHubTransaction;
            }
            private IotHubTransaction iotHubTransaction;
            public void sendEmptyMessage(int what)
            {
                Device.BeginInvokeOnMainThread(() => { handleMessage(what); });
                //mMessageQueue.AddLast(what);
            }
            public const int MSG_SEND_TO_IOTHUB = 1;
            //public async void Looper()
            //{
            //    while (IsActive)
            //    {
            //        int what = -1;
            //        if (mMessageQueue.First != null)
            //        {
            //            what = mMessageQueue.First.Value;
            //            mMessageQueue.RemoveFirst();
            //        }
            //        if (what > 0)
            //        {
            //            handleMessage(what);
            //        }
            //        await Task.Delay(10);
            //    }
            //}
            public void handleMessage(int what)
            {

                switch (what)
                {

                    case MSG_SEND_TO_IOTHUB:
                        iotHubTransaction.executeSendToIotHub();
                        break;
                }
            }
        }

        private void executeSendToIotHub()
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "executeSendToIotHub start mTransactionQueue.Count:" + mTransactionQueue.Count, LogLevel.I);
            TransactionQueueData queueData = null;
            if (mTransactionQueue.First != null)
            {
                queueData = mTransactionQueue.First.Value;
                mTransactionQueue.RemoveFirst();
            }
            if (queueData != null)
            {

                sendEventAsync(queueData);
            }
            else
            {
                // Transactionが無い場合は、処理を正常終了する。
                if (mSendCallback != null)
                {

                    mSendCallback.onSendResult(SendStatusCodeEnum.OK, null);
                }
                mTransaction = false;
            }
        }

        //private bool mIsSending = false;
        private void sendEventAsync(TransactionQueueData queueData)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "sendEventAsync start", LogLevel.D);
            String d2cMessage = queueData.jsonBody;

            switch (mCurrentSendMode)
            {

                case SendMode.NORMAL:
                case SendMode.RETRY:
                case SendMode.FAILBACK:
                    DebugMessageUtils.GetInstance().WriteLog(TAG, "sendEventAsync ACTIVE mCurrentSendMode:" + mCurrentSendMode/* + " mIsSending:" + mIsSending*/, LogLevel.I);
                    mDeviceClient.sendEventAsync(DestinationHost.ACTIVE, d2cMessage, this, queueData);
                    break;

                case SendMode.FAILOVER:
                    DebugMessageUtils.GetInstance().WriteLog(TAG, "sendEventAsync FAILOVER mCurrentSendMode:" + mCurrentSendMode/* + " mIsSending:" + mIsSending*/, LogLevel.I);
                    mDeviceClient.sendEventAsync(DestinationHost.STANDBY, d2cMessage, this, queueData);
                    break;
            }
            //mIsSending = true;
            DebugMessageUtils.GetInstance().WriteLog(TAG, "sendEventAsync end", LogLevel.D);
        }

        public void execute(SendStatusCodeEnum responseStatus, Object callbackContext)
        {
            //mIsSending = false;
            DebugMessageUtils.GetInstance().WriteLog(TAG, "execute start responseStatus:" + responseStatus + " mCurrentSendMode: " + mCurrentSendMode, LogLevel.I);
            switch (responseStatus)
            {

                case SendStatusCodeEnum.OK:
                case SendStatusCodeEnum.OK_EMPTY:
                    // 正常時処理
                    executeSuccessStatus(responseStatus, callbackContext);
                    break;
                default:
                    // エラー処理
                    executeErrorStatus(responseStatus, callbackContext);
                    break;
            }
        }

        private void executeSuccessStatus(SendStatusCodeEnum responseStatus, Object callbackContext)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "executeSuccessStatus start responseStatus:" + responseStatus + " mCurrentSendMode: " + mCurrentSendMode, LogLevel.D);
            //mSendCallback.onSendResult(SendStatusCodeEnum.OK, null);
            if (mSendSingleMessageCallback != null)
                mSendSingleMessageCallback.onSendSingleMessageResult(mCurrentSendMode, SendStatusCodeEnum.OK);
            switch (mCurrentSendMode)
            {

                case SendMode.FAILBACK:
                case SendMode.RETRY:
                    mCurrentSendMode = SendMode.NORMAL;
                    // 次のTransactionを実行
                    mTransactionHandler.sendEmptyMessage(TransactionHandler.MSG_SEND_TO_IOTHUB);
                    break;
                // FALL THROUGH
                case SendMode.FAILOVER:
                case SendMode.NORMAL:
                    // 次のTransactionを実行
                    mTransactionHandler.sendEmptyMessage(TransactionHandler.MSG_SEND_TO_IOTHUB);
                    break;
            }
            DebugMessageUtils.GetInstance().WriteLog(TAG, "executeSuccessStatus end", LogLevel.D);
        }

        private void executeErrorStatus(SendStatusCodeEnum responseStatus, Object callbackContext)
        {

            DebugMessageUtils.GetInstance().WriteLog(TAG, "executeErrorStatus start responseStatus:" + responseStatus + " mCurrentSendMode: " + mCurrentSendMode, LogLevel.D);
            if (mSendSingleMessageCallback != null)
                mSendSingleMessageCallback.onSendSingleMessageResult(mCurrentSendMode, responseStatus);
            switch (responseStatus)
            {

                case SendStatusCodeEnum.BAD_FORMAT:
                case SendStatusCodeEnum.UNAUTHORIZED:
                    // 次回送信を稼働系に切替の上、エラー終了する
                    mCurrentSendMode = SendMode.NORMAL;
                    sendErrorCallback(responseStatus, callbackContext);
                    mTransaction = false;
                    break;

                default:
                    executeFailStatus(responseStatus, callbackContext);
                    break;
            }
            DebugMessageUtils.GetInstance().WriteLog(TAG, "executeErrorStatus end", LogLevel.D);
        }

        private void executeFailStatus(SendStatusCodeEnum responseStatus, Object callbackContext)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "executeFailStatus start responseStatus:" + responseStatus, LogLevel.I);
            TransactionQueueData queueData = (TransactionQueueData)callbackContext;

            switch (mCurrentSendMode)
            {

                case SendMode.NORMAL:
                    mCurrentSendMode = SendMode.RETRY;
                    mSendRetryCount = 1;
                    // 送信失敗データを積み直して、Transactionを再実行
                    mTransactionQueue.AddFirst(queueData);
                    mTransactionHandler.sendEmptyMessage(TransactionHandler.MSG_SEND_TO_IOTHUB);
                    break;

                case SendMode.RETRY:
                    mSendRetryCount++;
                    if (mSendRetryCount >= FAILOVER_CONDITION_RETRY_COUNT)
                    {

                        mCurrentSendMode = SendMode.FAILOVER;
                        mFailBackStartTimeMillis = DateTimeUtils.GetCurrentTimeMillis();
                    }
                    // 送信失敗データを積み直して、Transactionを再実行
                    mTransactionQueue.AddFirst(queueData);
                    mTransactionHandler.sendEmptyMessage(TransactionHandler.MSG_SEND_TO_IOTHUB);
                    break;

                case SendMode.FAILOVER:
                    sendErrorCallback(responseStatus, callbackContext);
                    mTransaction = false;
                    break;

                case SendMode.FAILBACK:
                    mCurrentSendMode = SendMode.FAILOVER;
                    mFailBackStartTimeMillis = DateTimeUtils.GetCurrentTimeMillis();
                    // 送信失敗データを積み直して、Transactionを再実行
                    mTransactionQueue.AddFirst(queueData);
                    mTransactionHandler.sendEmptyMessage(TransactionHandler.MSG_SEND_TO_IOTHUB);
                    break;
            }
            DebugMessageUtils.GetInstance().WriteLog(TAG, "executeFailStatus end", LogLevel.D);
        }

        //private SendMode CurrentSendMode
        //{
        //    set
        //    {
        //        mCurrentSendMode = value;
        //        DebugUtils.GetInstance().CurrentSendMode = mCurrentSendMode;
        //    }
        //}

        private void sendErrorCallback(SendStatusCodeEnum responseStatus, Object callbackContext)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "sendErrorCallback start responseStatus:" + responseStatus, LogLevel.D);
            if (mSendCallback != null)
            {

                TransactionQueueData queueData = (TransactionQueueData)callbackContext;

                List<TouchParamBase> retryData = new List<TouchParamBase>();
                if (mTouchList != null)
                {

                    for (int i = queueData.touchListIndex; i < mTouchList.Count; i++)
                    {

                        retryData.Add(mTouchList[i]);
                    }
                }
                mSendCallback.onSendResult(responseStatus, retryData);
            }
            DebugMessageUtils.GetInstance().WriteLog(TAG, "sendErrorCallback end", LogLevel.D);
        }
    }
}
