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
                    if (query.ArgNames.Length > 0 && query.Args.Length > 0)
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

        /*
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
                        CreatorId = (int)dataReader["userId"],
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

        }*/

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

        public int CreateUser(Register data)
        {
            string insert = "INSERT INTO pksudb.users (userName,password,realName,email,active,needsApproval) " +
                            "VALUES (@username, @password, @realname, @email, 1, 1);"
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
                    cmd.Parameters.AddWithValue("@username", data.UserName);
                    cmd.Parameters.AddWithValue("@password", data.Password);
                    cmd.Parameters.AddWithValue("@realname", data.RealName);
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
                                        + ", Caused by: "+cmd.CommandText;
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

                    string insert = "INSERT INTO pksudb.eventtypes (eventTypeName) VALUES (@name); "
                                + "SELECT last_insert_id();";

                    string foreignkeys = "";

                    cmd.CommandText = insert;
                    cmd.Parameters.AddWithValue("@name", data.Name);
                    cmd.Prepare();
                    result = Convert.ToInt32(cmd.ExecuteScalar());

                    string createTable = "CREATE TABLE pksudb.table_" + result + "("
                                                + "eventId int NOT NULL, ";

                    if (data.TypeSpecific != null)
                    {
                        
                        string insertField = "INSERT INTO pksudb.eventtypefields "
                                             + "(eventTypeId, fieldName, fieldDescription, requiredField, fieldType, varCharLength) "
                                             + "VALUES (@typeid , @fieldname , @descr , @req , @datatype , @varch ); "
                                             + "SELECT last_insert_id();";

                        cmd.Parameters.AddWithValue("@typeid", result);
                        cmd.Parameters.AddWithValue("@fieldname", null);
                        cmd.Parameters.AddWithValue("@descr", null);
                        cmd.Parameters.AddWithValue("@req", null);
                        cmd.Parameters.AddWithValue("@datatype", null);
                        cmd.Parameters.AddWithValue("@varch", null);
                        cmd.CommandText = insertField;

                        foreach (FieldDataModel fdm in data.TypeSpecific)
                        {
                            cmd.Parameters["@fieldname"].Value = fdm.Name;
                            cmd.Parameters["@descr"].Value = fdm.Description;
                            cmd.Parameters["@req"].Value = fdm.Required;
                            cmd.Parameters["@datatype"].Value = fdm.GetTypeAsInt();
                            cmd.Parameters["@varch"].Value = fdm.VarcharLength;

                            cmd.Prepare();

                            int id = Convert.ToInt32(cmd.ExecuteScalar());

                            createTable += "field_" + id + " " + fdm.GetDBType() + ", ";
                            switch(fdm.Datatype)
                            {
                                case Fieldtype.User:
                                    foreignkeys += ", "
                                     + "CONSTRAINT fieldIdCons_" + id
                                     + " FOREIGN KEY (field_" + id + ") REFERENCES pksudb.users (userId) "
                                     + "ON DELETE SET NULL "
                                     + "ON UPDATE NO ACTION ";
                                     break;
                                case Fieldtype.Group:
                                    foreignkeys += ", "
                                     + "CONSTRAINT fieldIdCons_" + id
                                     + " FOREIGN KEY (field_" + id + ") REFERENCES pksudb.groups (groupId) "
                                     + "ON DELETE SET NULL "
                                     + "ON UPDATE NO ACTION ";
                                     break;
                                case Fieldtype.File: 
                                    foreignkeys += ", "
                                     + "CONSTRAINT fieldIdCons_" + id
                                     + " FOREIGN KEY (field_" + id + ") REFERENCES pksudb.files (fileId) "
                                     + "ON DELETE SET NULL "
                                     + "ON UPDATE NO ACTION ";
                                     break;
                                default: break;
                            }
                        }
                    }

                    createTable += "PRIMARY KEY (eventId), "
                                     + "CONSTRAINT eventIdCons_" + result + " "
                                     + "FOREIGN KEY (eventId) REFERENCES pksudb.events (eventId) "
                                     + "ON DELETE CASCADE "
                                     + "ON UPDATE NO ACTION " + foreignkeys
                                     + ");";

                    cmd.CommandText = createTable;
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

                    string updateET = "UPDATE eventtypes SET eventTypeName = @etname WHERE eventTypeId = @etid";

                    cmd.CommandText = updateET;
                    cmd.Parameters.AddWithValue("@etname", data.Name);
                    cmd.Parameters.AddWithValue("@etid", data.ID);
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    if (data.TypeSpecific != null)
                    {
                        cmd.Parameters.AddWithValue("@fid", null);
                        cmd.Parameters.AddWithValue("@fname", null);
                        cmd.Parameters.AddWithValue("@fdescr", null);
                        cmd.Parameters.AddWithValue("@freq", null);
                        cmd.Parameters.AddWithValue("@fdattyp", null);
                        cmd.Parameters.AddWithValue("@fvarchr", null);

                        string updateField = "UPDATE pksudb.eventtypefields SET fieldName = @fname , fieldDescription = @fdescr"
                                             + " , requiredField = @freq , fieldType = @fdattyp WHERE eventTypeId = @etid"
                                             + " AND fieldId = @fid;";

                        string insertField = "INSERT INTO pksudb.eventtypefields"
                                             + " (eventTypeId, fieldName, fieldDescription, requiredField, fieldType, varCharLength)"
                                             + " VALUES ( @etid , @fname , @fdescr , @freq , @fdattyp , @fvarchr);"
                                             + "SELECT last_insert_id();";

                        string removeField = "DELETE FROM pksudb.eventtypefields WHERE fieldId = @fid";

                        foreach (FieldDataModel fdm in data.TypeSpecific)
                        {
                            string alterEventTable;

                            if (fdm.ViewID == -1)
                            {
                                alterEventTable = "ALTER TABLE table_" + data.ID + " DROP COLUMN field_" + fdm.ID;

                                //We have to remove the field, as it was found in the db.
                                cmd.CommandText = removeField;
                                cmd.Parameters["@fid"].Value = fdm.ID;
                                cmd.Prepare();
                                cmd.ExecuteNonQuery();
                            }
                            else if (fdm.ViewID == -2)
                            {

                                cmd.CommandText = insertField;
                                cmd.Parameters["@fname"].Value = fdm.Name;
                                cmd.Parameters["@fdescr"].Value = fdm.Description;
                                cmd.Parameters["@freq"].Value = fdm.Required;
                                cmd.Parameters["@fdattyp"].Value = fdm.GetTypeAsInt();
                                cmd.Parameters["@fvarchr"].Value = fdm.VarcharLength;
                                cmd.Prepare();
                                int id = Convert.ToInt32(cmd.ExecuteScalar());

                                alterEventTable = "ALTER TABLE table_" + data.ID + " ADD field_" + id + " " + fdm.GetDBType();
                            }
                            else
                            {

                                alterEventTable = "ALTER TABLE table_" + data.ID + " MODIFY COLUMN field_" + fdm.ID + " " + fdm.GetDBType();

                                cmd.CommandText = updateField;
                                cmd.Parameters["@fname"].Value = fdm.Name;
                                cmd.Parameters["@fdescr"].Value = fdm.Description;
                                cmd.Parameters["@freq"].Value = fdm.Required;
                                cmd.Parameters["@fdattyp"].Value = fdm.GetTypeAsInt();
                                cmd.Parameters["@fid"].Value = fdm.ID;
                                cmd.Prepare();
                                cmd.ExecuteNonQuery();
                            }

                            cmd.CommandText = alterEventTable;
                            cmd.ExecuteNonQuery();
                        }
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
                        cmd.CommandText = "SELECT eventTypeId FROM pksudb.events WHERE eventId = @eid";
                        cmd.Parameters.AddWithValue("@eid", eem.ID);
                        cmd.Prepare();
                        prevType = "" + Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    string updateEventTable = eem.ID == -1 ? (
                                                "INSERT INTO pksudb.events" +
                                                "(userId,eventTypeId,eventName,eventStart,eventEnd,visible,state) VALUES " +
                                                "( @ecreatorid , @eselectet , @ename , @estart , @eend , @evisible , @estate );"
                                                + "SELECT last_insert_id();") : (
                                                "UPDATE pksudb.events SET eventTypeId = @eselectet , eventName = @ename , "
                                                + "eventStart = @estart , eventEnd = @eend , visible = @evisible , "
                                                + " state = @estate WHERE eventId = @eid;");

                    cmd.CommandText = updateEventTable;
                    cmd.Parameters.AddWithValue("@ecreatorid", eem.CreatorId);
                    cmd.Parameters.AddWithValue("@eselectet", eem.SelectedEventType);
                    cmd.Parameters.AddWithValue("@ename", eem.Name);
                    cmd.Parameters.AddWithValue("@estart", eem.Start);
                    cmd.Parameters.AddWithValue("@eend", eem.End);
                    cmd.Parameters.AddWithValue("@evisible", eem.Visible);
                    cmd.Parameters.AddWithValue("@estate", eem.State);
                    cmd.Prepare();
                    if (eem.ID == -1) { newId = Convert.ToInt32(cmd.ExecuteScalar()); }
                    else
                    {
                        cmd.ExecuteNonQuery();
                        newId = eem.ID;
                    }

                    cmd.Parameters.AddWithValue("@nid", newId);

                    // Check if type has changed, clean up if it has...
                    bool changed = !String.IsNullOrEmpty(prevType) && !prevType.Equals(eem.SelectedEventType);
                    if (changed)
                    {
                        cmd.CommandText = "SHOW TABLES LIKE 'table_" + prevType + "'";
                        MySqlDataReader dataReader = cmd.ExecuteReader();
                        bool rows = dataReader.HasRows;
                        dataReader.Close();
                        if (rows)
                        {
                            // The old type has a table with a entry, remove this entry.
                            string delete = "DELETE FROM table_" + prevType + " WHERE eventId = @nid";
                            cmd.CommandText = delete;
                            cmd.Prepare();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Check if (new) type has a specifics table
                    if (eem.TypeSpecifics != null)
                    {
                        string updateTable;
                        if (eem.ID == -1 || changed)
                        {
                            // We have to insert because it is a create, or we have changed the type...
                            string prologue = "INSERT INTO pksudb.table_" + eem.SelectedEventType + " ( eventId , ";
                            string epilogue = " VALUES ( @nid , ";

                            for (int i = 0; i < eem.TypeSpecifics.Count; i++)
                            {
                                FieldModel fm = eem.TypeSpecifics[i];
                                prologue += "field_" + fm.ID;
                                string value = "@fieldval" + i;
                                epilogue += value;
                                cmd.Parameters.AddWithValue(value, fm.GetDBValue());
                                if (i < eem.TypeSpecifics.Count - 1)
                                {
                                    prologue += " , ";
                                    epilogue += " , ";
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
                                string value = "@fieldvalue" + i;
                                updateTable += "field_" + fm.ID + " = " + value;
                                cmd.Parameters.AddWithValue(value, fm.GetDBValue());
                                if (i < eem.TypeSpecifics.Count - 1)
                                {
                                    updateTable += " , ";
                                }
                            }
                            updateTable += " WHERE eventId = @nid ;";
                        }
                        cmd.CommandText = updateTable;
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }

                    //Handle room edit/create here.
                    //Currently deletes everything and inserts again, even if unchanged.
                    //Could maybe be handled better.
                    string roomInsert = "INSERT INTO pksudb.eventroomsused(eventId,roomId) VALUES ( @nid , @rmval );";
                    cmd.Parameters.AddWithValue("@rmval", null);
                    cmd.CommandText = "DELETE FROM pksudb.eventroomsused WHERE eventId = @nid ;";
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = roomInsert;
                    cmd.Prepare();
                    foreach (SelectListItem room in eem.RoomSelectList)
                    {
                        if (room.Selected)
                        {
                            cmd.Parameters["@rmval"].Value = room.Value;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    

                    // Handle the editor rights. The below is a primitive implementation. Simply deletes
                    // all date previously there and inserts the new data. Works for creation and edit
                    cmd.CommandText = "DELETE FROM pksudb.eventeditorsusers WHERE eventId = @nid";
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "INSERT INTO pksudb.eventeditorsusers(eventId,userId)"
                                                  + " VALUES ( @nid , @editusr );";
                    cmd.Parameters.AddWithValue("@editusr", null);
                    cmd.Prepare();
                    foreach (SelectListItem edtusr in eem.UserEditorList)
                    {
                        if (edtusr.Selected)
                        {
                            cmd.Parameters["@editusr"].Value = edtusr.Value;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    cmd.CommandText = "DELETE FROM pksudb.eventeditorsgroups WHERE eventId = @nid";
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "INSERT INTO pksudb.eventeditorsgroups(eventId,groupId)"
                                                  + " VALUES ( @nid , @edtgrpid );";
                    cmd.Parameters.AddWithValue("@edtgrpid", null);
                    cmd.Prepare();
                    foreach (SelectListItem edtgrp in eem.GroupEditorList)
                    {
                        if (edtgrp.Selected)
                        {
                            cmd.Parameters["@grpid"].Value = edtgrp.Value;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Handle visibility, same way as above, except that we check if the event has global visibility
                    cmd.CommandText = "DELETE FROM pksudb.eventvisibility WHERE eventId = @nid";
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                    if (!eem.Visible)
                    {
                        cmd.CommandText = "INSERT INTO pksudb.eventvisibility(eventId,groupId)"
                                                      + " VALUES ( @nid , @grpid );";
                        cmd.Parameters.AddWithValue("@grpid", null);
                        cmd.Prepare();
                        foreach (SelectListItem edtgrp in eem.GroupVisibleList)
                        {
                            if (edtgrp.Selected)
                            {
                                cmd.Parameters["@grpid"].Value = edtgrp.Value;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    mst.Commit();
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
                ErrorMessage = "Could not open connection to database!";
                return false;
            }
        }

        /// <summary>
        /// Create a new group -WIP
        /// </summary>
        /// <param name="groupmodel">Model with data to be stored</param>
        /// <returns>bool indicating success or failure</returns>
        public bool CreateGroup(GroupModel groupmodel)
        {
            return false;
        }

        /// <summary>
        /// Edit an existing group -WIP
        /// </summary>
        /// <param name="groupmodel"></param>
        /// <returns>bool indicating success or failure</returns>
        public bool EditGroup(GroupModel groupmodel)
        {
            //NOT FINISHED
            string cmd = "UPDATE groups SET groupName = @groupName WHERE groupId = @groupId";
            string[] argnames = { "@groupName", "@groupId" };
            object[] args = { groupmodel.Name, groupmodel.ID };
            CustomQuery query = new CustomQuery { Cmd = cmd, ArgNames = argnames, Args = args };
            MySqlConnect msc = new MySqlConnect();
            msc.ExecuteQuery(query);
            
            
            return false;
        }
    }
}