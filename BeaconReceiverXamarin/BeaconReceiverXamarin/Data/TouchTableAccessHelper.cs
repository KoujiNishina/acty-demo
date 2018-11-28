using BeaconReceiverConnectorXamarin.IoTHub.Data;
using BeaconReceiverConnectorXamarin.Interface;
using BeaconReceiverConnectorXamarin.Utils;
using IoTHubJavaClientRewrittenInDotNet.Util;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace BeaconReceiverXamarin.Data
{
    /**
     * すれ違いテーブルAccessHelper
     *
     * Created by chiho.adachi on 2016/08/03.
     */
    public class TouchTableAccessHelper
    {
        const string TAG = "TouchTableAccessHelper";
        /** テーブル名 */
        public const String TABLE_NAME = "TouchDataRecord";
        /** ID */
        public const String COLUMN_ID = "_id";
        /** ビーコン識別名(一意) */
        public const String COLUMN_KEY_NAME = "key_name";
        /** UUID */
        public const String COLUMN_UUID = "uuid";
        /** MajorID */
        public const String COLUMN_MAJOR_ID = "major";
        /** MinorID */
        public const String COLUMN_MINOR_ID = "minor";
        /** ビーコン名 */
        public const String COLUMN_NICKNAME = "nickname";
        /** MeasuredPower */
        public const String COLUMN_MEASURED_POWER = "measured_power";
        /** 情報受信時刻ミリ秒 */
        public const String COLUMN_RECEIVED_BEACON_DATE = "recv_beacon_date";
        /** 緯度 */
        public const String COLUMN_LAT = "latitude";
        /** 経度 */
        public const String COLUMN_LON = "lon";
        /** 測位日時 */
        public const String COLUMN_POSITIONING_LOCATION_DATE = "recv_location_date";
        /** 受信電波強度 */
        public const String COLUMN_RSSI = "rssi";

        private TouchTableAccessHelper() { }
        /**
         * 指定テーブルからデータを全件取得
         *
         * @param db
         * @param receiver
         * @return
         */
        public static List<TouchData> GetAllData(SQLiteConnection db, Receiver receiver)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "GetAllData start", LogLevel.D);
            List<TouchData> retList = new List<TouchData>();
            try
            {
                IEnumerable<TouchDataRecord> touches = db.Table<TouchDataRecord>();
                foreach (TouchDataRecord org in touches)
                {
                    var ret = createSendData(org);
                    ret.receiver = receiver;
                    retList.Add(ret);
                }
                DebugMessageUtils.GetInstance().WriteLog(TAG, "GetAllData end retList.Count:" + retList.Count, LogLevel.I);
            }
            catch (Exception e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "GetAllData failed", e, LogLevel.E);
                db.Close();
                throw e;
            }
            return retList;

        }

        /**
         * インメモリテーブルからの取り出し時には、occurred_atやfixed_atをyyyy/MM/ddThh:mm:ss形式に変換したが
         * t_touchテーブルからの取り出し時には、そのまま取り出したいため、別メソッド化している.
         * (そのまま取り出さないと常に'1970-01-01T09:00:00'になるため)
         *
         * @param db
         * @param receiver
         * @return
         */
        public static List<TouchParamBase> getAllDataFromPersistence(SQLiteConnection db, Receiver receiver)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "getAllDataFromPersistence start", LogLevel.D);
            List<TouchParamBase> retList = new List<TouchParamBase>();
            try
            {
                IEnumerable<TouchDataRecord> touches = db.Table<TouchDataRecord>();
                foreach (TouchDataRecord org in touches)
                {
                    var ret = getSendData(org);
                    ret.receiver = receiver;
                    retList.Add(ret);
                }
                DebugMessageUtils.GetInstance().WriteLog(TAG, "getAllDataFromPersistence end retList.Count:" + retList.Count, LogLevel.I);
            }
            catch (Exception e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "getAllDataFromPersistence failed", e, LogLevel.E);
                db.Close();
                throw e;
            }
            return retList;
        }

        /**
         * 受信ビーコン情報取得
         * UUIDごとにRSSI最大値データを取得
         *
         * @param db
         * @param dateTimeMillisFrom 取得開始時間
         * @param dateTimeMillisTo 取得終了時間
         * @param receiver レシーバ情報
         * @return
         */
        public static List<TouchData> getMaxRssiData(SQLiteConnection db, String dateTimeMillisFrom, String dateTimeMillisTo, Receiver receiver)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "getMaxRssiData start dateTimeMillisFrom:" + dateTimeMillisFrom + " dateTimeFrom:" + DateTimeUtils.ConvertIsoDatetimeTimeZone(long.Parse(dateTimeMillisFrom)) + " dateTimeMillisTo:" + dateTimeMillisTo + " dateTimeTo:" + DateTimeUtils.ConvertIsoDatetimeTimeZone(long.Parse(dateTimeMillisTo)), LogLevel.I);
            List<TouchData> retList = new List<TouchData>();
            List<TouchDataRecord> cr = null;
            String[] columns = { "*", "max(" + COLUMN_RSSI + ") AS " + COLUMN_RSSI };
            StringBuilder selection = new StringBuilder();
            List<string> selectionArgList = new List<string>();

            if (dateTimeMillisFrom != null)
            {
                selection.Append(COLUMN_RECEIVED_BEACON_DATE).Append(" >= ?");
                selectionArgList.Add(dateTimeMillisFrom);
            }

            if (dateTimeMillisTo != null)
            {
                selection.Append(" AND ");
                selection.Append(COLUMN_RECEIVED_BEACON_DATE).Append(" <= ?");
                selectionArgList.Add(dateTimeMillisTo);
            }

            try
            {
                if (selectionArgList.Count == 0)
                {
                    cr = db.Query<TouchDataRecord>("SELECT " + string.Join(",", columns) + " FROM [" + TABLE_NAME + "] GROUP BY [" + COLUMN_KEY_NAME + "]");
                }
                else
                {
                    cr = db.Query<TouchDataRecord>("SELECT " + string.Join(",", columns) + " FROM [" + TABLE_NAME + "] WHERE " + selection.ToString() + " GROUP BY [" + COLUMN_KEY_NAME + "]", selectionArgList.ToArray());
                }
                retList = cr.ConvertAll<TouchData>(org => { var ret = createSendData(org); ret.receiver = receiver; return ret; });
                DebugMessageUtils.GetInstance().WriteLog(TAG, "getMaxRssiData end retList.Count:" + retList.Count, LogLevel.I);
            }
            catch (Exception e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "getMaxRssiData failed", e, LogLevel.E);
                db.Close();
                throw e;
            }

            return retList;
        }

        /**
         * 受信ビーコン情報取得
         * UUIDごとのRSSI中央値データを取得
         *
         * @param db
         * @param dateTimeMillisFrom 取得開始時間
         * @param dateTimeMillisTo 取得終了時間
         * @param receiver レシーバ情報
         * @return
         */
        public static List<TouchData> getCenterRssiData(SQLiteConnection db, String dateTimeMillisFrom, String dateTimeMillisTo, Receiver receiver)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "getCenterRssiData start dateTimeMillisFrom:" + dateTimeMillisFrom + " dateTimeFrom:" + DateTimeUtils.ConvertIsoDatetimeTimeZone(long.Parse(dateTimeMillisFrom)) + " dateTimeMillisTo:" + dateTimeMillisTo + " dateTimeTo:" + DateTimeUtils.ConvertIsoDatetimeTimeZone(long.Parse(dateTimeMillisTo)), LogLevel.I);
            List <TouchData> retList = new List<TouchData>();
            List<TouchDataRecord> cr = null;
            StringBuilder selection = new StringBuilder();
            List<string> selectionArgList = new List<string>();

            selection.Append("SELECT ").Append(COLUMN_ID).Append(", ").Append(COLUMN_KEY_NAME).Append(", ").Append(COLUMN_UUID).Append(", ");
            selection.Append(COLUMN_MAJOR_ID).Append(", ").Append(COLUMN_MINOR_ID).Append(", ").Append(COLUMN_NICKNAME).Append(", ").Append(COLUMN_MEASURED_POWER).Append(", ");
            selection.Append(COLUMN_RECEIVED_BEACON_DATE).Append(", ").Append(COLUMN_LAT).Append(", ").Append(COLUMN_LON).Append(", ");
            selection.Append(COLUMN_POSITIONING_LOCATION_DATE).Append(", ");
            selection.Append("MAX(").Append(COLUMN_RECEIVED_BEACON_DATE).Append("), AVG(").Append(COLUMN_RSSI).Append(")AS ").Append(COLUMN_RSSI);
            selection.Append(" FROM");
            selection.Append("(SELECT ").Append(TABLE_NAME).Append(".*, ser.uuid_ser, ((cast(con.uuid_con as REAL) + 1) /2) AS center_no");
            selection.Append(" FROM ").Append(TABLE_NAME);
            // UUIDごとの連番
            selection.Append(" LEFT JOIN");
            selection.Append("(SELECT t1.").Append(COLUMN_ID).Append(", t1.").Append(COLUMN_KEY_NAME).Append(", t1.").Append(COLUMN_RSSI).Append(", COUNT(t1.").Append(COLUMN_KEY_NAME).Append(")AS uuid_ser");
            selection.Append(" FROM ").Append(TABLE_NAME).Append(" t1, ").Append(TABLE_NAME).Append(" t2");
            selection.Append(" WHERE t1.").Append(COLUMN_KEY_NAME).Append(" = t2.").Append(COLUMN_KEY_NAME);
            selection.Append(" AND (t1.").Append(COLUMN_RSSI).Append(" > t2.").Append(COLUMN_RSSI);
            selection.Append(" OR (t1.").Append(COLUMN_RSSI).Append(" = t2.").Append(COLUMN_RSSI);
            selection.Append(" AND t1.rowid >= t2.rowid))");
            selection.Append(" GROUP BY t1.").Append(COLUMN_ID).Append(", t1.").Append(COLUMN_KEY_NAME).Append(", t1.").Append(COLUMN_RSSI).Append(", t1.rowid");
            selection.Append(") ser");
            selection.Append(" ON ").Append(TABLE_NAME).Append(".").Append(COLUMN_ID).Append(" = ser.").Append(COLUMN_ID);
            // UUIDごとの件数
            selection.Append(" LEFT JOIN");
            selection.Append("(SELECT ").Append(COLUMN_KEY_NAME).Append(", COUNT(").Append(COLUMN_KEY_NAME).Append(")AS uuid_con");
            selection.Append(" FROM ").Append(TABLE_NAME);
            selection.Append(" GROUP BY ").Append(COLUMN_KEY_NAME);
            selection.Append(") con");
            selection.Append(" ON ").Append(TABLE_NAME).Append(".").Append(COLUMN_KEY_NAME).Append(" = con.").Append(COLUMN_KEY_NAME);
            // 条件
            selection.Append(" WHERE ");
            if (dateTimeMillisFrom != null)
            {
                selection.Append(TABLE_NAME).Append(".").Append(COLUMN_RECEIVED_BEACON_DATE).Append(" >= ?");
                selection.Append(" AND ");
                selectionArgList.Add(dateTimeMillisFrom);
            }
            if (dateTimeMillisTo != null)
            {
                selection.Append(TABLE_NAME).Append(".").Append(COLUMN_RECEIVED_BEACON_DATE).Append(" <= ?");
                selection.Append(" AND ");
                selectionArgList.Add(dateTimeMillisTo);
            }
            selection.Append(" ser.uuid_ser >= cast(center_no as INT)");
            selection.Append(" AND ser.uuid_ser <= round(center_no, 0)");
            selection.Append(" ORDER BY ").Append(TABLE_NAME).Append(".").Append(COLUMN_KEY_NAME).Append(", ").Append(TABLE_NAME).Append(".").Append(COLUMN_RSSI);
            selection.Append(")");
            selection.Append(" GROUP BY ").Append(COLUMN_KEY_NAME);

            try
            {
                cr = db.Query<TouchDataRecord>(selection.ToString(), selectionArgList.ToArray());
                retList = cr.ConvertAll(org => { var ret = createSendData(org); ret.receiver = receiver; return ret; });
                DebugMessageUtils.GetInstance().WriteLog(TAG, "getCenterRssiData end retList.Count:" + retList.Count, LogLevel.I);
            }
            catch (Exception e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "getCenterRssiData failed", e, LogLevel.E);
                db.Close();
                throw e;
            }

            return retList;
        }


        /**
         * 受信ビーコン情報取得
         * UUIDごとにRSSI生値データを取得
         *
         * @param db
         * @param dateTimeMillisFrom 取得開始時間
         * @param dateTimeMillisTo 取得終了時間
         * @param receiver レシーバ情報
         * @return
         */
        public static List<TouchData> getRawRssiData(SQLiteConnection db, String dateTimeMillisFrom, String dateTimeMillisTo, Receiver receiver)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "getRawRssiData start dateTimeMillisFrom:" + dateTimeMillisFrom + " dateTimeFrom:" + DateTimeUtils.ConvertIsoDatetimeTimeZone(long.Parse(dateTimeMillisFrom)) + " dateTimeMillisTo:" + dateTimeMillisTo + " dateTimeTo:" + DateTimeUtils.ConvertIsoDatetimeTimeZone(long.Parse(dateTimeMillisTo)), LogLevel.I);
            List<TouchData> retList = new List<TouchData>();
            List<TouchDataRecord> cr = null;
            String[] columns = { "*" };
            StringBuilder selection = new StringBuilder();
            List<string> selectionArgList = new List<string>();

            if (dateTimeMillisFrom != null)
            {
                selection.Append(COLUMN_RECEIVED_BEACON_DATE).Append(" >= ?");
                selectionArgList.Add(dateTimeMillisFrom);
            }

            if (dateTimeMillisTo != null)
            {
                selection.Append(" AND ");
                selection.Append(COLUMN_RECEIVED_BEACON_DATE).Append(" <= ?");
                selectionArgList.Add(dateTimeMillisTo);
            }

            try
            {
                if (selectionArgList.Count == 0)
                {
                    cr = db.Query<TouchDataRecord>("SELECT " + string.Join(",", columns) + " FROM [" + TABLE_NAME + "]");
                }
                else
                {
                    cr = db.Query<TouchDataRecord>("SELECT " + string.Join(",", columns) + " FROM [" + TABLE_NAME + "] WHERE " + selection.ToString(), selectionArgList.ToArray());
                }
                retList = cr.ConvertAll<TouchData>(org => { var ret = createSendData(org); ret.receiver = receiver; return ret; });
                DebugMessageUtils.GetInstance().WriteLog(TAG, "getRawRssiData end retList.Count:" + retList.Count, LogLevel.I);
            }
            catch (Exception e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "getRawRssiData failed", e, LogLevel.E);
                db.Close();
                throw e;
            }

            return retList;
        }

        /**
         * 受信ビーコン情報登録
         *
         * @param db
         * @param data
         */
        public static void addData(SQLiteConnection db, BeaconData data)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "addData(beacon) start occurred_date:" + data.occurred_date + (data.location != null ? " data.location.fixed_at:" + data.location.fixed_at : "") + " rssi:" + data.rssi, LogLevel.D);
            TouchDataRecord cv = new TouchDataRecord();
            cv.key_name = data.beacon.uuid + "-" + data.beacon.major + "-" + data.beacon.minor;
            cv.rssi = (int)data.rssi;
            cv.uuid = data.beacon.uuid;
            cv.major = (int)data.beacon.major;
            cv.minor = (int)data.beacon.minor;
            cv.nickname = data.beacon.nickname;
            cv.measured_power = (int)data.beacon.measured_power;
            cv.recv_beacon_date = DateTimeUtils.ConvertIsoDatetimeTimeZone(data.occurred_date);
            cv.latitude = data.location.latitude;
            cv.lon = data.location.longitude;
            cv.recv_location_date = data.location.fixed_at == null ? null : (long?)DateTimeUtils.ConvertIsoDatetimeTimeZone(data.location.fixed_at);

            try
            {
                var count = db.Insert(cv);
                DebugMessageUtils.GetInstance().WriteLog(TAG, "addData(beacon) end key_name:" + cv.key_name + " rssi:" + cv.rssi + " measured_power:" + cv.measured_power + " count:" + count + " cv.recv_beacon_date:" + cv.recv_beacon_date + " cv.recv_beacon_date_str:" + DateTimeUtils.ConvertIsoDatetimeTimeZone(cv.recv_beacon_date) + " cv.latitude:" + cv.latitude + " cv.lon:" + cv.lon + " cv.recv_location_date:" + cv.recv_location_date + " cv.recv_location_date_str:" + (cv.recv_location_date == null ? "" : DateTimeUtils.ConvertIsoDatetimeTimeZone((long)cv.recv_location_date)), LogLevel.I);
            }
            catch (Exception e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "addData(beacon) failed", e, LogLevel.E);
                db.Close();
                throw e;
            }
        }

        /**
         * ビーコン情報登録(インメモリ->永続化)
         *
         * @param db
         * @param data
         */
        public static void addData(SQLiteConnection db, TouchData data)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "addData(touch) start occurred_at:" + data.occurred_at + (data.location != null ? " data.location.fixed_at:" + data.location.fixed_at : "") + " rssi:" + data.rssi, LogLevel.D);
            TouchDataRecord cv = new TouchDataRecord(); 
            cv.key_name = data.beacon.uuid + "-" + data.beacon.major + "-" + data.beacon.minor;
            cv.rssi = (int)data.rssi;
            cv.uuid = data.beacon.uuid;
            cv.major = (int)data.beacon.major;
            cv.minor = (int)data.beacon.minor;
            cv.measured_power = (int)data.beacon.measured_power;
            cv.recv_beacon_date = DateTimeUtils.ConvertIsoDatetimeTimeZone(data.occurred_at);
            cv.latitude = data.location.latitude;
            cv.lon = data.location.longitude;
            cv.recv_location_date = data.location.fixed_at == null ? null : (long?)DateTimeUtils.ConvertIsoDatetimeTimeZone(data.location.fixed_at);

            try
            {
                var count = db.Insert(cv);
                DebugMessageUtils.GetInstance().WriteLog(TAG, "addData(touch) end key_name:" + cv.key_name + " rssi:" + cv.rssi + " measured_power:" + cv.measured_power + " count:" + count + " cv.recv_beacon_date:" + cv.recv_beacon_date + " cv.recv_beacon_date_str:" + DateTimeUtils.ConvertIsoDatetimeTimeZone(cv.recv_beacon_date) + " cv.latitude:" + cv.latitude + " cv.lon:" + cv.lon + " cv.recv_location_date:" + cv.recv_location_date + " cv.recv_location_date_str:" + (cv.recv_location_date == null ? "" : DateTimeUtils.ConvertIsoDatetimeTimeZone((long)cv.recv_location_date)), LogLevel.I);
            }
            catch (Exception e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "addData(touch) failed", e, LogLevel.E);
                db.Close();
                throw e;
            }
        }

        /**
         * sendDateTimeMillis以前のデータを削除
         *
         * @param db
         * @param sendDateTimeMillis 送信日時ミリ秒文字列
         */
        public static void deleteBefore(SQLiteConnection db, String sendDateTimeMillis)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "deleteBefore start sendDateTimeMillis:" + sendDateTimeMillis, LogLevel.D);
            try
            {
                var count = db.Execute("DELETE FROM [" + TABLE_NAME + "] WHERE [" + COLUMN_RECEIVED_BEACON_DATE + "] <=?", new String[] { sendDateTimeMillis });
                DebugMessageUtils.GetInstance().WriteLog(TAG, "deleteBefore end count:" + count, LogLevel.I);
            }
            catch (Exception e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "deleteBefore failed", e, LogLevel.E);
                db.Close();
                throw e;
            }
        }

        /**
         *
         * @param db
         */
        public static void deleteAll(SQLiteConnection db)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "deleteAll start", LogLevel.D);
            try
            {
                var count = db.DeleteAll<TouchDataRecord>();
                DebugMessageUtils.GetInstance().WriteLog(TAG, "deleteAll end count:" + count, LogLevel.I);
            }
            catch (Exception e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "deleteAll failed", e, LogLevel.E);
                db.Close();
                throw e;
            }
        }

        /**
         *
         * @param db
         */
        public static void dropTable(SQLiteConnection db)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "dropTable start", LogLevel.D);
            try
            {
                var count = db.DropTable<TouchDataRecord>();
                DebugMessageUtils.GetInstance().WriteLog(TAG, "dropTable end count:" + count, LogLevel.D);
            }
            catch (Exception e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "dropTable failed", e, LogLevel.E);
                db.Close();
                throw e;
            }
        }
        /**
         * ビーコン検出時の送信データ生成
         *
         * @param cr
         * @return
         */
        private static TouchData createSendData(TouchDataRecord cr)
        {
            TouchData retData = new TouchData();

            // ビーコン情報
            retData.beacon.uuid = cr.uuid;
            retData.beacon.major = cr.major;
            retData.beacon.minor = cr.minor;
            retData.beacon.measured_power = cr.measured_power;

            // イベント発生位置情報
            long fixedAt = cr.recv_location_date == null ? 0 : (long)cr.recv_location_date;
            if (0 < fixedAt)
            {
                retData.location.latitude = cr.latitude;
                retData.location.longitude = cr.lon;
                retData.location.fixed_at = DateTimeUtils.ConvertIsoDatetimeTimeZone(fixedAt);
            }

            // 通知情報
            retData.rssi = cr.rssi;
            retData.distance = calcDistance((int)retData.rssi, (int)retData.beacon.measured_power);
            retData.occurred_at = DateTimeUtils.ConvertIsoDatetimeTimeZone(cr.recv_beacon_date);

            return retData;
        }

        /**
         * 上記のメソッドと同様の処理を行う.
         * ただし、日時の再整形は実施しない.(常に'1970-01-01T09:00:00'になるため) -> C#だとうまくいかない。C#版では、日付については、プログラム側では常にISO 8601形式文字列で、DB側では常にエポックミリ秒形式で保持するようにします。
         *
         * @param cr
         * @return
         */
        private static TouchData getSendData(TouchDataRecord cr)
        {
            TouchData retData = new TouchData();

            retData.beacon.uuid = cr.uuid;
            retData.beacon.major = cr.major;
            retData.beacon.minor = cr.minor;
            retData.beacon.measured_power = cr.measured_power;

            long fixedAt = cr.recv_location_date == null ? 0 : (long)cr.recv_location_date;
            if (0 < fixedAt)
            {
                retData.location.latitude = cr.latitude;
                retData.location.longitude = cr.lon;
                //retData.location.fixed_at = fixedAt.ToString();
                retData.location.fixed_at = DateTimeUtils.ConvertIsoDatetimeTimeZone(fixedAt);
            }

            retData.rssi = cr.rssi;
            retData.distance = calcDistance((int)retData.rssi, (int)retData.beacon.measured_power);
            //retData.occurred_at = cr.recv_beacon_date.ToString();
            retData.occurred_at = DateTimeUtils.ConvertIsoDatetimeTimeZone(cr.recv_beacon_date);
            DebugMessageUtils.GetInstance().WriteLog(TAG, "getSendData end retData.occurred_at:" + retData.occurred_at, LogLevel.V);
            return retData;
        }

        /**
         * ビーコンまでの距離(メートル)算出
         * 小数点第2位以下切り捨て
         *
         * @param rssi
         * @param measuredPower
         * @return ビーコンまでの距離(メートル) ※距離が0以下の場合は0を返す
         */
        private static double calcDistance(int rssi, int measuredPower)
        {
            double attenuate = 2.5; // 減衰率
            int precision = 2;      // 演算に使用する小数点以下の桁数

            //距離算出
            double distance = Math.Pow(10, (rssi - measuredPower) / (-10 * attenuate));
            double ret = 0;
            if (0 < distance)
            {
                ret = ToRoundDown(distance, precision);
            }

            return ret;
        }
        /// ------------------------------------------------------------------------
        /// <summary>
        ///     指定した精度の数値に切り捨てします。</summary>
        /// <param name="dValue">
        ///     丸め対象の倍精度浮動小数点数。</param>
        /// <param name="iDigits">
        ///     戻り値の有効桁数の精度。</param>
        /// <returns>
        ///     iDigits に等しい精度の数値に切り捨てられた数値。</returns>
        /// ------------------------------------------------------------------------
        public static double ToRoundDown(double dValue, int iDigits)
        {
            double dCoef = System.Math.Pow(10, iDigits);

            return dValue > 0 ? System.Math.Floor(dValue * dCoef) / dCoef :
                                System.Math.Ceiling(dValue * dCoef) / dCoef;
        }
    }
}
