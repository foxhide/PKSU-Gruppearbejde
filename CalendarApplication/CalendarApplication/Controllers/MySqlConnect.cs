using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

using CalendarApplication.Models.Calendar;
using CalendarApplication.Models.Event;
using CalendarApplication.Models.Account;
using CalendarApplication.Models.User;
using CalendarApplication.Models.EventType;

namespace CalendarApplication.Controllers
{
    public class MySqlConnect
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

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
                        MessageBox.Show("Cannot connect!");
                        break;

                    case 1045:
                        MessageBox.Show("User/password did not match!");
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
                MessageBox.Show("Could not close connection! "+ex.ErrorCode);
                return false;
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
                //Create the table
                DataTable dt = new DataTable();

                //Create Command and run it
                MySqlCommand cmd = new MySqlCommand(query, connection);
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
                this.CloseConnection();

                return dt;
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
                //Initialise naming counter and create set
                int counter = 0;
                DataSet ds = new DataSet();

                foreach (string query in queries)
                {
                    //Create the table
                    DataTable dt = new DataTable("table_"+counter);

                    //Create Command and run it
                    MySqlCommand cmd = new MySqlCommand(query, connection);
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
            else
            {
                //This is an error!
                return null;
            }

        }

        public List<BasicEvent> GetEvents(bool rooms, string whereStatement, string orderBy)
        {
            //Open connection
            if (this.OpenConnection() == true)
            {
                //Create the list
                List<BasicEvent> list = new List<BasicEvent>();

                string query = "SELECT * FROM (events NATURAL JOIN users NATURAL JOIN eventtypes" +
                                (rooms?" NATURAL JOIN eventroomsused NATURAL JOIN rooms":"")+") "+
                                (string.IsNullOrEmpty(whereStatement) ? "" : " WHERE "+whereStatement)+
                                " ORDER BY " + orderBy + (rooms?", roomId":"");

                //Create Command and run it
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //Create events and fill list
                bool go = dataReader.Read();
                while (go)
                {
                    BasicEvent ev = new BasicEvent
                    {
                        ID = (int)dataReader["eventId"],
                        Name = (string)dataReader["eventName"],
                        Creator = (string)dataReader["username"],
                        TypeName = (string)dataReader["eventTypeName"],
                        Start = (DateTime)dataReader["eventStart"],
                        End = (DateTime)dataReader["eventEnd"],
                        State = (int)dataReader["state"],
                        Rooms = rooms ? new List<Room>() : null
                    };
                    
                    if (rooms)
                    {
                        ev.Rooms.Add(new Room { ID = (int)dataReader["roomId"], Name = (string)dataReader["roomName"] });


                        go = dataReader.Read();
                        while (go && (int)dataReader["eventId"] == ev.ID)
                        {
                            ev.Rooms.Add(new Room { ID = (int)dataReader["roomId"], Name = (string)dataReader["roomName"] });
                            go = dataReader.Read();
                        }
                    }
                    else
                    {
                        go = dataReader.Read();
                    }
                    
                    list.Add(ev);
                }

                //Close the connection
                this.CloseConnection();

                return list;
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

        public UserModel GetUser(int ID)
        {
            if (this.OpenConnection())
            {

                MySqlCommand msc = new MySqlCommand("SELECT * FROM pksudb.users WHERE userId = "+ID,connection);
                MySqlDataReader dataReader = msc.ExecuteReader();

                if (!dataReader.Read())
                {
                    return null;
                }

                UserModel result = new UserModel
                {
                    ID = ID,
                    UserName = (string)dataReader["userName"],
                    RealName = (string)dataReader["realName"],
                    Email = (string)dataReader["email"]
                };

                this.CloseConnection();
                return result;
            }
            else
            {
                return null;
            }
        }

        public List<EventTypeModel> GetEventTypes(string where)
        {
            if (this.OpenConnection())
            {

                MySqlCommand msc = new MySqlCommand("SELECT * FROM pksudb.eventtypes" +
                                                    (string.IsNullOrEmpty(where) ? "" : " WHERE "+where),
                                                    connection);
                MySqlDataReader dataReader = msc.ExecuteReader();

                List<EventTypeModel> result = new List<EventTypeModel>();

                while (dataReader.Read())
                {
                    EventTypeModel et = new EventTypeModel
                    {
                        ID = (int)dataReader["eventTypeId"],
                        Name = (string)dataReader["eventTypeName"],
                        Selected = true
                    };
                    result.Add(et);
                }

                this.CloseConnection();
                return result;
            }
            else
            {
                return null;
            }
        }

        public int CreateUser(Register data)
        {
            string insert = "INSERT INTO pksudb.users (userName,password,realName,email,active,needsApproval) VALUES ('"
                            + data.UserName + "','" + data.Password + "','" + data.RealName + "','" + data.Email + "',1,1);"
                            + " SELECT last_insert_id();";

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

                    cmd.CommandText = insert;
                    result = Convert.ToInt32(cmd.ExecuteScalar());

                    mst.Commit();

                    this.CloseConnection();
                }
                catch (MySqlException ex0)
                {
                    try
                    {
                        mst.Rollback();
                    }
                    catch (MySqlException ex1)
                    {
                    }

                }
                finally
                {
                    this.CloseConnection();
                }
                return result;
            }
            else
            {
                return -2;
            }

        }

