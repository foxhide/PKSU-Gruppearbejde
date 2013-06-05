using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;

using CalendarApplication.Models.Account;

namespace CalendarApplication.Database
{
    public class MySqlUser : MySqlConnect
    {
        public int CreateUser(Register data)
        {
            string insert = "INSERT INTO pksudb.users (userName,password,realName,phoneNum,email,active,needsApproval) " +
                            "VALUES (@username, @password, @realname, @phone, @email, 1, 1);"
                            + " SELECT last_insert_id();";

            if (this.OpenConnection() == true)
            {
                MySqlTransaction mst = null;
                MySqlCommand cmd = null;
                int result = -1;

                try
                {
                    mst = this.connection.BeginTransaction();
                    cmd = new MySqlCommand();
                    cmd.Connection = this.connection;
                    cmd.Transaction = mst;

                    cmd.CommandText = insert;
                    string hashedPassword = PasswordHashing.CreateHash(data.Password);
                    cmd.Parameters.AddWithValue("@username", data.UserName);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);
                    cmd.Parameters.AddWithValue("@realname", data.RealName);
                    cmd.Parameters.AddWithValue("@phone", data.Phone);
                    cmd.Parameters.AddWithValue("@email", data.Email);
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

        public bool EditUser(int id, object newValue, string change)
        {
            if (this.OpenConnection() == true)
            {
                MySqlTransaction mst = null;
                MySqlCommand cmd = null;

                try
                {
                    mst = this.connection.BeginTransaction();
                    cmd = new MySqlCommand();
                    cmd.Connection = this.connection;
                    cmd.Transaction = mst;

                    string update = "UPDATE pksudb.users SET " + change + " = @value WHERE userId = @uid";
                    cmd.Parameters.AddWithValue("@value", newValue);
                    cmd.Parameters.AddWithValue("@uid", id);

                    cmd.CommandText = update;
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

        public bool deleteUser(int userId)
        {
            if (this.OpenConnection() == true)
            {
                MySqlTransaction mst = null;
                MySqlCommand cmd = null;
                
                try
                {
                    mst = this.connection.BeginTransaction();
                    cmd = new MySqlCommand();
                    cmd.Connection = this.connection;
                    cmd.Transaction = mst;

                    string delete = "DELETE FROM pksudb.users WHERE userId = @uid";
                    cmd.Parameters.AddWithValue("@uid", userId);

                    cmd.CommandText = delete;
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