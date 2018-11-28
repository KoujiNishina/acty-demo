using BeaconReceiverConnectorXamarin.IoTHub.Data;
using BeaconReceiverConnectorXamarin.Utils;
using BeaconReceiverXamarin.Data;
using IoTHubJavaClientRewrittenInDotNet.Util;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BeaconReceiverXamarin.DB
{
    /**
     * Created by chiho.adachi on 2016/08/03.
     */
    public class DatabaseAdapter
    {
        static readonly string TAG = typeof(DatabaseAdapter).Name;
        /** DB名 */
        private const String DB_NAME = "BleApp";

        /** Singleton */
        private static DatabaseAdapter mAdapter = new DatabaseAdapter();
        
        /** インメモリDBインスタンス */
        private SQLiteConnection memoryDatabase;
        private SQLiteConnection persistanceDatabase;


        public static DatabaseAdapter getInstance()
        {
            mAdapter.openInMemoryDatabase();
            mAdapter.openPersistenceDatabase();
            return mAdapter;
        }

        private DatabaseAdapter()
        {
        }

        public void openInMemoryDatabase()
        {
            if (memoryDatabase == null)
            {
                var inMemoryOpenHelper = new InMemorySQLiteOpenHelper();
                memoryDatabase = inMemoryOpenHelper.getWritableDatabase();
            }
        }
        public void openPersistenceDatabase()
        {
            if (persistanceDatabase == null)
            {
                var persistenceOpenHelper = new PersistenceSQLiteOpenHelper();
                persistanceDatabase = persistenceOpenHelper.getWritableDatabase();
            }
        }

        public void closeInMemoryDatabase()
        {
            if (memoryDatabase != null)
            {
                memoryDatabase.Close();
                memoryDatabase = null;
            }
        }

        public void closePersistenceDatabase()
        {
            if (persistanceDatabase != null)
            {
                persistanceDatabase.Close();
                persistanceDatabase = null;
            }
        }

        /**
         * 【Touch(Memory)】インメモリテーブルデータ全件取得
         *
         * @param receiver
         * @return
         */
        public List<TouchData> getAllDataInMemory(Receiver receiver)
        {
            List<TouchData> retList = null;
            if (memoryDatabase != null)
            {
                retList = TouchTableAccessHelper.GetAllData(memoryDatabase, receiver);
            }

            return retList;
        }

        /**
         * 【Touch(Persistence)】永続化テーブルデータ全件取得
         *
         * @param receiver
         * @return
         */
        public List<TouchParamBase> getAllDataInTouch(Receiver receiver)
        {
            List<TouchParamBase> retList = null;
            if (persistanceDatabase != null)
            {
                retList = TouchTableAccessHelper.getAllDataFromPersistence(persistanceDatabase, receiver);
            }

            return retList;
        }

        /**
         * 【Touch(Memory)】受信ビーコン情報取得(RSSI最大値)
         *
         * @param dateTimeMillisFrom 取得開始時間
         * @param dateTimeMillisTo 取得終了時間
         * @return
         */
        public List<TouchData> getMaxRssiTouchDataInMemory(long dateTimeMillisFrom, long dateTimeMillisTo, Receiver receiver)
        {
            List<TouchData> retList = null;
            if (memoryDatabase != null)
            {
                retList = TouchTableAccessHelper.getMaxRssiData(
                        memoryDatabase, dateTimeMillisFrom.ToString(), dateTimeMillisTo.ToString(), receiver);
            }

            return retList;
        }

        /**
         * 【Touch(Memory)】受信ビーコン情報取得(RSSI中央値)
         *
         * @param dateTimeMillisFrom 取得開始時間
         * @param dateTimeMillisTo 取得終了時間
         * @return
         */
        public List<TouchData> getCenterRssiTouchDataInMemory(long dateTimeMillisFrom, long dateTimeMillisTo, Receiver receiver)
        {
            List<TouchData> retList = null;
            if (memoryDatabase != null)
            {
                retList = TouchTableAccessHelper.getCenterRssiData(
                        memoryDatabase, dateTimeMillisFrom.ToString(), dateTimeMillisTo.ToString(), receiver);
            }

            return retList;
        }
        /**
         * 【Touch(Memory)】受信ビーコン情報取得(RSSI生値)
         *
         * @param dateTimeMillisFrom 取得開始時間
         * @param dateTimeMillisTo 取得終了時間
         * @return
         */
        public List<TouchData> getRawRssiTouchDataInMemory(long dateTimeMillisFrom, long dateTimeMillisTo, Receiver receiver)
        {
            List<TouchData> retList = null;
            if (memoryDatabase != null)
            {
                retList = TouchTableAccessHelper.getRawRssiData(
                        memoryDatabase, dateTimeMillisFrom.ToString(), dateTimeMillisTo.ToString(), receiver);
            }

            return retList;
        }

        /**
         * 【Touch(Memory)】受信ビーコン情報登録
         *
         * @param data
         */
        public void addTouchDataInMemory(BeaconData data)
        {
            //DebugMessageUtils.GetInstance().WriteLog(TAG, "addTouchDataInMemory memoryDatabase:" + memoryDatabase + " data:" + data, LogLevel.I);
            if (memoryDatabase != null && data != null)
            {
                TouchTableAccessHelper.addData(memoryDatabase, data);
            }
        }

        /**
         * 【Touch(Persistence)】受信ビーコン情報登録
         *
         * @param data
         */
        public void addTouchData(TouchData data)
        {
            //DebugMessageUtils.GetInstance().WriteLog(TAG, "addTouchData persistanceDatabase:" + memoryDatabase + " data:" + data, LogLevel.I);
            if (persistanceDatabase != null && data != null)
            {
                TouchTableAccessHelper.addData(persistanceDatabase, data);
            }
        }

        /**
         * 【Touch(Memory)】sendDateTimeMillis以前のデータを削除
         *
         * @param sendDateTimeMillis 送信日時ミリ秒
         */
        public void deleteBeforeTouchInMemory(long sendDateTimeMillis)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "deleteBeforeTouchInMemory start sendDateTimeMillis:" + sendDateTimeMillis, LogLevel.I);
            if (memoryDatabase != null)
            {
                TouchTableAccessHelper.deleteBefore(memoryDatabase, sendDateTimeMillis.ToString());
            }
        }

        /**
         * 【Touch(Persistence)】sendDateTimeMillis以前のデータを削除
         *
         * @param sendDateTimeMillis 送信日時ミリ秒
         */
        public void deleteBeforeTouch(long sendDateTimeMillis)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "deleteBeforeTouch start sendDateTimeMillis:" + sendDateTimeMillis, LogLevel.I);
            if (persistanceDatabase != null)
            {
                TouchTableAccessHelper.deleteBefore(persistanceDatabase, DateTimeUtils.ConvertIsoDatetimeTimeZone(sendDateTimeMillis));
            }
        }

        /**
         * 【Touch(Persistence)】データを全件削除
         */
        public void deleteAllTouch()
        {
            if (persistanceDatabase != null)
            {
                TouchTableAccessHelper.deleteAll(persistanceDatabase);
            }
        }

        /**
         * インメモリDB用SqLiteOpenHelperクラス
         */
        class InMemorySQLiteOpenHelper : SQLiteOpenHelper
        {

            public InMemorySQLiteOpenHelper() : base(":memory:", typeof(TouchDataRecord))
            {
            }
        }

        /**
         * 永続化DB用SqLiteOpenHelperクラス
         */
        class PersistenceSQLiteOpenHelper : SQLiteOpenHelper
        {

            public PersistenceSQLiteOpenHelper() : base(DependencyService.Get<IFileHelper>().GetLocalFilePath(DB_NAME + ".db3"), typeof(TouchDataRecord))
            { }

        }

    }
}
