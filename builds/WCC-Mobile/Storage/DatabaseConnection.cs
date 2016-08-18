using SQLite;
using System.Collections.Generic;
using System.IO;
using WCCMobile.Models;

namespace WCCMobile.Storage
{
    

    public interface IDatabaseConnection
    {
        SQLiteConnection DbConnection();
    }

    public class DatabaseConnection : IDatabaseConnection
    {
        public SQLiteConnection DbConnection()
        {
            var dbName = "Schedule.db3";
            var path = Path.Combine(System.Environment.
              GetFolderPath(System.Environment.
              SpecialFolder.Personal), dbName);
            return new SQLiteConnection(path);
        }
    }
}