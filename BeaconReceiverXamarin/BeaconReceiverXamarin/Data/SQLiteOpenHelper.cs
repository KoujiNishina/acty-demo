using BeaconReceiverConnectorXamarin.Interface;
using BeaconReceiverConnectorXamarin.Utils;
using SQLite;
using System;

namespace BeaconReceiverXamarin.Data
{
    public class SQLiteOpenHelper
    {
        const string TAG = "SQLiteOpenHelper";
        readonly SQLiteConnection database;

        public SQLiteOpenHelper(string dbPath, Type type)
        {
            database = new SQLiteConnection(dbPath);
            try
            {
                database.CreateTable(type, CreateFlags.None);
            }
            catch (Exception e)
            {
                DebugMessageUtils.GetInstance().WriteLog(TAG, "CreateTable failed", e, LogLevel.E);
                throw e;
            }
        }
        public SQLiteConnection getWritableDatabase()
        {
            return database;
        }
    }
}
