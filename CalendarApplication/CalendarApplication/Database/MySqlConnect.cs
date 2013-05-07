using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

using CalendarApplication.Models.Calendar;
using CalendarApplication.Models.Event;
using CalendarApplication.Models.Account;
using CalendarApplication.Models.User;
using CalendarApplication.Models.Group;
using CalendarApplication.Models.EventType;

namespace CalendarApplication.Database
{
    public class MySqlConnect
    {
        protected MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        public string ErrorMessage { set; get; }

        /// <summary>
        /// Creates a new MySqlConnection. Open with method OpenConnection().
        /// </summary>
        public MySqlConnect()
        {
            server = "localhost";
            database = "pksudb";
            uid = "root";
            password = "pksu2013";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
        }

        /// <summary>
        /// Opens the connection to the database
        /// </summary>
        /// <returns>true if succesful, otherwise false</returns>
        public bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        this.ErrorMessage = "Cannot connect to database!";
                        break;

                    case 1045:
                       this.ErrorMessage = "User/password did not match!";
                        break;
                }
                return false;
            }
        }

        /// <summary>
        /// Closes the connection to the database
        /// </summary>
        /// <returns>true if succesful, otherwise false</returns>
        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                this.ErrorMessage = "Could not close connection! "+ex.ErrorCode;
                return false;
            }
        }

        /// <summary>
        /// Executes a query and returns the result in a datatable
        /// </summary>
        /// <param name="query">is the query to be executed</param>
        /// <returns>A datatable with the result, null on error</returns>
        public DataTable ExecuteQuery(CustomQuery query)
        {
            //Open connection
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = null;

                try
                {
                    //Create the table
                    DataTable dt = new DataTable();

                    //Create Command and run it

                    cmd = new MySqlCommand(query.Cmd, connection);
                    if (query.ArgNames != null && query.Args != null)
                    {
                        for (int i = 0; i < query.ArgNames.Length; i++)
                            {
                                cmd.Parameters.AddWithValue(query.ArgNames[i], query.Args[i]);
                            }
                        cmd.Prepare();
                    }
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                
                    //Set the table columns
                    int cols = dataReader.FieldCount;
                    for (int i = 0; i < cols; i++)
                    {
                        dt.Columns.Add(dataReader.GetName(i), dataReader.GetFieldType(i));
                    }

                    //Fill the table
                    while (dataReader.Read())
                    {
                        DataRow dr = dt.NewRow();
                        for (int i = 0; i < cols; i++)
                        {
                            dr[i] = dataReader[i];
                        }
                        dt.Rows.Add(dr);
                    }

                    //Close the connection
                    dataReader.Close();
                    this.CloseConnection();

                    return dt;
                }
                catch (MySqlException ex0)
                {
                    this.ErrorMessage = "Database error -> Error message: " + ex0.Message + ", Caused by: " + cmd.CommandText;
                    this.CloseConnection();
                    return null;
                }
            }
            else
            {
                //This is an error!
                return null;
            }

        }

        /// <summary>
        /// Executes a query and returns the result in a datatable
        /// </summary>
        /// <param name="query">is the query to be executed</param>
        /// <returns>A datatable with the result, null on error</returns>
        
        public DataTable ExecuteQuery(string query)
        {
            //Open connection
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = null;

                try
                {
                    //Create the table
                    DataTable dt = new DataTable();

                    //Create Command and run it
                    cmd = new MySqlCommand(query, connection);
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Set the table columns
                    int cols = dataReader.FieldCount;
                    for (int i = 0; i < cols; i++)
                    {
                        dt.Columns.Add(dataReader.GetName(i), dataReader.GetFieldType(i));
                    }

                    //Fill the table
                    while (dataReader.Read())
                    {
                        DataRow dr = dt.NewRow();
                        for (int i = 0; i < cols; i++)
                        {
                            dr[i] = dataReader[i];
                        }
                        dt.Rows.Add(dr);
                    }

                    //Close the connection
                    dataReader.Close();
                    this.CloseConnection();

                    return dt;
                }
                catch (MySqlException ex0)
                {
                    this.ErrorMessage = "Database error -> Error message: " + ex0.Message + ", Caused by: " + cmd.CommandText;
                    this.CloseConnection();
                    return null;
                }
            }
            else
            {
                //This is an error!
                return null;
            }

        }

        /// <summary>
        /// Executes several queries and returns the result in a dataset with several tables.
        /// </summary>
        /// <param name="queries">is the queries to be executed</param>
        /// <returns>A dataset with the resulting datatables, or null on error</returns>
         public DataSet ExecuteQuery(CustomQuery[] queries)
        {
            //Open connection
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = null;

                try
                {
                    //Initialise naming counter and create set
                    int counter = 0;
                    DataSet ds = new DataSet();

                    foreach (CustomQuery query in queries)
                    {
                        //Create the table
                        DataTable dt = new DataTable("table_" + counter);

                        //Create Command and run it

                        cmd = new MySqlCommand(query.Cmd, connection);
                        if (query.ArgNames != null && query.Args != null)
                        {
                            for (int i = 0; i < query.ArgNames.Length; i++)
                            {
                                cmd.Parameters.AddWithValue(query.ArgNames[i], query.Args[i]);
                            }
                        }
                        cmd.Prepare();
                        MySqlDataReader dataReader = cmd.ExecuteReader();

                        //Set the table columns
                        int cols = dataReader.FieldCount;
                        for (int i = 0; i < cols; i++)
                        {
                            dt.Columns.Add(dataReader.GetName(i), dataReader.GetFieldType(i));
                        }

                        //Fill the table
                        while (dataReader.Read())
                        {
                            DataRow dr = dt.NewRow();
                            for (int i = 0; i < cols; i++)
                            {
                                dr[i] = dataReader[i];
                            }
                            dt.Rows.Add(dr);
                        }

                        //Add table to set
                        ds.Tables.Add(dt);
                        counter++;
                        dataReader.Close();
                    }

                    //Close the connection
                    this.CloseConnection();

                    return ds;

                }
                catch (MySqlException ex0)
                {
                    this.ErrorMessage = "Database error -> Error message: " + ex0.Message + ", Caused by: " + cmd.CommandText;
                    this.CloseConnection();
                    return null;
                }
            }
            else
            {
                //This is an error!
                return null;
            }

        }

         /// <summary>
         /// Executes several queries and returns the result in a dataset with several tables.
         /// </summary>
         /// <param name="queries">is the queries to be executed</param>
         /// <returns>A dataset with the resulting datatables, or null on error</returns>
        public DataSet ExecuteQuery(string[] queries)
        {
            //Open connection
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = null;

                try
                {
                    //Initialise naming counter and create set
                    int counter = 0;
                    DataSet ds = new DataSet();

                    foreach (string query in queries)
                    {
                        //Create the table
                        DataTable dt = new DataTable("table_"+counter);

                        //Create Command and run it
                        cmd = new MySqlCommand(query, connection);
                        MySqlDataReader dataReader = cmd.ExecuteReader();

                        //Set the table columns
                        int cols = dataReader.FieldCount;
                        for (int i = 0; i < cols; i++)
                        {
                            dt.Columns.Add(dataReader.GetName(i), dataReader.GetFieldType(i));
                        }

                        //Fill the table
                        while (dataReader.Read())
                        {
                            DataRow dr = dt.NewRow();
                            for (int i = 0; i < cols; i++)
                            {
                                dr[i] = dataReader[i];
                            }
                            dt.Rows.Add(dr);
                        }

                        //Add the table to the set
                        ds.Tables.Add(dt);
                        counter++;
                        dataReader.Close();
                    }

                    //Close the connection
                    this.CloseConnection();

                    return ds;

                }
                catch (MySqlException ex0)
                {
                    this.ErrorMessage = "Database error -> Error message: " + ex0.Message + ", Caused by: " + cmd.CommandText;
                    this.CloseConnection();
                    return null;
                }
            }
            else
            {
                //This is an error!
                return null;
            }

        }

        public List<Room> GetRooms()
        {
            //Open connection
            if (this.OpenConnection() == true)
            {
                //Create the list
                List<Room> list = new List<Room>();

                string query = "SELECT * FROM rooms ORDER BY roomId";

                //Create Command and run it
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    Room r = new Room
                    {
                        ID = (int)dataReader["roomId"],
                        Name = (string)dataReader["roomName"]
                    };
                    list.Add(r);
                }

                return list;
            }
            else
            {
                //This is an error!
                return null;
            }

        }

        public List<GroupModel> GetGroups(string Table, string Where)
        {
            if (this.OpenConnection())
            {
                List<GroupModel> result = new List<GroupModel>();

                MySqlCommand msc = new MySqlCommand("SELECT * FROM (" + Table + ")"
                                                + (string.IsNullOrEmpty(Where) ? "" : " WHERE (" + Where + ")"),
                                                connection);
                MySqlDataReader dataReader = msc.ExecuteReader();

                while (dataReader.Read())
                {
                    GroupModel gm = new GroupModel
                    {
                        ID = (int)dataReader["groupId"],
                        Name = (string)dataReader["groupName"]
                    };
                    result.Add(gm);
                }

                this.CloseConnection();
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}