using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;

namespace Chess.Tools.SQLite
{
    /// <summary>
    /// Provide base functionality to run SQL via a SQLite connection.
    /// </summary>
    public abstract class SqliteDataContextBase
    {
        #region Constructor

        public SqliteDataContextBase(string dbFilePath)
        {
            _dbFilePath = dbFilePath;
        }

        #endregion Constructor

        #region Members

        private string _dbFilePath;

        #endregion Members

        #region Methods

        protected SqliteConnection createConnection()
        {
            return new SqliteConnection($"DataSource={ _dbFilePath };");
        }

        /// <summary>
        /// Run a SQL query and retrieve the data as a result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be queried.</param>
        /// <returns>A set of records returned by the SQLite data source.</returns>
        protected List<Dictionary<string, object>> queryItems(string sql)
        {
            // init the result set with an empty list
            var data = new List<Dictionary<string, object>>();

            // create a connection to the given SQLite data source
            using (var connection = createConnection())
            {
                // create the SQL command to be executed
                using (var command = new SqliteCommand(sql, connection))
                {
                    // open the SQLite connection
                    connection.Open();

                    // execute the SQL command
                    using (var reader = command.ExecuteReader())
                    {
                        // parse the data retrieved from the SQLite data source
                        data = readItems(reader);
                    }

                    // close the connection
                    connection.Close();
                }
            }

            return data;
        }

        /// <summary>
        /// Run a SQL query and retrieve a single data record.
        /// </summary>
        /// <param name="sql">The SQL statement to be queried.</param>
        /// <returns>A single data record returned by the SQLite data source.</returns>
        protected Dictionary<string, object> queryItem(string sql)
        {
            // init the result record with null
            Dictionary<string, object> data = null;

            // create a connection to the given SQLite data source
            using (var connection = createConnection())
            {
                // create the SQL command to be executed
                using (var command = new SqliteCommand(sql, connection))
                {
                    // open the SQLite connection
                    connection.Open();

                    // execute the SQL command
                    using (var reader = command.ExecuteReader())
                    {
                        // read the first record
                        if (reader.Read())
                        {
                            // parse the data retrieved from the SQLite data source
                            data = readItem(reader);
                        }
                    }

                    // close the SQLite connection
                    connection.Close();
                }
            }

            return data;
        }

        /// <summary>
        /// Run a SQL query and retrieve a specific column from a single data record.
        /// </summary>
        /// <param name="sql">The SQL statement to be queried.</param>
        /// <returns>A specific column from a single data record returned by the SQLite data source.</returns>
        protected object queryItem(string sql, string columnName)
        {
            // init the result record with null
            object data = null;

            // create a connection to the given SQLite data source
            using (var connection = createConnection())
            {
                // create the SQL command to be executed
                using (var command = new SqliteCommand(sql, connection))
                {
                    // open the SQLite connection
                    connection.Open();

                    // execute the SQL command
                    using (var reader = command.ExecuteReader())
                    {
                        // read the first record
                        if (reader.Read())
                        {
                            // get only the data from the given column
                            data = reader[columnName];
                        }
                    }

                    // close the SQLite connection
                    connection.Close();
                }
            }

            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scriptFilePath"></param>
        /// <returns></returns>
        protected int executeScript(string scriptFilePath)
        {
            // read script content
            string sql;
            using (var reader = new StreamReader(scriptFilePath)) { sql = reader.ReadToEnd(); }

            // open a new database connection and execute the script as command
            return executeSql(sql);
        }

        /// <summary>
        /// Run a writing SQL statement and retrieve the number of records affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <returns>The number of records affected (-1 signals an error).</returns>
        protected int executeSql(string sql)
        {
            // init the affected records value with -1
            int ret = -1;

            // create a connection to the given SQLite data source
            using (var connection = createConnection())
            {
                // open the SQLite connection
                connection.Open();

                // execute the SQL command and set affected records
                ret = executeSql(sql, connection);

                // close the SQLite connection
                connection.Close();
            }

            return ret;
        }

        /// <summary>
        /// Run a writing SQL statement and retrieve the number of records affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The database connection to be used.</param>
        /// <returns>The number of records affected (-1 signals an error).</returns>
        protected int executeSql(string sql, SqliteConnection connection)
        {
            // init the affected records value with -1
            int ret = -1;

            // create the SQL command to be executed
            using (var command = new SqliteCommand(sql, connection))
            {
                // open the connection if it is not already open
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }

                // execute the SQL command and set affected records
                ret = command.ExecuteNonQuery();
            }

            return ret;
        }

        #region Helpers

        /// <summary>
        /// Help parsing a SQLite result set with multiple records and columns.
        /// </summary>
        /// <param name="reader">The data reader containing the raw queried data.</param>
        /// <returns>A result set of (column name, value) tuples as a dictionary.</returns>
        protected List<Dictionary<string, object>> readItems(DbDataReader reader)
        {
            // init the result set as an empty list
            var items = new List<Dictionary<string, object>>();

            // go through all records
            while (reader.Read())
            {
                // parse the (column name, value) tuples and apply them to the result set
                var item = readItem(reader);
                items.Add(item);
            }

            return items;
        }

        /// <summary>
        /// Help parsing a single record with multiple columns from a SQLite result set.
        /// </summary>
        /// <param name="reader">The data reader containing the raw queried data.</param>
        /// <returns>A record with all (column name, value) tuples as a dictionary.</returns>
        protected Dictionary<string, object> readItem(DbDataReader reader)
        {
            // init the record as an empty dictionary
            var item = new Dictionary<string, object>();

            // loop through all columns
            for (int column = 0; column < reader.FieldCount; column++)
            {
                // parse the column name and value
                string columnName = reader.GetName(column);
                object value = reader.GetValue(column);

                // apply the data to the record dictionary
                item.Add(columnName, value);
            }

            return item;
        }

        #endregion Helpers

        #endregion Methods
    }
}
