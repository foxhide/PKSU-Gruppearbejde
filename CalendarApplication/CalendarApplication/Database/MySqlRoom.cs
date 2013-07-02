using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace CalendarApplication.Database
{
    public class MySqlRoom : MySqlConnect
    {
        /// <summary>
        /// Create a new room
        /// </summary>
        /// <param name="name">Name of new room</param>
        /// <returns>id of new room, -1 in case of failure</returns>
        public int CreateRoom(string name)
        {
            if (this.OpenConnection() == true)
            {
                MySqlTransaction mst = null;
                MySqlCommand cmd = null;
                int result = -1;

                try
                {
                    mst = connection.BeginTransaction();
                    cmd = new MySqlCommand();
                    cmd.Connection = connection;
                    cmd.Transaction = mst;

                    string insert = "INSERT INTO rooms (roomName) VALUES (@name); "
                                + "SELECT last_insert_id();";

                    cmd.CommandText = insert;
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Prepare();
                    result = Convert.ToInt32(cmd.ExecuteScalar());

                    mst.Commit();

                    this.CloseConnection();
                }
                catch (MySqlException ex0)
                {
                    try
                    {
                        mst.Rollback();
                        this.CloseConnection();
                        ErrorMessage = "Some database error occured: Discarded changes, Error message: " + ex0.Message
                                        + ", Caused by: " + cmd.CommandText;
                        return -1;
                    }
                    catch (MySqlException ex1)
                    {
                        this.CloseConnection();
                        ErrorMessage = "Some database error occured: Could not discard changes, DB corrupt, Error message: " + ex1.Message
                                        + ", Caused by: " + cmd.CommandText;
                        return -1;
                    }
                }
                return result;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Rename an existing room
        /// </summary>
        /// <param name="id">room id</param>
        /// <param name="newName">New name of room</param>
        /// <returns>bool indicating success or failure</returns>
        public bool RenameRoom(int id, string newName)
        {
            if (this.OpenConnection() == true)
            {
                MySqlTransaction mst = null;
                MySqlCommand cmd = null;

                try
                {
                    mst = connection.BeginTransaction();
                    cmd = new MySqlCommand();
                    cmd.Connection = connection;
                    cmd.Transaction = mst;

                    string rename = "UPDATE rooms SET roomName = @name WHERE roomId = @id";

                    cmd.CommandText = rename;
                    cmd.Parameters.AddWithValue("@name", newName);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    mst.Commit();

                    this.CloseConnection();
                }
                catch (MySqlException ex0)
                {
                    try
                    {
                        mst.Rollback();
                        this.CloseConnection();
                        ErrorMessage = "Some database error occured: Discarded changes, Error message: " + ex0.Message
                                        + ", Caused by: " + cmd.CommandText;
                        return false;
                    }
                    catch (MySqlException ex1)
                    {
                        this.CloseConnection();
                        ErrorMessage = "Some database error occured: Could not discard changes, DB corrupt, Error message: " + ex1.Message
                                        + ", Caused by: " + cmd.CommandText;
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Delete an existing room
        /// </summary>
        /// <param name="id">id of room to be deleted</param>
        /// <returns>bool indicating success or failure</returns>
        public bool DeleteRoom(int id)
        {
            if (this.OpenConnection() == true)
            {
                MySqlTransaction mst = null;
                MySqlCommand cmd = null;

                try
                {
                    mst = connection.BeginTransaction();
                    cmd = new MySqlCommand();
                    cmd.Connection = connection;
                    cmd.Transaction = mst;

                    string delete = "DELETE FROM rooms WHERE roomId = @id";

                    cmd.CommandText = delete;
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    mst.Commit();

                    this.CloseConnection();
                }
                catch (MySqlException ex0)
                {
                    try
                    {
                        mst.Rollback();
                        this.CloseConnection();
                        ErrorMessage = "Some database error occured: Discarded changes, Error message: " + ex0.Message
                                        + ", Caused by: " + cmd.CommandText;
                        return false;
                    }
                    catch (MySqlException ex1)
                    {
                        this.CloseConnection();
                        ErrorMessage = "Some database error occured: Could not discard changes, DB corrupt, Error message: " + ex1.Message
                                        + ", Caused by: " + cmd.CommandText;
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}