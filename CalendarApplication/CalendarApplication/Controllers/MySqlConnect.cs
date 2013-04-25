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
                try
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
                    dataReader.Close();
                    this.CloseConnection();

                    return dt;
                }
                catch (MySqlException ex0)
                {
                    this.ErrorMessage = "A database error occurred: " + ex0.Message;
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
                catch (MySqlException ex0)
                {
                    this.ErrorMessage = "A database error occurred: " + ex0.Message;
                    return null;
                }
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
                    mst = this.connection.BeginTransaction();
                    cmd = new MySqlCommand();
                    cmd.Connection = this.connection;
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
                        this.CloseConnection();
                        ErrorMessage = "Some databse error occured: Discarded changes, Error message: " + ex0.Message
                                        + ", Caused by: "+cmd.CommandText;
                        return -1;
                    }
                    catch (MySqlException ex1)
                    {
                        this.CloseConnection();
                        ErrorMessage = "Some databse error occured: Could not discard changes, DB corrupt, Error message: " + ex1.Message
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

        public bool CreateEventType(EventTypeModel data)
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

                    string insert = "INSERT INTO pksudb.eventtypes (eventTypeName) VALUES ('" + data.Name + "'); "
                                + "SELECT last_insert_id();";

                    cmd.CommandText = insert;
                    result = Convert.ToInt32(cmd.ExecuteScalar());

                    if (data.TypeSpecific != null)
                    {
                        string createTable = "CREATE TABLE pksudb.table_" + result + "("
                                                + "eventId int NOT NULL, ";

                        foreach (FieldDataModel fdm in data.TypeSpecific)
                        {
                            string insertField = "INSERT INTO pksudb.eventtypefields"
                                                + "(eventTypeId, fieldName, fieldDescription, requiredField, fieldType, varCharLength)"
                                                + "VALUES (" + result + ",'" + fdm.Name + "','" + fdm.Description + "',"
                                                + (fdm.Required ? "1," : "0,") + (int)fdm.Datatype + "," + fdm.VarcharLength
                                                + "); SELECT last_insert_id();";

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
                    }

                    mst.Commit();

                    this.CloseConnection();
                }
                catch (MySqlException ex0)
                {
                    try
                    {
                        mst.Rollback();
                        this.CloseConnection();
                        ErrorMessage = "Some databse error occured: Discarded changes, Error message: " + ex0.Message
                                        + ", Caused by: " + cmd.CommandText;
                        return false;
                    }
                    catch (MySqlException ex1)
                    {
                        this.CloseConnection();
                        ErrorMessage = "Some databse error occured: Could not discard changes, DB corrupt, Error message: " + ex1.Message
                                        + ", Caused by: " + cmd.CommandText;
                        return false;
                    }
                }
                return true;
            }
            else
            {
                ErrorMessage = "Could not open connection to database!";
                return false;
            }
        }

        public bool EditEventType(EventTypeModel data)
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

                    string updateET = "UPDATE eventtypes SET eventTypeName = '"+data.Name+"' WHERE eventTypeId = "+data.ID;

                    cmd.CommandText = updateET;
                    cmd.ExecuteNonQuery();

                    
                    foreach (FieldDataModel fdm in data.TypeSpecific)
                    {
                        string alterEventTable;

                        if (fdm.ViewID == -1)
                        {
                            //We have to remove the field, as it was found in the db.
                            string updateField = "DELETE FROM pksudb.eventtypefields WHERE fieldId = " + fdm.ID;
                            alterEventTable = "ALTER TABLE table_" + data.ID + " DROP COLUMN field_" + fdm.ID;

                            cmd.CommandText = updateField;
                            cmd.ExecuteNonQuery();
                        }
                        else if(fdm.ViewID == -2)
                        {
                            string insertField = "INSERT INTO pksudb.eventtypefields"
                                                 + "(eventTypeId, fieldName, fieldDescription, requiredField, fieldType, varCharLength)"
                                                 + "VALUES (" + data.ID + ",'" + fdm.Name + "','" + fdm.Description + "',"
                                                 + (fdm.Required ? "1," : "0,") + (int)fdm.Datatype + ","+ fdm.VarcharLength
                                                 + "); SELECT last_insert_id();";

                            cmd.CommandText = insertField;
                            int id = Convert.ToInt32(cmd.ExecuteScalar());

                            alterEventTable = "ALTER TABLE table_" + data.ID + " ADD field_" + id + " " + fdm.GetDBType();
                        }
                        else
                        {
                            string updateField = "UPDATE pksudb.eventtypefields SET "
                                                    + "fieldName = '" + fdm.Name
                                                    + "', fieldDescription = '" + fdm.Description
                                                    + "', requiredField = " + (fdm.Required ? "1" : "0")
                                                    + ", fieldType = " + (int)fdm.Datatype
                                                    + " WHERE eventTypeId = " + data.ID
                                                        + " AND fieldId = " + fdm.ID;

                            alterEventTable = "ALTER TABLE table_" + data.ID + " MODIFY COLUMN field_" + fdm.ID + " " + fdm.GetDBType();

                            cmd.CommandText = updateField;
                            cmd.ExecuteNonQuery();
                        }

                        cmd.CommandText = alterEventTable;
                        cmd.ExecuteNonQuery();

                    }

                    mst.Commit();

                    this.CloseConnection();
                }
                catch (MySqlException ex0)
                {
                    try
                    {
                        mst.Rollback();
                        this.CloseConnection();
                        ErrorMessage = "Some databse error occured: Discarded changes, Error message: " + ex0.Message
                                        + ", Caused by: " + cmd.CommandText;
                        return false;
                    }
                    catch (MySqlException ex1)
                    {
                        this.CloseConnection();
                        ErrorMessage = "Some databse error occured: Could not discard changes, DB corrupt, Error message: " + ex1.Message
                                        + ", Caused by: " + cmd.CommandText;
                        return false;
                    }
                }
                return true;
            }
            else
            {
                ErrorMessage = "Could not open connection to database!";
                return false;
            }

        }

        public bool EditEvent(EventEditModel eem)
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
                    int newId;

                    string prevType = eem.SelectedEventType;
                    if (eem.ID != -1)
                    {
                        cmd.CommandText = "SELECT eventTypeId FROM pksudb.events WHERE eventId == " + eem.ID;
                        prevType = "" + Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    string updateEventTable = eem.ID == -1 ?
                                                "INSERT INTO pksudb.events" +
                                                "(userId,eventTypeId,eventName,eventStart,eventEnd,visible,state) VALUES " +
                                                "(" + eem.CreatorId + "," + eem.SelectedEventType + ",'" + eem.Name + "','" +
                                                eem.Start.ToString("yyyy-MM-dd hh:mm:ss") + "','" + eem.End.ToString("yyyy-MM-dd hh:mm:ss") + "'," +
                                                (eem.Visible ? "1" : "0") + "," + eem.State + "); SELECT last_insert_id();" :
                                                "UPDATE pksudb.events SET eventTypeId = " + eem.SelectedEventType +
                                                ", eventName = '" + eem.Name + "', eventStart = '" + eem.Start.ToString("yyyy-MM-dd hh:mm:ss") +
                                                "', eventEnd = '" + eem.End.ToString("yyyy-MM-dd hh:mm:ss") + "', visible = " +
                                                (eem.Visible?"1":"0") + ", state = " + eem.State + " WHERE eventId = " + eem.ID;

                    cmd.CommandText = updateEventTable;
                    if (eem.ID == -1) { newId = Convert.ToInt32(cmd.ExecuteScalar()); }
                    else
                    {
                        cmd.ExecuteNonQuery();
                        newId = eem.ID;
                    }
                    

                    // Check if type has changed, clean up if it has...
                    if (!prevType.Equals(eem.SelectedEventType))
                    {
                        cmd.CommandText = "SHOW TABLES LIKE 'table_" + prevType + "'";
                        MySqlDataReader dataReader = cmd.ExecuteReader();
                        bool rows = dataReader.HasRows;
                        dataReader.Close();
                        if (rows)
                        {
                            // The old type has a table with a entry, remove this entry.
                            string delete = "DELETE FROM table_" + prevType + " WHERE eventId == " + eem.ID;
                            cmd.CommandText = delete;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Check if (new) type has a specifics table
                    if (eem.TypeSpecifics != null)
                    {
                        string updateTable;
                        if (eem.ID == -1 || !prevType.Equals(eem.SelectedEventType))
                        {
                            // We have to insert because it is a create, or we have changed the type...
                            string prologue = "INSERT INTO pksudb.table_" + eem.SelectedEventType + " (eventId,";
                            string epilogue = " VALUES (" + newId + ",";
                            for (int i = 0; i < eem.TypeSpecifics.Count; i++)
                            {
                                FieldModel fm = eem.TypeSpecifics[i];
                                prologue += "field_" + fm.ID;
                                epilogue += fm.GetDBValue();
                                if (i < eem.TypeSpecifics.Count - 1)
                                {
                                    prologue += ",";
                                    epilogue += ",";
                                }
                            }
                            updateTable = prologue + ")" + epilogue + ")";
                        }
                        else
                        {
                            // We only have to update
                            updateTable = "UPDATE pksudb.table_" + eem.SelectedEventType + " SET ";
                            for (int i = 0; i < eem.TypeSpecifics.Count; i++)
                            {
                                FieldModel fm = eem.TypeSpecifics[i];
                                updateTable += "field_" + fm.ID + " = " + fm.GetDBValue();
                                if (i < eem.TypeSpecifics.Count - 1)
                                {
                                    updateTable += ",";
                                }
                            }
                            updateTable += " WHERE eventId == " + eem.ID;
                        }
                        cmd.CommandText = updateTable;
                        cmd.ExecuteNonQuery();
                    }

                    if (eem.ID == -1)
                    {
                        foreach (SelectListItem room in eem.RoomSelectList)
                        {
                            if (room.Selected)
                            {
                                cmd.CommandText = "INSERT INTO pksudb.eventroomsused(eventId,roomId) VALUES ("
                                                    + newId + "," + room.Value + ")";
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    else
                    {
                        //Handle room edit here.
                    }

                    mst.Commit();
                }
                catch (MySqlException ex0)
                {
                    try
                    {
                        mst.Rollback();
                        this.CloseConnection();
                        ErrorMessage = "Some databse error occured: Discarded changes, Error message: " + ex0.Message
                                        + ", Caused by: " + cmd.CommandText;
                        return false;
                    }
                    catch (MySqlException ex1)
                    {
                        this.CloseConnection();
                        ErrorMessage = "Some databse error occured: Could not discard changes, DB corrupt, Error message: " + ex1.Message
                                        + ", Caused by: " + cmd.CommandText;
                        return false;
                    }
                }
                return true;
            }
            else
            {
                ErrorMessage = "Could not open connection to database!";
                return false;
            }
        }
    }
}