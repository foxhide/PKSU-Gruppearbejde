using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySql.Data.MySqlClient;

using CalendarApplication.Models.Event;
using CalendarApplication.Models.EventType;
using System.IO;

namespace CalendarApplication.Database
{
    public class MySqlEvent : MySqlConnect
    {

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

                    string insert = "INSERT INTO eventtypes (eventTypeName) VALUES (@name); "
                                + "SELECT last_insert_id();";

                    string foreignkeys = "";

                    cmd.CommandText = insert;
                    cmd.Parameters.AddWithValue("@name", data.Name);
                    cmd.Prepare();
                    result = Convert.ToInt32(cmd.ExecuteScalar());

                    string createTable = "CREATE TABLE table_" + result + "("
                                                + "eventId int NOT NULL, ";

                    if (data.TypeSpecific != null)
                    {

                        string insertField = "INSERT INTO eventtypefields "
                                             + "(eventTypeId, fieldName, fieldDescription, requiredCreation, requiredApproval, fieldType, varCharLength, fieldOrder) "
                                             + "VALUES (@typeid , @fieldname , @descr , @reqc , @reqa , @datatype , @varch , @order ); "
                                             + "SELECT last_insert_id();";

                        cmd.Parameters.AddWithValue("@typeid", result);
                        cmd.Parameters.AddWithValue("@fieldname", null);
                        cmd.Parameters.AddWithValue("@descr", null);
                        cmd.Parameters.AddWithValue("@reqc", null);
                        cmd.Parameters.AddWithValue("@reqa", null);
                        cmd.Parameters.AddWithValue("@datatype", null);
                        cmd.Parameters.AddWithValue("@varch", null);
                        cmd.Parameters.AddWithValue("@order", null);
                        cmd.CommandText = insertField;

                        for (int i = 0; i < data.TypeSpecific.Count; i++)
                        {
                            FieldDataModel fdm = data.TypeSpecific[i];

                            cmd.Parameters["@fieldname"].Value = fdm.Name;
                            cmd.Parameters["@descr"].Value = fdm.Description;
                            cmd.Parameters["@reqc"].Value = fdm.RequiredCreate;
                            cmd.Parameters["@reqa"].Value = fdm.RequiredApprove || fdm.RequiredCreate;
                            cmd.Parameters["@datatype"].Value = fdm.GetTypeAsInt();
                            cmd.Parameters["@varch"].Value = fdm.VarcharLength;
                            cmd.Parameters["@order"].Value = fdm.ViewID;

                            cmd.Prepare();

                            int id = Convert.ToInt32(cmd.ExecuteScalar());

                            createTable += "field_" + id + " " + fdm.GetDBType() + ", ";
                            switch (fdm.Datatype)
                            {
                                case Fieldtype.User:
                                    foreignkeys += ", "
                                     + "CONSTRAINT fieldIdCons_" + id
                                     + " FOREIGN KEY (field_" + id + ") REFERENCES users (userId) "
                                     + "ON DELETE SET NULL "
                                     + "ON UPDATE NO ACTION ";
                                    break;
                                case Fieldtype.Group:
                                    foreignkeys += ", "
                                     + "CONSTRAINT fieldIdCons_" + id
                                     + " FOREIGN KEY (field_" + id + ") REFERENCES groups (groupId) "
                                     + "ON DELETE SET NULL "
                                     + "ON UPDATE NO ACTION ";
                                    break;
                                case Fieldtype.File:
                                    foreignkeys += ", "
                                     + "CONSTRAINT fieldIdCons_" + id
                                     + " FOREIGN KEY (field_" + id + ") REFERENCES files (fileId) "
                                     + "ON DELETE SET NULL "
                                     + "ON UPDATE NO ACTION ";
                                    break;
                                default: break;
                            }
                        }
                    }