        public int CreateEventType(EventTypeModel data)
        {
            if (this.OpenConnection() == true)
            {
                MySqlTransaction mst = null;
                MySqlCommand cmd = null;
                int result = -1;

                //try
                //{
                    mst = connection.BeginTransaction();
                    cmd = new MySqlCommand();
                    cmd.Connection = connection;
                    cmd.Transaction = mst;

                    string insert = "INSERT INTO pksudb.eventtypes (eventTypeName) VALUES ('" + data.Name + "'); "
                                + "SELECT last_insert_id();";

                    cmd.CommandText = insert;
                    result = Convert.ToInt32(cmd.ExecuteScalar());

                    string createTable = "CREATE TABLE pksudb.table_" + result + "("
                                            + "eventId int NOT NULL, ";

                    foreach (FieldDataModel fdm in data.TypeSpecific)
                    {
                        string insertField = "INSERT INTO pksudb.eventtypefields"
                                            + "(eventTypeId, fieldName, fieldDescription, requiredField, fieldType)"
                                            + "VALUES (" + result + ",'" + fdm.Name + "','" + fdm.Description + "',"
                                            + (fdm.Required ? "1," : "0,") + fdm.Datatype + "); SELECT last_insert_id();";

                        cmd.CommandText = insertField;
                        int id = Convert.ToInt32(cmd.ExecuteScalar());

                        createTable += "field_" + id + " " + fdm.GetDBType() + ", ";
                    }

                    createTable += "PRIMARY KEY (eventId), "
                                 + "CONSTRAINT eventIdCons_" + result + " "
                                 + "FOREIGN KEY (eventId) REFERENCES pksudb.events (eventId) "
                                 + "ON DELETE NO ACTION "
                                 + "ON UPDATE NO ACTION "
                                 + ");";

                    cmd.CommandText = createTable;
                    cmd.ExecuteNonQuery();

                    mst.Commit();

                    this.CloseConnection();
                /*}
                catch (MySqlException ex0)
                {
                    try
                    {
                        mst.Rollback();
                    }
                    catch (MySqlException ex1)
                    {
                    }

                }
                finally
                {
                    this.CloseConnection();
                }*/
                return result;
            }
            else
            {
                return -2;
            }

        }

        public int EditEventType(EventTypeModel data)
        {
            if (this.OpenConnection() == true)
            {
                MySqlTransaction mst = null;
                MySqlCommand cmd = null;

              //  try
              //  {
                    mst = connection.BeginTransaction();
                    cmd = new MySqlCommand();
                    cmd.Connection = connection;
                    cmd.Transaction = mst;

                    string updateET = "UPDATE eventtypes SET eventTypeName = '"+data.Name+"' WHERE eventTypeId = "+data.ID;

                    cmd.CommandText = updateET;
                    cmd.ExecuteNonQuery();

                    
                    foreach (FieldDataModel fdm in data.TypeSpecific)
                    {
                        if (fdm.ViewID == -1)
                        {
                            //We have to remove the field, as it was found in the db.
                            string updateField = "DELETE FROM pksudb.eventtypefields WHERE fieldId = " + fdm.ID;
                            string alterEventTable = "ALTER TABLE table_" + data.ID + " DROP COLUMN field_" + fdm.ID;

                            cmd.CommandText = alterEventTable;
                            cmd.ExecuteNonQuery();

                            cmd.CommandText = updateField;
                            cmd.ExecuteNonQuery();
                        }
                        else if(fdm.ViewID == -2)
                        {
                            string insertField = "INSERT INTO pksudb.eventtypefields"
                                                 + "(eventTypeId, fieldName, fieldDescription, requiredField, fieldType)"
                                                 + "VALUES (" + data.ID + ",'" + fdm.Name + "','" + fdm.Description + "',"
                                                 + (fdm.Required ? "1," : "0,") + fdm.Datatype + "); SELECT last_insert_id();";

                            cmd.CommandText = insertField;
                            int id = Convert.ToInt32(cmd.ExecuteScalar());

                            string alterEventTable = "ALTER TABLE table_" + data.ID + " ADD field_" + id + " " + fdm.GetDBType();
                            cmd.CommandText = alterEventTable;
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            string updateField = "UPDATE pksudb.eventtypefields SET "
                                                    + "fieldName = '" + fdm.Name
                                                    + "', fieldDescription = '" + fdm.Description
                                                    + "', requiredField = " + (fdm.Required ? "1" : "0")
                                                    + ", fieldType = " + fdm.Datatype
                                                    + " WHERE eventTypeId = " + data.ID
                                                        + " AND fieldId = " + fdm.ID;

                            cmd.CommandText = updateField;
                            cmd.ExecuteNonQuery();
                        }

                    }

                    mst.Commit();

                    this.CloseConnection();
             /*   }
                catch (MySqlException ex0)
                {
                    try
                    {
                        mst.Rollback();
                    }
                    catch (MySqlException ex1)
                    {
                    }

                }
                finally
                {
                    this.CloseConnection();
                }*/
                return 1;
            }
            else
            {
                return -2;
            }

        }
    }
}