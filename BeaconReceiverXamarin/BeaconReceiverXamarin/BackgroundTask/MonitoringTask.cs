using BeaconReceiverConnectorXamarin.Interface;
using BeaconReceiverConnectorXamarin.Utils;
using BeaconReceiverXamarin.BLE;
using BeaconReceiverXamarin.Connect;
using BeaconReceiverXamarin.Constants;
using BeaconReceiverXamarin.Data;
using BeaconReceiverXamarin.DB;
using BeaconReceiverXamarin.Interface;
using BeaconReceiverXamarin.Location;
using BeaconReceiverXamarin.Resource;
using BeaconReceiverXamarin.Status;
using BeaconReceiverXamarin.Store;
using BeaconReceiverXamarin.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BeaconReceiverXamarin.BackgroundTask
{
    public class MonitoringTask : IDebugMessageHandler
    {
        const string TAG = "MonitoringTask";
        const int CHECK_REACHABILITY_INTERVAL_IN_MS = 5000;
        //この回数失敗した場合はステータスをエラー状態にする(リトライは続行する)
        const int CHECK_REACHABILITY_ERROR_COUNT = 25;
        const int FETCH_SETTINGS_INTERVAL_IN_MS = 5000;
        const int REGISTER_INTERVAL_IN_MS = 5000;
        /**
         * SendDeviceToCloudTask
         */
        private SendDeviceToCloudTask mSendDeviceToCloudTask;
        /**
         * BleScanner
         */
        private BleScanner mBleScanner;
        /**
         * LocationScanner
         */
        private LocationScanner mLocationScanner;
        /**
         * DatabaseAdapter
         */
        private DatabaseAdapter mDatabaseAdapter;
        /**
         * ServiceState
         */
        private bool isRunning = false;
        private IMainTaskCallback mCallback;
        private IMessageWriter mMessageWriter;
        private IBluetoothLE _bluetoothLe = CrossBluetoothLE.Current;
        public MonitoringTask(IMainTaskCallback callback)
        {
            mMessageWriter = DependencyService.Get<IMessageWriter>();
            DebugMessageUtils.SetDebugMessageHandler(this);
            mCallback = callback;
            // DBインスタンス取得.
            mDatabaseAdapter = DatabaseAdapter.getInstance();

            // LocationScannerインスタンス取得.
            mLocationScanner = new LocationScanner();
            mBleScanner = new BleScanner();

            // 分析・送信マネージャインスタンス取得.
            mSendDeviceToCloudTask = new SendDeviceToCloudTask();
        }
        //private ManualResetEvent waitHandle;
        public async void DoMainTask()
        {
            WriteLog(TAG, "DoMainTask Start", LogLevel.D);
            DebugUtils.GetInstance().TaskStartTime = DateTime.Now;

            /// ★★★TODO ActyG1はサービスを毎日再起動せず動かし続けることが予想される…サービス起動させたまま
            /// ★★★ログローテーションさせる仕組みが必要。
            OpenLogFile();
            CleanOldLogFiles();
            if (SetupDataStore.isMonitoring())
            {
                WriteLog(TAG, "前回サービスは異常終了しました。", LogLevel.W);
                AppStatusManager.GetInstance().ChangeAppStatus(AppStatusEnum.ShuttedDownByError);
            }
            else
            {
                AppStatusManager.GetInstance().ChangeAppStatus(AppStatusEnum.Starting);
            }
            SetupDataStore.setMonitoring(true);
            if (await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage) != PermissionStatus.Granted)
            {
                await HandleError("外部ファイルアクセスが許可されていません。設定画面から許可を与えてください。");
                return;
            }
            if (!_bluetoothLe.IsOn)
            {
                await HandleError("BLEをONにしてください");
                return;
            }
            _bluetoothLe.StateChanged += OnStateChanged;
            //waitHandle = new ManualResetEvent(false);
            //_adapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
            if (SetupDataStore.isFirst() || bool.Parse(AppResource.EverytimeReloadLocalSettingEnable))
            {

                // 【初回】
                // Web設定更新（ローカル）。
                SetupDataStore.updateWebSettingsPreferences(null);
                ShowMessage(TAG, "ローカル設定ファイルによる初期設定に成功しました。", LogLevel.I);
            }
            //ActyG1は端末起動後インターネット接続可能になるまでしばらく待つ必要がある
            await CustomerFrontApiClient.WaitUntilNetworkIsReacable(CHECK_REACHABILITY_INTERVAL_IN_MS,
                (isReachable, errorCount) =>
                {
                    if (isReachable)
                    {
                        ShowMessage(TAG, "インターネット接続成功", LogLevel.I);
                        AppStatusManager.GetInstance().ChangeAppStatus(AppStatusEnum.Starting);
                    }
                    else
                    {
                        if (errorCount >= CHECK_REACHABILITY_ERROR_COUNT)
                        {
                            AppStatusManager.GetInstance().ChangeAppStatus(AppStatusEnum.Error);
                        }
                    }
                });
            // Web設定更新（ダウンロード）
            if (bool.Parse(AppResource.WebSettingsDownloadEnable))
            {
                //一応リトライ処理を入れる。ActyG1では、WaitUntilNetworkIsReacableの直後にリクエストが失敗することがたまにある
                if (!await CustomerFrontApiClient.fetchSettings(5, FETCH_SETTINGS_INTERVAL_IN_MS))
                {
                    await HandleError("設定ファイルのダウンロードに失敗しました。");
                    return;
                }
                else
                {
                    ShowMessage(TAG, "設定ファイルのダウンロードに成功しました。", LogLevel.I);
                }
            }
            //  受信機登録済みチェック
            if (!SetupDataStore.isRegistered())
            {
                // 【受信機未登録】
                // 受信機情報登録
                //一応リトライ処理を入れる。ActyG1では、WaitUntilNetworkIsReacableの直後にリクエストが失敗することがたまにある
                if (!await CustomerFrontApiClient.register(5, REGISTER_INTERVAL_IN_MS))
                {
                    await HandleError("受信機情報登録に失敗しました。");
                    return;
                }
                else
                {
                    ShowMessage(TAG, "受信機情報登録に成功しました。", LogLevel.I);
                }
            }
            if (!await StartProcess())  //HandleErrorはStartProcess内で
            {
                return;
            }
            //waitHandle.WaitOne(Timeout.Infinite);

            bool isContinue = true;
            while (isContinue)
            {
                ServiceCommand command = mCallback.WaitForCommand();
                switch (command)
                {
                    case ServiceCommand.Stop:
                        await StopProcess(true);
                        isContinue = false;
                        break;
                }
            }
            WriteLog(TAG, "DoMainTask End", LogLevel.D);
        }

        private async Task<bool> StartProcess()
        {
            WriteLog(TAG, "StartProcess start", LogLevel.D);

            SettingsData data = SetupDataStore.getSettingsDataFromPreferences();
            // UUIDホワイトリスト設定. 最小電波強度設定.
            List<String> uuidWhiteList =
                    data.uuid_white_list;
            int allowedMinRssi = ConvertNullableIntToInt(data.allowed_min_rssi, -80);
            mBleScanner.setUuidWhiteList(uuidWhiteList);
            mBleScanner.setMinRssi(allowedMinRssi);
            String hostPrefix = "HostName=";
            String devicePrefix = ";DeviceId=";
            String accessKeyPrefix = ";SharedAccessKey=";

            // 稼働系IoTHub ConnectionString.
            String activeHostConnectionString = new StringBuilder()
                    .Append(hostPrefix)
                    .Append(data.active_host)
                    .Append(devicePrefix)
                    .Append(data.iothub_auth_info.name)
                    .Append(accessKeyPrefix)
                    .Append(data.iothub_auth_info.password)
                    .ToString();
            // 待機系IoTHub ConnectionString.
            String standbyHostConnectionString = new StringBuilder()
                    .Append(hostPrefix)
                    .Append(data.standby_host)
                    .Append(devicePrefix)
                    .Append(data.iothub_auth_info.name)
                    .Append(accessKeyPrefix)
                    .Append(data.iothub_auth_info.password)
                    .ToString();

            // 分析・送信間隔.
            int analysisInterval = ConvertNullableIntToInt(data.analysis_interval, 60);
            int sendInterval = ConvertNullableIntToInt(data.send_interval, 60);

            // フェイルオーバー・フェイルバック条件.
            int failoverCondition = ConvertNullableIntToInt(data.failover, 3);
            int failbackCondition = ConvertNullableIntToInt(data.failback, 3);

            // 受信機情報.
            Receiver receiver = new Receiver();
            receiver.name = data.iothub_auth_info.name;
            receiver.nickname = data.receiver_nickname;
            if ("" == receiver.nickname)
            {
                receiver.nickname = null;
            }

            // RSSI値の種別.
            String rssiType = SetupDataStore.getString(AppResource.setting_rssi_type_key, null);

            // 不正な設定値が入力されていた場合には終了.
            try
            {

                mSendDeviceToCloudTask.setReceiver(receiver);
                mSendDeviceToCloudTask.setActiveHostString(activeHostConnectionString);
                mSendDeviceToCloudTask.setStandbyHostString(standbyHostConnectionString);
                mSendDeviceToCloudTask.setAnalysisInterval(analysisInterval);
                mSendDeviceToCloudTask.setSendInterval(sendInterval);
                mSendDeviceToCloudTask.setFailoverCondition(failoverCondition);
                mSendDeviceToCloudTask.setFailbackCondition(failbackCondition);
                mSendDeviceToCloudTask.setRssiType(rssiType);

            }
            catch (ArgumentException e)
            {
                await HandleError(string.Format(AppResource.msg_error_illegal_settings, e.Message));
                //StopProcess(false);
                return false;

            }


            //// インメモリDB接続開始.
            //mDatabaseAdapter.openInMemoryDatabase();
            //mDatabaseAdapter.openPersistenceDatabase();

            // 分析・送信処理開始.
            try
            {

                mSendDeviceToCloudTask.doTransaction();

            }
            catch (Exception e) {

                //Toast.makeText(getApplicationContext(), AppResource.msg_error_iothub_auth_failure), Toast.LENGTH_LONG).show();
                //e.printStackTrace();
                //stopProcess(false);
                await HandleError(string.Format(AppResource.msg_error_iothub_auth_failure, e.Message));
                //StopProcess(false);
                return false;

            }

            // GPS許可チェック
            var isGranted = await CheckGpsPermissionAsync();
            if (!isGranted)
            {
                ShowMessage(TAG, "位置情報の使用が許可されていません。設定画面から位置情報の使用許可を与えてください。", LogLevel.W);
            }
            else if (!mLocationScanner.IsEnabled)
            {
                ShowMessage(TAG, "位置情報が利用できません。", LogLevel.W);
            }
            else
            {
                // 位置情報取得開始.
                var result = await mLocationScanner.startScan();
                if (!result)
                    ShowMessage(TAG, "位置情報取得開始に失敗しました。", LogLevel.W);
            }
            // BLEアドバタイズ取得開始.
            mBleScanner.startScan();

            // サービスの起動状態を内部で保持.
            isRunning = true;
            AppStatusManager.GetInstance().ChangeAppStatus(AppStatusEnum.Running);
            WriteLog(TAG, "StartProcess end", LogLevel.D);
            //await Task.Delay(20000);
            //throw new Exception();
            //waitHandle.Set();
            return true;
        }
        /**
         * サービス終了前処理.
         */
        private async Task StopProcess(bool sendRemainingData)
        {
            WriteLog(TAG, "StopProcess start", LogLevel.D);

            if (isRunning)
            {

                // BLEアドバタイズ取得終了.
                if (mBleScanner != null)
                {
                    await mBleScanner.stopScan();
                    mBleScanner = null;
                }

                // 位置情報取得終了.
                if (mLocationScanner != null)
                {
                    await mLocationScanner.stopScan();
                    mLocationScanner = null;
                }

                // 分析・送信処理終了.
                if (mSendDeviceToCloudTask != null)
                {
                    mSendDeviceToCloudTask.stopTransaction(sendRemainingData);
                    mSendDeviceToCloudTask = null;
                }

                // インメモリDBクローズ.
                if (mDatabaseAdapter != null)
                {
                    mDatabaseAdapter.closeInMemoryDatabase();
                    mDatabaseAdapter = null;
                }
            }

            WriteLog(TAG, "StopProcess end", LogLevel.D);
            CloseLogFile();
            isRunning = false;
            Finish();
            SetupDataStore.setMonitoring(false);
            AppStatusManager.GetInstance().ChangeAppStatus(AppStatusEnum.Stopped);
        }
        private int ConvertNullableIntToInt(int? intVal, int defaultVal)
        {
            return intVal == null ? defaultVal : (int)intVal;
        }
        private async void OnStateChanged(object sender, BluetoothStateChangedArgs e)
        {
            if (!_bluetoothLe.IsOn)
            {
                await HandleError("BLEがOFFになりました。");
            }
        }
        private async Task HandleError(string message)
        {
            await HandleError(message, null);
        }
        private async Task HandleError(string message, Exception ex)
        {
            ShowMessage(TAG, message, ex, LogLevel.E);
            await StopProcess(false);
        }
        public void ShowMessage(string tag, string message, LogLevel logLevel)
        {
            ShowMessage(tag, message, null, logLevel);
        }
        public void ShowMessage(string tag, string message, Exception ex, LogLevel logLevel)
        {
            WriteLog(tag, message, ex, logLevel);
            ShowToast(message);
        }
        public void ShowToast(string message)
        {
            mMessageWriter.ShowToast(message);
        }
        public void WriteLog(string tag, string message, LogLevel logLevel)
        {
            WriteLog(tag, message, null, logLevel);
        }
        public void WriteLog(string tag, string message, Exception ex, LogLevel logLevel)
        {
            WriteLogConsole(tag, message, ex, logLevel);
            WriteLogFile(message, ex, logLevel);
        }
        private void WriteLogConsole(string tag, string message, Exception ex, LogLevel logLevel)
        {
            if (AppConstants.OutputConsoleLogLevel <= logLevel)
                mMessageWriter.WriteLog(tag, message, ex, logLevel);
        }
        private void Finish()
        {
            //サービス終了
            mCallback.PostStopSelf();
        }

        #region Permission

        /// <summary>
        /// GPS許可チェック
        /// </summary>
        /// <returns>許可されている場合のみtrue</returns>
        private async Task<bool> CheckGpsPermissionAsync()
        {
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
            return status == PermissionStatus.Granted;
        }
        #endregion

        #region ログ出力

        //ログ出力ストリーム
        private StreamWriter logWriter;
        /// <summary>
        /// 24時間以上前に作成されたログファイル削除
        /// </summary>
        private void CleanOldLogFiles()
        {
            try
            {
                string sourceDirectory = mMessageWriter.GetIOFileDirPath();
                var logFiles = Directory.EnumerateFiles(sourceDirectory, "log_*.txt");
                foreach (string logFilePath in logFiles)
                {
                    FileInfo file = new FileInfo(logFilePath);
                    string fileName = file.Name;

                    DateTime fileDate = DateTime.MinValue;
                    DateTime.TryParseExact(fileName.Substring(4, 8), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out fileDate);
                    if ((DateTime.Today - fileDate) >= TimeSpan.FromDays(3))
                        File.Delete(logFilePath);
                }
            }
            catch (Exception ex)
            {
                ShowMessage(TAG, "古いログファイル削除に失敗しました。message:" + ex.Message, ex, LogLevel.E);
            }
        }

        private void OpenLogFile()
        {
            if (!Directory.Exists(mMessageWriter.GetIOFileDirPath()))
            {
                Directory.CreateDirectory(mMessageWriter.GetIOFileDirPath());
            }
            string logFilePath = mMessageWriter.GetIOFileDirPath() + "/log_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            try
            {
                logWriter = new StreamWriter(logFilePath, true, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                WriteLogConsole(TAG, "ログファイルを開けませんでした。filePath:" + logFilePath + " message:" + ex.Message, ex, LogLevel.E);
            }
        }
        private void WriteLogFile(string message, Exception e, LogLevel logLevel)
        {
            if (AppConstants.OutputFileLogLevel <= logLevel)
            {
                if (logWriter != null)
                {
                    try
                    {
                        logWriter.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + GetLogLevelStr(logLevel) + " " + message + (e == null ? "" : " - " + e.Message));
                        logWriter.Flush();
                    }
                    catch (Exception ex)
                    {
                        string msg = "ログ出力に失敗しました。 message:" + ex.Message;
                        WriteLogConsole(TAG, msg, ex, LogLevel.E);
                        ShowToast(msg);
                    }
                }
            }
        }
        private string GetLogLevelStr(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.I:
                    return "情報";
                case LogLevel.W:
                    return "警告";
                case LogLevel.E:
                    return "エラー";
                default:
                    return "デバッグ情報";
            }
        }
        private void CloseLogFile()
        {
            if (logWriter != null)
            {
                logWriter.Close();
            }
        }
        #endregion

    }
}
