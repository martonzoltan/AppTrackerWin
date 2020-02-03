using AppTrackerWin.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace AppTrackerWin.Helper
{
    public class StorageHelper
    {
        private const string CreateTableQuery = @"CREATE TABLE IF NOT EXISTS [AppTimeLogs] (
                                               [ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                                               [User] NVARCHAR(2048)  NULL,
                                               [Application] VARCHAR(2048)  NULL,
                                               [TimeSpent] INT(32) NULL,
                                               [InsertedDate] Date
                                               )";
        private const string DatabaseFile = "timeSpentStorage.db";
        private const string DatabaseSource = "data source=" + DatabaseFile;
         // Create the file which will be hosting our database
        public void CreateDatabaseFileIfNotExists()
        {
            if (!File.Exists(DatabaseFile))
            {
                SQLiteConnection.CreateFile(DatabaseFile);
            }
        }

        public void AddEntryToDatabase(TrackedWindow trackedWindow)
        {
            using (var connection = new SQLiteConnection(DatabaseSource))
            {
                // Create a database command
                using (var command = new SQLiteCommand(connection))
                {
                    connection.Open();

                    // Create the table
                    command.CommandText = CreateTableQuery;
                    command.ExecuteNonQuery();

                    // Insert entries in database table
                    command.CommandText = "INSERT INTO AppTimeLogs (User,Application,TimeSpent,InsertedDate) "+
                        " VALUES ('Detty', @ExcelProcess, @TimeElapsed, date('now'))";
                    command.Parameters.Add(new SQLiteParameter("ExcelProcess", trackedWindow.Name));
                    command.Parameters.Add(new SQLiteParameter("TimeElapsed", trackedWindow.TimeSpent));

                    command.ExecuteNonQuery();

                    connection.Close(); // Close the connection to the database
                }
            }
        }

        public List<TrackedWindowStorage> GetAllDatabaseEntries()
        {
            List<TrackedWindowStorage> twStorage = new List<TrackedWindowStorage>();
            using (var connection = new SQLiteConnection(DatabaseSource))
            {
                // Create a database command
                using (var command = new SQLiteCommand(connection))
                {
                    connection.Open();
                    command.CommandText = "Select InsertedDate, User, Application, sum(TimeSpent) AS 'TimeSpent' "+
                        "FROM AppTimeLogs "+
                        "Group By InsertedDate, User, Application ORDER BY InsertedDate ASC, sum(TimeSpent) DESC";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            twStorage.Add(new TrackedWindowStorage
                            {
                                Name = reader["Application"].ToString(),
                                UserName = reader["User"].ToString(),
                                Date = (DateTime)reader["InsertedDate"],
                                TimeSpent = Convert.ToInt32(reader["TimeSpent"])
                            });
                            //Console.WriteLine(reader["User"] + " : " + reader["Application"] + " - " + reader["TimeSpent"]);
                        }
                    }
                    connection.Close(); // Close the connection to the database
                }
            }
            return twStorage;
        }
        
    }
}