                    createTable += "PRIMARY KEY (eventId), "
                                     + "CONSTRAINT eventIdCons_" + result + " "
                                     + "FOREIGN KEY (eventId) REFERENCES events (eventId) "
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
                return false;
            }
        }

        /// <summary>
        /// Tries to edit an event type in the database, using the data provided in the EventTypeModel
        /// </summary>
        /// <param name="data">The data for the event type</param>
        /// <returns></returns>
        public bool EditEventType(EventTypeModel data)
        {
            if (this.OpenConnection() == true)
            {
                MySqlTransaction mst = null;
                MySqlCommand cmd = null;
                List<string> filesToDelete = new List<string>();

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
                        cmd.Parameters.AddWithValue("@freqc", null);
                        cmd.Parameters.AddWithValue("@freqa", null);
                        cmd.Parameters.AddWithValue("@fdattyp", null);
                        cmd.Parameters.AddWithValue("@fvarchr", null);
                        cmd.Parameters.AddWithValue("@forder", null);

                        string updateField = "UPDATE eventtypefields SET fieldName = @fname , fieldDescription = @fdescr"
                                             + " , requiredCreation = @freqc , requiredApproval = @freqa , varCharLength = @fvarchr, fieldOrder = @forder"
                                             + " WHERE eventTypeId = @etid AND fieldId = @fid;";

                        string insertField = "INSERT INTO eventtypefields"
                                             + " (eventTypeId, fieldName, fieldDescription, requiredCreation, requiredApproval , fieldType, varCharLength, fieldOrder)"
                                             + " VALUES ( @etid , @fname , @fdescr , @freqc , @freqa , @fdattyp , @fvarchr, @forder);"
                                             + "SELECT last_insert_id();";

                        string removeField = "DELETE FROM eventtypefields WHERE fieldId = @fid";

                        /* fdm.ID holds the ID of the field in the db. If fdm.ID == -1, a new field is created.
                         * fdm.ViewID holds the ViewID of the field. This is used to determine the order of the fields.
                         * If a field is removed, the ViewID is set to -1.
                         */
                        foreach (FieldDataModel fdm in data.TypeSpecific)
                        {
                            string alterEventTable = "";
                            bool altered = false;
                            if (fdm.ViewID == -1)
                            {
                                //field was removed by user -> remove it from the database

                                //remove foreign keys before dropping column if necessary
                                //also check for files and delete/dangle if necessary
                                List<string> files = new List<string>();
                                bool keyDel = false;
                                switch (fdm.Datatype)
                                {
                                    case Fieldtype.User:
                                        alterEventTable = "ALTER TABLE table_" + data.ID + " DROP FOREIGN KEY fieldIdCons_" + fdm.ID
                                                          + " , DROP INDEX fieldIdCons_" + fdm.ID + " , DROP COLUMN field_" + fdm.ID + " ;";
                                        keyDel = true;
                                        break;
                                    case Fieldtype.Group:
                                        alterEventTable = "ALTER TABLE table_" + data.ID + " DROP FOREIGN KEY fieldIdCons_" + fdm.ID
                                                          + " , DROP INDEX fieldIdCons_" + fdm.ID + " , DROP COLUMN field_" + fdm.ID + " ;";
                                        keyDel = true;
                                        break;
                                    case Fieldtype.File:
                                        alterEventTable = "ALTER TABLE table_" + data.ID + " DROP FOREIGN KEY fieldIdCons_" + fdm.ID
                                                          + " , DROP INDEX fieldIdCons_" + fdm.ID + " , DROP COLUMN field_" + fdm.ID + " ;";
                                        keyDel = true;
                                        // remove files' association with this field
                                        // perhaps delete?
                                        files = HandleDanglingFiles(data.ID, fdm.ID, mst, fdm.FileDelete);
                                        foreach (string file in files)
                                        {
                                            filesToDelete.Add(file);
                                        }
                                        break;
                                    case Fieldtype.FileList:
                                        // remove files' association with this field
                                        // perhaps delete?
                                        files = HandleDanglingFileList(fdm.ID, mst, fdm.FileDelete);
                                        foreach (string file in files)
                                        {
                                            filesToDelete.Add(file);
                                        }
                                        break;
                                    default: break;
                                }



                                if (!keyDel) { alterEventTable = "ALTER TABLE table_" + data.ID + " DROP COLUMN field_" + fdm.ID; }

                                //We have to remove the field, as it was found in the db.
                                cmd.CommandText = removeField;
                                cmd.Parameters["@fid"].Value = fdm.ID;
                                cmd.Prepare();
                                cmd.ExecuteNonQuery();
                                altered = true;
                            }
                            else if (fdm.ID == -1)
                            {
                                //a new field was added
                                cmd.CommandText = insertField;
                                cmd.Parameters["@fname"].Value = fdm.Name;
                                cmd.Parameters["@fdescr"].Value = fdm.Description;
                                cmd.Parameters["@freqc"].Value = fdm.RequiredCreate;
                                cmd.Parameters["@freqa"].Value = fdm.RequiredApprove || fdm.RequiredCreate;
                                cmd.Parameters["@fdattyp"].Value = fdm.GetTypeAsInt();
                                cmd.Parameters["@fvarchr"].Value = fdm.VarcharLength;
                                cmd.Parameters["@forder"].Value = fdm.ViewID;
                                cmd.Prepare();
                                int id = Convert.ToInt32(cmd.ExecuteScalar());

                                alterEventTable = "ALTER TABLE table_" + data.ID + " ADD COLUMN field_" + id + " " + fdm.GetDBType();

                                //add foreign keys after adding column if necessary
                                switch (fdm.Datatype)
                                {
                                    case Fieldtype.User:
                                        alterEventTable += " , ADD CONSTRAINT "
                                                           + "fieldIdCons_" + id
                                                           + " FOREIGN KEY (field_" + id + ") REFERENCES users (userId) "
                                                           + "ON DELETE SET NULL "
                                                           + "ON UPDATE NO ACTION ;";
                                        break;
                                    case Fieldtype.Group:
                                        alterEventTable += " , ADD CONSTRAINT "
                                                           + "fieldIdCons_" + id
                                                           + " FOREIGN KEY (field_" + id + ") REFERENCES groups (groupId) "
                                                           + "ON DELETE SET NULL "
                                                           + "ON UPDATE NO ACTION ;";
                                        break;
                                    case Fieldtype.File:
                                        alterEventTable += " , ADD CONSTRAINT "
                                                           + "fieldIdCons_" + id
                                                           + " FOREIGN KEY (field_" + id + ") REFERENCES files (fileId) "
                                                           + "ON DELETE SET NULL "
                                                           + "ON UPDATE NO ACTION ;";
                                        break;
                                    default: break;
                                }

                                altered = true;
                            }
                            else
                            {
                                //field MIGHT have been changed, since ID != -1 and ViewID != -1

                                //don't allow datatype to be changed
                                //unless we have a Text-field -> we have to update varCharLength
                                if (fdm.Datatype == Fieldtype.Text)
                                {
                                    // Get the old length
                                    cmd.CommandText = "SELECT varCharLength FROM eventtypefields WHERE fieldId = " + fdm.ID;
                                    int oldLength = Convert.ToInt32(cmd.ExecuteScalar());
                                    if (oldLength < fdm.VarcharLength)
                                    {
                                        // New length is greater. Update the table
                                        alterEventTable = "ALTER TABLE table_" + data.ID + " MODIFY COLUMN field_" + fdm.ID
                                                            + " varchar(" + fdm.VarcharLength + ")";
                                        altered = true;
                                    }
                                }

                                cmd.CommandText = updateField;
                                cmd.Parameters["@fname"].Value = fdm.Name;
                                cmd.Parameters["@fdescr"].Value = fdm.Description;
                                cmd.Parameters["@freqc"].Value = fdm.RequiredCreate;
                                cmd.Parameters["@freqa"].Value = fdm.RequiredApprove || fdm.RequiredCreate;
                                cmd.Parameters["@fvarchr"].Value = fdm.VarcharLength;
                                cmd.Parameters["@forder"].Value = fdm.ViewID;
                                cmd.Parameters["@fid"].Value = fdm.ID;
                                cmd.Prepare();
                                cmd.ExecuteNonQuery();
                            }

                            if (altered)
                            {
                                cmd.CommandText = alterEventTable;
                                cmd.Prepare();
                                cmd.ExecuteNonQuery();
                            }
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
                // database changes made it through ok, so delete appropiate files
                DeleteFilesFromServer(filesToDelete, true);
                return true;
            }
            else
            {
                ErrorMessage = "Could not open connection to database!";
                return false;
            }

        }

        /// <summary>
        /// Function for updating the active state of an event type
        /// </summary>
        /// <param name="eventTypeId">The ID of the event type to update</param>
        /// <param name="active">The new state (true or false)</param>
        /// <returns>True on success, otherwise false</returns>
        public bool SetEventTypeActive(int eventTypeId, bool active)
        {
            if (this.OpenConnection())
            {
                MySqlTransaction mst = null;
                MySqlCommand cmd = null;

                try
                {
                    mst = this.connection.BeginTransaction();
                    cmd = new MySqlCommand();
                    cmd.Connection = this.connection;
                    cmd.Transaction = mst;

                    cmd.CommandText = "UPDATE eventtypes SET active = @act WHERE eventTypeId = @id";
                    cmd.Parameters.AddWithValue("@act", active);
                    cmd.Parameters.AddWithValue("@id", eventTypeId);
                    cmd.Prepare();

                    cmd.ExecuteNonQuery();

                    mst.Commit();

                    this.CloseConnection();
                    return true;
                }
                catch (MySqlException ex1)
                {
                    try
                    {
                        mst.Rollback();
                        this.CloseConnection();
                        this.ErrorMessage = ex1.ErrorCode.ToString();
                        return false;
                    }
                    catch (MySqlException ex2)
                    {
                        this.CloseConnection();
                        this.ErrorMessage = ex2.ErrorCode.ToString();
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        public int EditEvent(EventEditModel eem)
        {
            if (this.OpenConnection() == true)
            {
                MySqlTransaction mst = null;
                MySqlCommand cmd = null;
                int newId = -1;
                List<string> filesToDelete = new List<string>();
                List<string> filesAdded = new List<string>();

                try
                {
                    mst = connection.BeginTransaction();
                    cmd = new MySqlCommand();
                    cmd.Connection = connection;
                    cmd.Transaction = mst;

                    string prevType = eem.SelectedEventType;
                    if (eem.ID != -1)
                    {
                        cmd.CommandText = "SELECT eventTypeId FROM events WHERE eventId = @eid";
                        cmd.Parameters.AddWithValue("@eid", eem.ID);
                        cmd.Prepare();
                        prevType = "" + Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    string updateEventTable = eem.ID == -1 ? (
                                                "INSERT INTO events" +
                                                "(userId,creation,eventTypeId,eventName,eventStart,eventEnd,visible,state) VALUES " +
                                                "( @ecreatorid , @creationdate , @eselectet , @ename , @estart , @eend , @evisible , @estate );"
                                                + "SELECT last_insert_id();") : (
                                                "UPDATE events SET eventTypeId = @eselectet , eventName = @ename , "
                                                + "eventStart = @estart , eventEnd = @eend , visible = @evisible , "
                                                + " state = @estate WHERE eventId = @eid;");

                    cmd.CommandText = updateEventTable;
                    cmd.Parameters.AddWithValue("@ecreatorid", eem.CreatorId);
                    if (eem.ID == -1) { cmd.Parameters.AddWithValue("@creationdate", DateTime.Now); } // Add current date if creation
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

                    // a parameter for fileIds for files that are left dangling after an edit
                    // will be needed later in the code
                    cmd.Parameters.AddWithValue("@dangle", null);

                    // Check if type has changed, clean up if it has...
                    bool changed = !String.IsNullOrEmpty(prevType) && !prevType.Equals(eem.SelectedEventType);
                    if (changed)
                    {
                        // The old type has a table with an entry, remove this entry.
                        string delete = "DELETE FROM table_" + prevType + " WHERE eventId = @nid";
                        cmd.CommandText = delete;
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();

                        // also delete any and all list values
                        cmd.CommandText = "DELETE FROM userlist WHERE eventId = @nid";
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "DELETE FROM grouplist WHERE eventId = @nid";
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "DELETE FROM stringlist WHERE eventId = @nid";
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                        
                        // delete files permanently if the checkbox said to
                        if (eem.DeleteFiles)
                        {
                            filesToDelete = this.DeleteFilesByEvent(newId, mst);
                        }
                        else
                        {
                            // remove eventId from dangling files
                            cmd.CommandText = "UPDATE files SET eventId = NULL WHERE eventId = @nid";
                            cmd.Prepare();
                            cmd.ExecuteNonQuery();
                        }
                        cmd.CommandText = "DELETE FROM filelist WHERE eventId = @nid";
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }

                    // Check if (new) type has a specifics table
                    if (eem.TypeSpecifics != null)
                    {
                        string updateTable;
                        if (eem.ID == -1 || changed)
                        {
                            // We have to insert because it is a create, or we have changed the type...
                            string prologue = "INSERT INTO table_" + eem.SelectedEventType + " ( eventId , ";
                            string epilogue = " VALUES ( @nid , ";

                            for (int i = 0; i < eem.TypeSpecifics.Count; i++)
                            {
                                FieldModel fm = eem.TypeSpecifics[i];
                                if (fm.Datatype == Fieldtype.File && fm.File.InputFile != null)
                                {
                                    // set file info and upload the file
                                    fm.File.EventID = newId;
                                    fm.File.UploaderID = CalendarApplication.Models.User.UserModel.GetCurrentUserID();
                                    fm.File.ID = this.UploadFile(fm.File, mst);
                                    filesAdded.Add("/App_Data/Files/" + fm.File.ID + "_" + fm.File.InputFile.FileName);
                                }
                                else if (fm.Datatype == Fieldtype.GroupList
                                        || fm.Datatype == Fieldtype.UserList)
                                {
                                    this.InsertListValues(newId, fm, mst);
                                }
                                else if (fm.Datatype == Fieldtype.TextList) { this.InsertStringValues(newId, fm, mst); }
                                else if (fm.Datatype == Fieldtype.FileList) 
                                { 
                                    List<List<string>> files = this.HandleFileList(newId, fm, mst, false);
                                    foreach(string file in files[0])
                                    {
                                        filesAdded.Add(file);
                                    }
                                }
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
                            // We have to update
                            updateTable = "UPDATE table_" + eem.SelectedEventType + " SET ";
                            for (int i = 0; i < eem.TypeSpecifics.Count; i++)
                            {
                                FieldModel fm = eem.TypeSpecifics[i];
                                if (fm.Datatype == Fieldtype.GroupList
                                        || fm.Datatype == Fieldtype.UserList)
                                {
                                    this.InsertListValues(newId, fm, mst);
                                }
                                // text lists should be updated, not inserted
                                else if (fm.Datatype == Fieldtype.TextList) { this.UpdateStringValues(newId, fm, mst); }
                                else if (fm.Datatype == Fieldtype.FileList)
                                { 
                                    List<List<string>> files = this.HandleFileList(newId, fm, mst, true);
                                    foreach(string file in files[0])
                                    {
                                        filesAdded.Add(file);
                                    }
                                    foreach(string file in files[1])
                                    {
                                        filesToDelete.Add(file);
                                    }
                                }
                                else 
                                if (fm.Datatype == Fieldtype.File)
                                {
                                    // old file exists?
                                    if (fm.File.ID > 0)
                                    {
                                        //if old file is inactive make appropriate changes, otherwise do nothing
                                        if (!fm.File.Active)
                                        {
                                            // old file should be deleted?
                                            if (fm.File.Delete)
                                            {
                                                string filepath = this.DeleteFileByID(fm.File.ID, mst);
                                                if (filepath != null) { filesToDelete.Add(filepath); }
                                                fm.File.ID = 0;
                                            }
                                            else
                                            {
                                                // set eventId to null at old file
                                                cmd.Parameters["@dangle"].Value = fm.File.ID;
                                                cmd.CommandText = "UPDATE files SET eventId = NULL WHERE fileId = @dangle";
                                                cmd.Prepare();
                                                cmd.ExecuteNonQuery();
                                                fm.File.ID = 0;
                                            }
                                            // new file was uploaded to replace?
                                            if (fm.File.InputFile != null)
                                            {
                                                // set file info and upload the file
                                                fm.File.EventID = newId;
                                                fm.File.UploaderID = CalendarApplication.Models.User.UserModel.GetCurrentUserID();
                                                fm.File.ID = this.UploadFile(fm.File, mst);
                                                filesAdded.Add("/App_Data/Files/" + fm.File.ID + "_" + fm.File.InputFile.FileName);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // new file exists?
                                        if (fm.File.InputFile != null)
                                        {
                                            // set file info and upload the file
                                            fm.File.EventID = newId;
                                            fm.File.UploaderID = CalendarApplication.Models.User.UserModel.GetCurrentUserID();
                                            fm.File.ID = this.UploadFile(fm.File, mst);
                                            filesAdded.Add("/App_Data/Files/" + fm.File.ID + "_" + fm.File.InputFile.FileName);
                                        }
                                    }
                                }
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
                    else
                    {
                        //when there are no specifics, just insert eventId when creating
                        if (eem.ID == -1)
                        {                            
                            string insert = "INSERT INTO table_" + eem.SelectedEventType + " ( eventId ) VALUES ( @nid )";
                            cmd.CommandText = insert;
                            cmd.Prepare();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    //Handle room edit/create here.
                    //Currently deletes everything and inserts again, even if unchanged.
                    //Could maybe be handled better.
                    string roomInsert = "INSERT INTO eventroomsused(eventId,roomId) VALUES ( @nid , @rmval );";
                    cmd.Parameters.AddWithValue("@rmval", null);
                    cmd.CommandText = "DELETE FROM eventroomsused WHERE eventId = @nid ;";
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
                    cmd.CommandText = "DELETE FROM eventeditorsusers WHERE eventId = @nid";
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "INSERT INTO eventeditorsusers(eventId,userId)"
                                                  + " VALUES ( @nid , @editusr );";
                    cmd.Parameters.AddWithValue("@editusr", null);
                    cmd.Prepare();
                    if (eem.UserEditorList == null) { eem.UserEditorList = new List<SelectListItem>(); }
                    foreach (SelectListItem edtusr in eem.UserEditorList)
                    {
                        if (edtusr.Selected)
                        {
                            cmd.Parameters["@editusr"].Value = edtusr.Value;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    cmd.CommandText = "DELETE FROM eventeditorsgroups WHERE eventId = @nid";
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "INSERT INTO eventeditorsgroups(eventId,groupId)"
                                                  + " VALUES ( @nid , @edtgrpid );";
                    cmd.Parameters.AddWithValue("@edtgrpid", null);
                    cmd.Prepare();
                    if (eem.GroupEditorList == null) { eem.GroupEditorList = new List<SelectListItem>(); }
                    foreach (SelectListItem edtgrp in eem.GroupEditorList)
                    {
                        if (edtgrp.Selected)
                        {
                            cmd.Parameters["@edtgrpid"].Value = edtgrp.Value;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Handle visibility, same way as above, except that we check if the event has global visibility
                    cmd.CommandText = "DELETE FROM eventvisibility WHERE eventId = @nid";
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                    if (!eem.Visible)
                    {
                        cmd.CommandText = "INSERT INTO eventvisibility(eventId,groupId)"
                                                      + " VALUES ( @nid , @visgrpid );";
                        cmd.Parameters.AddWithValue("@visgrpid", null);
                        cmd.Prepare();
                        if (eem.GroupVisibleList == null) { eem.GroupVisibleList = new List<SelectListItem>(); }
                        foreach (SelectListItem edtgrp in eem.GroupVisibleList)
                        {
                            if (edtgrp.Selected)
                            {
                                cmd.Parameters["@visgrpid"].Value = edtgrp.Value;
                                cmd.ExecuteNonQuery();
                            }
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
                        // if files were added, the rollback will have removed them from the database
                        // thus they should be deleted from the server as a precaution
                        // SQL error message takes priority
                        DeleteFilesFromServer(filesAdded, false);
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
                // database changes made it through ok, so delete appropiate files
                DeleteFilesFromServer(filesToDelete, true);
                return newId;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Function for inserting values into eventtypefield lists. Assumes the connection to be open
        /// and the transaction to be started
        /// </summary>
        /// <param name="eventId">Id of the current event</param>
        /// <param name="fm">FieldModel, datatype should be a user or group list</param>
        /// <param name="currentTrans">The current transaction</param>
        /// <returns>True on success, false if fm is not a list</returns>
        private bool InsertListValues(int eventId, FieldModel fm, MySqlTransaction currentTrans)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.Transaction = currentTrans;
            cmd.Connection = this.connection;
            string table = null;
            switch (fm.Datatype)
            {
                case Fieldtype.UserList: table = "user"; break;
                case Fieldtype.GroupList: table = "group"; break;
                default: return false;
            }
            cmd.CommandText = "DELETE FROM " + table + "list WHERE eventId = @eid AND fieldId = @fid";
            cmd.Parameters.AddWithValue("@eid", eventId);
            cmd.Parameters.AddWithValue("@fid", fm.ID);
            cmd.Parameters.AddWithValue("@item", null);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO " + table + "list (eventId,fieldId," + table + "Id)"
                                      + " VALUES (@eid,@fid,@item)";
            cmd.Prepare();

            foreach (SelectListItem item in fm.List)
            {
                if (item.Selected)
                {
                    cmd.Parameters["@item"].Value = item.Value;
                    cmd.ExecuteNonQuery();
                }
            }
            return true;
        }

        /// <summary>
        /// Handles file lists. Deletes, uploads and inserts into database.
        /// </summary>
        /// <param name="eventId"> id of event to create/edit</param>
        /// <param name="fm">field model of event</param>
        /// <param name="currentTrans">transaction in progress</param>
        /// <param name="edit"> true for an edit, false for a create</param>
        /// <returns>list of list of string. files added are in [0], files deleted are in [1] (when applicable)</returns>
        private List<List<string>> HandleFileList(int eventId, FieldModel fm, MySqlTransaction currentTrans, bool edit)
        {
            List<List<string>> result = new List<List<string>>();
            List<string> added = new List<string>();
            List<string> deleted = new List<string>();
            if (fm.FileList == null)
            {
                // list is empty, just return empty lists
                result.Add(added);
                result.Add(deleted);
                return result;
            }
            MySqlCommand cmd = new MySqlCommand();
            cmd.Transaction = currentTrans;
            cmd.Connection = this.connection;

            cmd.Parameters.AddWithValue("@eid", eventId);
            cmd.Parameters.AddWithValue("@field", fm.ID);
            cmd.Parameters.AddWithValue("@file", null);

            if (edit)
            {
                foreach (FileModel file in fm.FileList)
                {
                    // old file?
                    if (file.ID > 0)
                    {
                        //if old file is inactive make appropriate changes, otherwise do nothing
                        if (!file.Active)
                        {
                            // old file should be deleted?
                            if (file.Delete)
                            {
                                string filepath = this.DeleteFileByID(file.ID, currentTrans);
                                if (filepath != null) { deleted.Add(filepath); }
                            }
                            else
                            {
                                // set eventId to null at old file
                                cmd.Parameters["@file"].Value = file.ID;
                                cmd.CommandText = "UPDATE files SET eventId = NULL WHERE fileId = @file";
                                cmd.Prepare();
                                cmd.ExecuteNonQuery();
                            }
                            //remove from list in any case
                            cmd.Parameters["@file"].Value = file.ID;
                            cmd.CommandText = "DELETE FROM filelist WHERE fileId = @file AND eventId = @eid AND fieldId = @field";
                            cmd.Prepare();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // new file exists and is active?
                        if (file.Active && file.InputFile != null)
                        {
                            // set file info, upload file and add to list
                            file.EventID = eventId;
                            file.UploaderID = CalendarApplication.Models.User.UserModel.GetCurrentUserID();
                            int id = UploadFile(file, currentTrans);
                            added.Add("/App_Data/Files/" + id + "_" + file.InputFile.FileName);

                            cmd.CommandText = "INSERT INTO filelist(fileId,fieldId,eventId) VALUES ( @file , @field , @eid )";
                            cmd.Prepare();
                            cmd.Parameters["@file"].Value = id;
                            cmd.ExecuteNonQuery();
                        }
                    }


                }
            }
            else
            {
                // all files are new and should be added if they are active
                cmd.CommandText = "INSERT INTO filelist(fileId,fieldId,eventId) VALUES ( @file , @field , @eid )";
                cmd.Prepare();
                foreach (FileModel file in fm.FileList)
                {
                    if (file.Active && file.InputFile != null)
                    {
                        file.EventID = eventId;
                        file.UploaderID = CalendarApplication.Models.User.UserModel.GetCurrentUserID();
                        int id = UploadFile(file, currentTrans);
                        added.Add("/App_Data/Files/" + id + "_" + file.InputFile.FileName);
                        cmd.Parameters["@file"].Value = id;
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            // files added should be at result[0]
            result.Add(added);
            // files deleted should be at result[1]
            result.Add(deleted);
            return result;
        }

        /// <summary>
        /// Function for inserting values into stringlist table. Assumes the connection to be open
        /// and the transaction to be started
        /// </summary>
        /// <param name="eventId">Id of the current event</param>
        /// <param name="fm">FieldModel, datatype should be a stringlist</param>
        /// <param name="currentTrans">The current transaction</param>
        /// <returns>True on success, false if datatype is not a stringlist</returns>
        private bool InsertStringValues(int eventId, FieldModel fm, MySqlTransaction currentTrans)
        {
            if (fm.Datatype != Fieldtype.TextList) { return false; }
            //if list is null there were no values, so return
            if (fm.StringList == null) { return true; }

            MySqlCommand cmd = new MySqlCommand();
            cmd.Transaction = currentTrans;
            cmd.Connection = this.connection;
            cmd.Parameters.AddWithValue("@eid", eventId);
            cmd.Parameters.AddWithValue("@fid", fm.ID);
            cmd.Parameters.AddWithValue("@item", null);
            cmd.CommandText = "INSERT INTO stringlist (eventId,fieldId,text)"
                                      + " VALUES (@eid,@fid,@item)";
            cmd.Prepare();


            foreach (StringListModel item in fm.StringList)
            {
                if (!string.IsNullOrWhiteSpace(item.Text) || !item.Active)
                {
                    cmd.Parameters["@item"].Value = item.Text;
                    cmd.ExecuteNonQuery();
                }
            }
            return true;
        }

        /// <summary>
        /// Function for updating values in stringlist table. Assumes the connection to be open
        /// and the transaction to be started
        /// </summary>
        /// <param name="eventId">Id of the current event</param>
        /// <param name="fm">FieldModel, datatype should be a stringlist</param>
        /// <param name="currentTrans">The current transaction</param>
        /// <returns>True on success, false if datatype is not stringlist</returns>
        private bool UpdateStringValues(int eventId, FieldModel fm, MySqlTransaction currentTrans)
        {
            if (fm.Datatype != Fieldtype.TextList) { return false; }
            //if list is null there were no values, so return
            if (fm.StringList == null) { return true; }

            MySqlCommand cmd = new MySqlCommand();
            cmd.Transaction = currentTrans;
            cmd.Connection = this.connection;
            cmd.Parameters.AddWithValue("@eid", eventId);
            cmd.Parameters.AddWithValue("@fid", fm.ID);
            cmd.Parameters.AddWithValue("@strid", null);
            cmd.Parameters.AddWithValue("@item", null);


            foreach (StringListModel item in fm.StringList)
            {
                if (item.Active == true && !string.IsNullOrWhiteSpace(item.Text) )
                {
                    if (item.ID < 0)
                    {
                        // if string is new, insert
                        cmd.CommandText = "INSERT INTO stringlist (eventId,fieldId,text)"
                                      + " VALUES (@eid,@fid,@item)";
                        cmd.Prepare();
                        cmd.Parameters["@item"].Value = item.Text;
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        // if string is already there, update it
                        cmd.CommandText = "UPDATE stringlist SET text = @item WHERE stringListId = @strid";
                        cmd.Prepare();
                        cmd.Parameters["@strid"].Value = item.ID;
                        cmd.Parameters["@item"].Value = item.Text;
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    // delete string if removed, empty or whitespace
                    cmd.CommandText = "DELETE FROM stringlist WHERE stringListId = @strid";
                    cmd.Prepare();
                    cmd.Parameters["@strid"].Value = item.ID;
                    cmd.ExecuteNonQuery();
                }

            }
            return true;
        }

        /// <summary>
        /// Uploads file to server
        /// Private method, needs an ongoing transaction and object must have a connection in progress
        /// </summary>
        /// <param name="fm">File model for file</param>
        /// <param name="currentTrans">current transaction</param>
        /// <returns></returns>
        private int UploadFile(FileModel fm, MySqlTransaction currentTrans)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.Transaction = currentTrans;
            cmd.Connection = this.connection;
            cmd.CommandText = "INSERT INTO files(fileName,pathToFile,eventId,userId,uploaded)"
                              + " VALUES ( @fn , @ptf , @eid , @uid , @now );"
                              + "SELECT last_insert_id();";
            cmd.Parameters.AddWithValue("@fn", fm.InputFile.FileName);
            cmd.Parameters.AddWithValue("@ptf", "temp");
            cmd.Parameters.AddWithValue("@eid", fm.EventID);
            cmd.Parameters.AddWithValue("@uid", fm.UploaderID);
            cmd.Parameters.AddWithValue("@now", DateTime.Now);
            cmd.Prepare();

            int id = Convert.ToInt32(cmd.ExecuteScalar());

            // make sure filename is unique by prepending unique database id
            cmd.CommandText = "UPDATE files SET pathToFile = @ptf WHERE fileId = @fid";
            cmd.Parameters.AddWithValue("@fid",id);
            string dbpath = "/App_Data/Files/" + id + "_" + fm.InputFile.FileName;
            cmd.Parameters["@ptf"].Value = dbpath;
            cmd.Prepare();
            cmd.ExecuteNonQuery();
            // save file
            string truepath = HttpContext.Current.Server.MapPath("/App_Data/Files");
            truepath = Path.Combine(truepath, "" + id + "_" + fm.InputFile.FileName);
            fm.InputFile.SaveAs(truepath);

            return id;
        }

        /// <summary>
        /// Method for setting state for a given event
        /// </summary>
        /// <param name="eventId">ID of the event</param>
        /// <param name="newState">The new state, state must be in [0,2]</param>
        /// <returns>True on success, false on error</returns>
        public bool SetEventState(int eventId, int newState)
        {
            // Sanity check of states
            if (newState < 0 || newState > 2) { return false; }

            if (this.OpenConnection())
            {
                MySqlTransaction mst = null;
                MySqlCommand cmd = new MySqlCommand();
                try
                {
                    // Try to update the state
                    mst = this.connection.BeginTransaction();
                    cmd.Connection = this.connection;
                    cmd.CommandText = "UPDATE events SET state = @state WHERE eventId = @eid";
                    cmd.Parameters.AddWithValue("@state", newState);
                    cmd.Parameters.AddWithValue("@eid", eventId);
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                    mst.Commit();

                    this.CloseConnection();
                    return true;
                }
                catch (MySqlException ex)
                {
                    // Error
                    this.ErrorMessage = ex.Message + " caused by: " + cmd.CommandText;
                    mst.Rollback();
                    this.CloseConnection();
                    return false;
                }
            }
            else
            {
                // Error
                return false;
            }
        }


        /// <summary>
        /// Delete existing event
        /// </summary>
        /// <param name="id">event id</param>
        /// <returns>bool indicating success or failure</returns>
        public bool DeleteEvent(int id)
        {
            if (this.OpenConnection())
            {
                MySqlTransaction mst = null;
                MySqlCommand cmd = null;
                cmd = new MySqlCommand();

                try
                {
                    mst = connection.BeginTransaction();
                    cmd.Connection = connection;
                    cmd.Transaction = mst;

                    cmd.CommandText = "DELETE FROM events WHERE eventId = @evid";
                    cmd.Parameters.AddWithValue("@evid", id);
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    mst.Commit();

                    this.CloseConnection();
                    return true;
                }
                catch (MySqlException ex)
                {
                    this.ErrorMessage = ex.Message + " caused by: " + cmd.CommandText;
                    mst.Rollback();
                    this.CloseConnection();
                    return false;
                }
            }
            else
            {
                //could not open connection
                return false;
            }
        }

        /// <summary>
        /// Delete all files tied to an event from database
        /// Private method, needs an ongoing transaction and object must have a connection in progress
        /// </summary>
        /// <param name="id">event id</param>
        /// <param name="mst">transaction in progress</param>
        /// <returns>string list of path to all files tied to event</returns>
        private List<string> DeleteFilesByEvent(int id, MySqlTransaction mst)
        {
            MySqlCommand cmd = null;
            cmd = new MySqlCommand();
            cmd.Transaction = mst;
            cmd.Connection = this.connection;
            System.Data.DataTable dt = new System.Data.DataTable();
            cmd.CommandText = "SELECT pathToFile FROM files WHERE eventId = @evid";
            cmd.Parameters.AddWithValue("@evid", id);
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
                System.Data.DataRow dr = dt.NewRow();
                for (int i = 0; i < cols; i++)
                {
                    dr[i] = dataReader[i];
                }
                dt.Rows.Add(dr);
            }

            //Close the connection
            dataReader.Close();

            cmd.CommandText = "DELETE FROM files WHERE eventId = @evid";
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            List<string> result = new List<string>();

            foreach (System.Data.DataRow row in dt.Rows)
            {
                result.Add((string)row["pathToFile"]);
            }
            return result;
        }

        /// <summary>
        /// Delete all files tied to an event
        /// </summary>
        /// <param name="id">event id</param>
        /// <returns>bool indicating success or failure</returns>
        public bool DeleteFilesByEvent(int id)
        {
            if (this.OpenConnection())
            {
                MySqlTransaction mst = null;
                MySqlCommand cmd = null;
                cmd = new MySqlCommand();
                System.Data.DataTable dt = new System.Data.DataTable();

                try
                {
                    mst = connection.BeginTransaction();
                    cmd.Connection = connection;
                    cmd.Transaction = mst;

                    cmd.CommandText = "SELECT pathToFile FROM files WHERE eventId = @evid";
                    cmd.Parameters.AddWithValue("@evid", id);
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
                        System.Data.DataRow dr = dt.NewRow();
                        for (int i = 0; i < cols; i++)
                        {
                            dr[i] = dataReader[i];
                        }
                        dt.Rows.Add(dr);
                    }

                    //Close the connection
                    dataReader.Close();

                    cmd.CommandText = "DELETE FROM files WHERE eventId = @evid";
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    mst.Commit();
                    this.CloseConnection();

                }
                catch (MySqlException ex)
                {
                    this.ErrorMessage = ex.Message + " caused by: " + cmd.CommandText;
                    mst.Rollback();
                    this.CloseConnection();
                    return false;
                }

                List<string> files = new List<string>();
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    files.Add((string)row["pathToFile"]);
                } 
                return DeleteFilesFromServer(files,true);
            }
            else
            {
                //could not open connection
                return false;
            }
        }

        /// <summary>
        /// Delete file from database by fileId
        /// Private method, needs a transaction and object must have a connection in progress
        /// </summary>
        /// <param name="id">file id</param>
        /// <param name="mst">transaction in progress</param>
        /// <returns>path to file as string, empty string on error</returns>
        private string DeleteFileByID(int id, MySqlTransaction mst)
        {
            string result = null;
            MySqlCommand cmd = null;
            cmd = new MySqlCommand();
            cmd.Transaction = mst;
            cmd.Connection = this.connection;
            System.Data.DataTable dt = new System.Data.DataTable();
            cmd.CommandText = "SELECT pathToFile FROM files WHERE fileId = @fid";
            cmd.Parameters.AddWithValue("@fid", id);
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
                System.Data.DataRow dr = dt.NewRow();
                for (int i = 0; i < cols; i++)
                {
                    dr[i] = dataReader[i];
                }
                dt.Rows.Add(dr);
            }

            //Close the connection
            dataReader.Close();

            cmd.CommandText = "DELETE FROM files WHERE fileId = @fid";
            cmd.Prepare();
            cmd.ExecuteNonQuery();
            if (dt.Rows.Count > 0)
            {
                result = (dt.Rows[0]["pathToFile"] is DBNull ? null : (string)dt.Rows[0]["pathToFile"]);
            }
            return result;
            
        }

        /// <summary>
        /// Delete a file by its ID
        /// </summary>
        /// <param name="id">file id</param>
        /// <returns>bool indicating success or failure</returns>
        public bool DeleteFileByID(int id)
        {
            if (this.OpenConnection())
            {
                MySqlTransaction mst = null;
                MySqlCommand cmd = null;
                cmd = new MySqlCommand();
                System.Data.DataTable dt = new System.Data.DataTable();

                try
                {
                    cmd.CommandText = "SELECT pathToFile FROM files WHERE fileId = @fid";
                    cmd.Parameters.AddWithValue("@fid", id);
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
                        System.Data.DataRow dr = dt.NewRow();
                        for (int i = 0; i < cols; i++)
                        {
                            dr[i] = dataReader[i];
                        }
                        dt.Rows.Add(dr);
                    }

                    //Close the connection
                    dataReader.Close();

                    cmd.CommandText = "DELETE FROM files WHERE fileId = @fid";
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    mst.Commit();
                    this.CloseConnection();
                }
                catch (MySqlException ex)
                {
                    this.ErrorMessage = ex.Message + " caused by: " + cmd.CommandText;
                    mst.Rollback();
                    this.CloseConnection();
                    return false;
                }


                if (dt.Rows.Count > 0 && !(dt.Rows[0]["pathToFile"] is DBNull))
                {
                    List<string> file = new List<string>();
                    file.Add((string)dt.Rows[0]["pathToFile"]);
                    return DeleteFilesFromServer(file, true);
                }
                this.ErrorMessage = "File not found in database.";
                return false;
            }
            else
            {
                //could not open connection
                return false;
            }
        }

        /// <summary>
        /// Handles files left over when a file field is removed from an event type
        /// If delete is set, all files will be deleted permanently
        /// Otherwise eventId is set to null at all files pertaining to file field
        /// </summary>
        /// <param name="eventTypeId"> id of event type containing the field</param>
        /// <param name="fieldId"> field id </param>
        /// <param name="currentTrans"> transaction in progress </param>
        /// <param name="delete"> whether files should be deleted </param>
        /// <returns> A list of path to files that were deleted, empty list when delete == false</returns>
        private List<string> HandleDanglingFiles(int eventTypeId, int fieldId, MySqlTransaction currentTrans, bool delete)
        {
            List<string> result = new List<string>();
            MySqlCommand cmd = null;
            cmd = new MySqlCommand();
            cmd.Transaction = currentTrans;
            cmd.Connection = this.connection;
            string field = "field_" + fieldId;
            cmd.CommandText = "SELECT DISTINCT(" + field + ") FROM table_" + eventTypeId;
            cmd.Parameters.AddWithValue("@file", null);
            cmd.Prepare();

            System.Data.DataTable dt = new System.Data.DataTable();
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
                System.Data.DataRow dr = dt.NewRow();
                for (int i = 0; i < cols; i++)
                {
                    dr[i] = dataReader[i];
                }
                dt.Rows.Add(dr);
            }

            //Close reader
            dataReader.Close();

            if (delete)
            {
                foreach (System.Data.DataRow dr in dt.Rows)
                {
                    if (!(dr[field] is DBNull))
                    {
                        int id = (int)dr[field];
                        string path = DeleteFileByID(id, currentTrans);
                        if (path != null) { result.Add(path); }
                    }
                }
            }
            else
            {
                cmd.CommandText = "UPDATE files SET eventId = NULL WHERE fileId = @file";
                cmd.Prepare();
                foreach (System.Data.DataRow dr in dt.Rows)
                {
                    if (!(dr[field] is DBNull))
                    {
                        cmd.Parameters["@file"].Value = (int)dr[field];
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Handles files left over when a file list field is removed from an event type
        /// If delete is set, all files will be deleted permanently
        /// Otherwise eventId is set to null at all files pertaining to file list field
        /// </summary>
        /// <param name="fieldId"> field id </param>
        /// <param name="currentTrans"> transaction in progress </param>
        /// <param name="delete"> whether files should be deleted </param>
        /// <returns> A list of path to files that were deleted, empty list when delete == false</returns>
        private List<string> HandleDanglingFileList(int fieldId, MySqlTransaction currentTrans, bool delete)
        {
            List<string> result = new List<string>();
            MySqlCommand cmd = null;
            cmd = new MySqlCommand();
            cmd.Transaction = currentTrans;
            cmd.Connection = this.connection;

            cmd.CommandText = "SELECT fileId FROM filelist WHERE fieldId = @field";
            cmd.Parameters.AddWithValue("@field", fieldId);
            cmd.Parameters.AddWithValue("@file", null);
            cmd.Prepare();

            System.Data.DataTable dt = new System.Data.DataTable();
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
                System.Data.DataRow dr = dt.NewRow();
                for (int i = 0; i < cols; i++)
                {
                    dr[i] = dataReader[i];
                }
                dt.Rows.Add(dr);
            }

            // Close reader
            dataReader.Close();

            if (delete)
            {
                foreach (System.Data.DataRow dr in dt.Rows)
                {
                    if (!(dr["fileId"] is DBNull))
                    {
                        int id = (int)dr["fileId"];
                        string path = DeleteFileByID(id, currentTrans);
                        if (path != null) { result.Add(path); }
                    }
                }
            }
            else
            {
                cmd.CommandText = "UPDATE files SET eventId = NULL WHERE fileId = @file";
                cmd.Prepare();
                foreach (System.Data.DataRow dr in dt.Rows)
                {
                    if (!(dr["fileId"] is DBNull))
                    {
                        cmd.Parameters["@file"].Value = (int)dr["fileId"];
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            return result;

        }

        /// <summary>
        /// Private method to delete files from server
        /// </summary>
        /// <param name="files">a string list of files to delete</param>
        /// <param name="errormsg">whether this.ErrorMessage should be set on error</param>
        /// <returns> true on success, sets up an error message in App_Data/Errors on error</returns>
        private bool DeleteFilesFromServer(List<string> files, bool viewError)
        {
            List<string> undeleted = new List<string>();
            List<Exception> exceptions = new List<Exception>();
            foreach (string file in files)
            {
                try
                {
                    File.Delete(HttpContext.Current.Server.MapPath(file));
                }
                catch (Exception ex)
                {
                    undeleted.Add(file);
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                string errorName = "FileDeletionError" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt";
                string path = HttpContext.Current.Server.MapPath("/App_Data/Errors");
                path = Path.Combine(path, errorName);
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine("The following files were deleted from database but not file system. Please delete manually:");
                    foreach (string filePath in undeleted)
                    {
                        sw.WriteLine(filePath);
                    }
                    sw.WriteLine();
                    sw.WriteLine("Full error messages:");
                    sw.WriteLine();
                    foreach (Exception ex in exceptions)
                    {
                        sw.WriteLine(ex.ToString());
                        sw.WriteLine();
                    }
                }
                if (viewError)
                {
                    this.ErrorMessage = "Some files could not be deleted. Consult the file " + errorName + " in the error logger";
                }
                return false;
            }
            else { return true; }
        }
    }
}