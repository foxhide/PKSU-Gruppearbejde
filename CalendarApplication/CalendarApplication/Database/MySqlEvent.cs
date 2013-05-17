using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

using CalendarApplication.Models.Event;
using CalendarApplication.Models.EventType;

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
                                             + "(eventTypeId, fieldName, fieldDescription, requiredCreation, requiredApproval, fieldType, varCharLength) "
                                             + "VALUES (@typeid , @fieldname , @descr , @reqc , @reqa , @datatype , @varch ); "
                                             + "SELECT last_insert_id();";

                        cmd.Parameters.AddWithValue("@typeid", result);
                        cmd.Parameters.AddWithValue("@fieldname", null);
                        cmd.Parameters.AddWithValue("@descr", null);
                        cmd.Parameters.AddWithValue("@reqc", null);
                        cmd.Parameters.AddWithValue("@reqa", null);
                        cmd.Parameters.AddWithValue("@datatype", null);
                        cmd.Parameters.AddWithValue("@varch", null);
                        cmd.CommandText = insertField;

                        foreach (FieldDataModel fdm in data.TypeSpecific)
                        {
                            cmd.Parameters["@fieldname"].Value = fdm.Name;
                            cmd.Parameters["@descr"].Value = fdm.Description;
                            cmd.Parameters["@reqc"].Value = fdm.RequiredCreate;
                            cmd.Parameters["@reqa"].Value = fdm.RequiredApprove;
                            cmd.Parameters["@datatype"].Value = fdm.GetTypeAsInt();
                            cmd.Parameters["@varch"].Value = fdm.VarcharLength;

                            cmd.Prepare();

                            int id = Convert.ToInt32(cmd.ExecuteScalar());

                            createTable += "field_" + id + " " + fdm.GetDBType() + ", ";
                            switch (fdm.Datatype)
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
                        cmd.Parameters.AddWithValue("@freqc", null);
                        cmd.Parameters.AddWithValue("@freqa", null);
                        cmd.Parameters.AddWithValue("@fdattyp", null);
                        cmd.Parameters.AddWithValue("@fvarchr", null);

                        string updateField = "UPDATE pksudb.eventtypefields SET fieldName = @fname , fieldDescription = @fdescr"
                                             + " , requiredCreation = @freqc , requiredApproval = @freqa , varCharLength = @fvarchr WHERE eventTypeId = @etid"
                                             + " AND fieldId = @fid;";

                        string insertField = "INSERT INTO pksudb.eventtypefields"
                                             + " (eventTypeId, fieldName, fieldDescription, requiredCreation, requiredApproval , fieldType, varCharLength)"
                                             + " VALUES ( @etid , @fname , @fdescr , @freqc , @freqa , @fdattyp , @fvarchr);"
                                             + "SELECT last_insert_id();";

                        string removeField = "DELETE FROM pksudb.eventtypefields WHERE fieldId = @fid";

                        foreach (FieldDataModel fdm in data.TypeSpecific)
                        {
                            string alterEventTable = "";
                            bool altered = false;
                            if (fdm.ViewID == -1)
                            {
                                //field was removed
                                //remove foreign keys before dropping column if necessary

                                //since the fdm is just a dummy when deleting, and thus has no relevant Datatype, this does not work.
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
                            else if (fdm.ViewID == -2)
                            {
                                //new field was added
                                cmd.CommandText = insertField;
                                cmd.Parameters["@fname"].Value = fdm.Name;
                                cmd.Parameters["@fdescr"].Value = fdm.Description;
                                cmd.Parameters["@freqc"].Value = fdm.RequiredCreate;
                                cmd.Parameters["@freqa"].Value = fdm.RequiredApprove;
                                cmd.Parameters["@fdattyp"].Value = fdm.GetTypeAsInt();
                                cmd.Parameters["@fvarchr"].Value = fdm.VarcharLength;
                                cmd.Prepare();
                                int id = Convert.ToInt32(cmd.ExecuteScalar());

                                alterEventTable = "ALTER TABLE table_" + data.ID + " ADD COLUMN field_" + id + " " + fdm.GetDBType();

                                //add foreign keys after adding column if necessary
                                switch (fdm.Datatype)
                                {
                                    case Fieldtype.User:
                                        alterEventTable += " , ADD CONSTRAINT "
                                                           + "fieldIdCons_" + id
                                                           + " FOREIGN KEY (field_" + id + ") REFERENCES pksudb.users (userId) "
                                                           + "ON DELETE SET NULL "
                                                           + "ON UPDATE NO ACTION ;";
                                        break;
                                    case Fieldtype.Group:
                                        alterEventTable += " , ADD CONSTRAINT "
                                                           + "fieldIdCons_" + id
                                                           + " FOREIGN KEY (field_" + id + ") REFERENCES pksudb.groups (groupId) "
                                                           + "ON DELETE SET NULL "
                                                           + "ON UPDATE NO ACTION ;";
                                        break;
                                    case Fieldtype.File:
                                        alterEventTable += " , ADD CONSTRAINT "
                                                           + "fieldIdCons_" + id
                                                           + " FOREIGN KEY (field_" + id + ") REFERENCES pksudb.files (fileId) "
                                                           + "ON DELETE SET NULL "
                                                           + "ON UPDATE NO ACTION ;";
                                        break;
                                    default: break;
                                }

                                altered = true;
                            }
                            else
                            {
                                //field MIGHT have been changed
                                //don't allow datatype to be changed
                                //alterEventTable = "ALTER TABLE table_" + data.ID + " MODIFY COLUMN field_" + fdm.ID + " " + fdm.GetDBType();

                                cmd.CommandText = updateField;
                                cmd.Parameters["@fname"].Value = fdm.Name;
                                cmd.Parameters["@fdescr"].Value = fdm.Description;
                                cmd.Parameters["@freqc"].Value = fdm.RequiredCreate;
                                cmd.Parameters["@freqa"].Value = fdm.RequiredApprove;
                                cmd.Parameters["@fvarchr"].Value = fdm.VarcharLength;
                                //cmd.Parameters["@fdattyp"].Value = fdm.GetTypeAsInt();
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
                return true;
            }
            else
            {
                ErrorMessage = "Could not open connection to database!";
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

                try
                {
                    mst = connection.BeginTransaction();
                    cmd = new MySqlCommand();
                    cmd.Connection = connection;
                    cmd.Transaction = mst;

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
                        // The old type has a table with an entry, remove this entry.
                        string delete = "DELETE FROM table_" + prevType + " WHERE eventId = @nid";
                        cmd.CommandText = delete;
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
                            string prologue = "INSERT INTO pksudb.table_" + eem.SelectedEventType + " ( eventId , ";
                            string epilogue = " VALUES ( @nid , ";

                            for (int i = 0; i < eem.TypeSpecifics.Count; i++)
                            {
                                FieldModel fm = eem.TypeSpecifics[i];
                                if (fm.Datatype == Fieldtype.GroupList
                                        || fm.Datatype == Fieldtype.UserList
                                        || fm.Datatype == Fieldtype.FileList)
                                {
                                    this.InsertListValues(newId, fm, mst);
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
                            // We only have to update
                            updateTable = "UPDATE pksudb.table_" + eem.SelectedEventType + " SET ";
                            for (int i = 0; i < eem.TypeSpecifics.Count; i++)
                            {
                                FieldModel fm = eem.TypeSpecifics[i];
                                if (fm.Datatype == Fieldtype.GroupList
                                        || fm.Datatype == Fieldtype.UserList
                                        || fm.Datatype == Fieldtype.FileList)
                                {
                                    this.InsertListValues(newId, fm, mst);
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
                            cmd.Parameters["@edtgrpid"].Value = edtgrp.Value;
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
                                                      + " VALUES ( @nid , @visgrpid );";
                        cmd.Parameters.AddWithValue("@visgrpid", null);
                        cmd.Prepare();
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
        /// <param name="fm">FieldModel, datatype should be a list</param>
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
                case Fieldtype.FileList: table = "file"; break;
                case Fieldtype.UserList: table = "user"; break;
                case Fieldtype.GroupList: table = "group"; break;
                default: return false;
            }
            cmd.CommandText = "DELETE FROM pksudb." + table + "list WHERE eventId = @eid AND fieldId = @fid";
            cmd.Parameters.AddWithValue("@eid", eventId);
            cmd.Parameters.AddWithValue("@fid", fm.ID);
            cmd.Parameters.AddWithValue("@item", null);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO pksudb." + table + "list (eventId,fieldId," + table + "Id)"
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
    }
}