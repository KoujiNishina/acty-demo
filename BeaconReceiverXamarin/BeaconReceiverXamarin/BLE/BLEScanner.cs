using BeaconReceiverXamarin.Data;
using BeaconReceiverXamarin.DB;
using BeaconReceiverXamarin.Location;
using BeaconReceiverConnectorXamarin.Utils;
using IoTHubJavaClientRewrittenInDotNet.Util;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using BeaconReceiverConnectorXamarin.Interface;
using System.Threading.Tasks;
using System.Timers;
using BeaconReceiverXamarin.Utils;

namespace BeaconReceiverXamarin.BLE
{
    public class BleScanner
    {
        const string TAG = "BleScanner";
        private Plugin.BLE.Abstractions.Contracts.IAdapter _adapter;
        /** 位置情報取得用 */
        private LocationCache mLocationCache;
        /** ビーコン・位置情報格納用 */
        private BeaconData mBeaconData;
        /** DatabaseAdapter */
        private DatabaseAdapter mDatabaseAdapter;
        /** UUIDホワイトリスト */
        private List<String> mUuidWhiteList;
        /** UUIDホワイトリストに1件も登録がないかどうか */
        private bool isBlank = true;
        /** 許容最小電波強度 */
        private int mMinRssi;
        /** 位置情報 */
        private Data.Location mLocation;
        private CancellationTokenSource _cancellationTokenSource;
        //private Task mScanCheck;
        //private bool mIsScanned = false;
        private System.Timers.Timer mScanCountTimer;

        private int scanCounter = 0;
        //private Dictionary<string, int> scanCounterMap = new Dictionary<string, int>();
        private HashSet<string> scanCounterSet = new HashSet<string>();



        public BleScanner()
        {
            mDatabaseAdapter = DatabaseAdapter.getInstance();
            _adapter = CrossBluetoothLE.Current.Adapter;
            _adapter.DeviceAdvertised += DeviceAdvertised;
            // 位置情報取得.
            mLocationCache = LocationCache.getInstance();
            mLocation = mLocationCache.getLocation();
        }
        /** BLEスキャン開始(onLeScanコールバック) */
        public void startScan()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _adapter.ScanMode = ScanMode.LowLatency;
            _adapter.ScanTimeout = int.MaxValue;
            _adapter.StartScanningForDevicesAsync(cancellationToken: _cancellationTokenSource.Token, allowDuplicatesKey: true);
            // 分析タスク追加.
            //mScanCheck = Task.Run(async () => { await Task.Delay(1000 * 60); if (!mIsScanned) startScan(); });  //1分間スキャンされない場合はリスタート
            //mIsScanned = false;
            //mScanCountTimer
            mScanCountTimer = new System.Timers.Timer();
            mScanCountTimer.Interval = 10000;
            mScanCountTimer.Elapsed += (sender, e) => {
                //var total = 0;
                //foreach (var val in scanCounterMap.Values)
                //    total += val;
                //DebugMessageUtils.GetInstance().ShowToast("Advertises:" + scanCounter + "\nAdvertises2:" + total + "\nDevices:" + scanCounterMap.Keys.Count + "\nin last 10 sec."); scanCounter = 0; scanCounterMap.Clear(); };
                //DebugMessageUtils.GetInstance().ShowToast("Advertises:" + scanCounter + "\nDevices:" + scanCounterMap.Keys.Count + "\nin last 10 sec."); scanCounter = 0; scanCounterMap.Clear();
                DebugMessageUtils.GetInstance().ShowToast("Advertises:" + scanCounter + "\nDevices:" + scanCounterSet.Count + "\nin last 10 sec."); scanCounter = 0; scanCounterSet.Clear();
            };
            mScanCountTimer.Start();

            DebugMessageUtils.GetInstance().WriteLog(TAG, "onStartScan", LogLevel.I);
        }

        /** BLEスキャン終了 */
        public async Task stopScan()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            await _adapter.StopScanningForDevicesAsync();
            mScanCountTimer.Stop();
            DebugMessageUtils.GetInstance().WriteLog(TAG, "stopScan", LogLevel.I);
        }
        /**
         * UUIDホワイトリスト設定
         *
         * @param uuidWhiteList
         */
        public void setUuidWhiteList(List<String> uuidWhiteList)
        {
            mUuidWhiteList = uuidWhiteList;
            if (mUuidWhiteList.Count > 0)
            {
                isBlank = false;
            }
        }

        /**
         * 取得したUUIDがホワイトリストにのっているか判定する
         * @param uuid
         * @return
         */
        private bool IsInWhiteList(String uuid, int major, int minor)
        {
            foreach (String listUuid in mUuidWhiteList)
            {
                var majorAndMinor = UuidUtils.GetMajorAndMinor(listUuid);
                if (majorAndMinor[0] == -1 && majorAndMinor[1] == -1)   //UUIDのみ
                {
                    if (uuid.StartsWith(listUuid))
                    {
                        return true;
                    }
                }
                else if (majorAndMinor[0] != -1 && majorAndMinor[1] == -1)  //UUID-major
                {
                    if ((uuid + "-" + major) == (listUuid))
                    {
                        return true;
                    }
                }
                else  //UUID-major-minor
                {
                    if ((uuid + "-" + major + "-" + minor) == (listUuid))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /**
         * 最小電波強度設定
         *
         * @param minRssi
         */
        public void setMinRssi(int minRssi)
        {
            mMinRssi = minRssi;
        }

        /**
         * BLEアドバタイズ取得時コールバック.
         * ビーコン情報と位置情報をインメモリDB格納.
         *
         * @param device
         * @param rssi
         * @param scanRecord
         */
        public void onLeScan(IDevice device)
        {
            //mIsScanned = true;
            scanCounter++;
            //if (scanCounterMap.ContainsKey(device.Id.ToString())) scanCounterMap[device.Id.ToString()]++;
            //else scanCounterMap.Add(device.Id.ToString(), 1);
            scanCounterSet.Add(device.Id.ToString());
            DebugMessageUtils.GetInstance().WriteLog(
                TAG,
                "onLeScan start" +
                " rssi: " + device.Rssi +
                " mMinRssi: " + mMinRssi +
                " Id:" + device.Id +
                " Name:" + device.Name +
                " device.AdvertisementRecords.Count:" + device.AdvertisementRecords.Count
                , LogLevel.D);
            // 許容するRSSI以下の値ならオンメモリテーブルに保存しない.
            if (device.Rssi < mMinRssi)
            {
                return;
            }

            if (device.AdvertisementRecords.Count < 1)
            {
                return;
            }
            byte[] manufacturerSpecificData = null;
            foreach (var rec in device.AdvertisementRecords)
            {
                //DebugMessageUtils.GetInstance().WriteLog(
                //    TAG,
                //    "AdvertisementRecord rec.Type: " + rec.Type +
                //    " rec.Data.Length: " + rec.Data.Length +
                //    " rec.Data[0]:" + (rec.Data.Length > 0 ? rec.Data[0] : 0) +
                //    " rec.Data[1]:" + (rec.Data.Length > 1 ? rec.Data[1] : 0) +
                //    " rec.Data[2]:" + (rec.Data.Length > 2 ? rec.Data[2] : 0) +
                //    " rec.Data[3]:" + (rec.Data.Length > 3 ? rec.Data[3] : 0)
                //, LogLevel.V);
                if (rec.Type == AdvertisementRecordType.ManufacturerSpecificData
                    && rec.Data.Length >= 25
                    && 
                    (   rec.Data[0] == 0x4c && rec.Data[1] == 0x00  //企業識別子 0x004C(Apple)
                            && rec.Data[2] == 0x02 && rec.Data[3] == 0x15   //iBeacon 識別子 0x1502
                        || 
                            rec.Data[2] == 0xbe && rec.Data[3] == 0xac   //AltBeacon  識別子 0xBEAC
                    )

                )
                {
                    manufacturerSpecificData = rec.Data;
                    break;
                }
            }
            if (manufacturerSpecificData == null)
            {
                return;
            }
            // ビーコン情報取得.
            Beacon beacon = new Beacon(manufacturerSpecificData);

            // UUIDホワイトリストが1件以上で、かつ
            // UUIDホワイトリスト対象外ならオンメモリDBに保存しない.
            if (!isBlank && !IsInWhiteList(beacon.uuid, (int)beacon.major, (int)beacon.minor))
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "beacon not in the whitelist: beacon.uuid:" + beacon.uuid + " beacon.major:" + beacon.major + " beacon.minor:" + beacon.minor, LogLevel.D);
                return;
            }

            // ビーコンに名前がついている場合のみ取得.
            if (!string.IsNullOrEmpty(device.Name))
            {
                beacon.nickname = device.Name;
            }
            DebugMessageUtils.GetInstance().WriteLog(TAG, "beacon found.: " +
                    beacon.uuid + "-" +
                    beacon.major + "-" +
                    beacon.minor + " rssi:" +
                    device.Rssi, LogLevel.I);

            // 位置情報取得.
            mLocation = mLocationCache.getLocation();

            // 現在時刻取得.
            //String occurred_date = DateTimeUtils.GetCurrentTimeMillis().ToString();
            String occurred_date = DateTimeUtils.GetCurrentTimeString();

            mBeaconData = new BeaconData(beacon, mLocation, device.Rssi, occurred_date);

            // オンメモリDB格納.
            mDatabaseAdapter.addTouchDataInMemory(mBeaconData);
            DebugMessageUtils.GetInstance().WriteLog(
                 TAG,
                 "onLeScan end" +
                 " rssi: " + device.Rssi +
                 " mMinRssi: " + mMinRssi +
                 " Id:" + device.Id +
                 " Name:" + device.Name
                 , LogLevel.D);
        }
        private void DeviceAdvertised(object sender, DeviceEventArgs args)
        {
            onLeScan(args.Device);
        }
    }
}
