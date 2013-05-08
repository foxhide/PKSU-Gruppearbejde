using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Windows.Forms;

using CalendarApplication.Models.User;
using CalendarApplication.Models.Event;
using CalendarApplication.Models.EventType;
using CalendarApplication.Database;

namespace CalendarApplication.Controllers
{
    public class EventController : Controller
    {
        //
        // GET: /Event/

        public ActionResult Index(int id)
        {
            EventWithDetails result = new EventWithDetails
            {
                ID = id
            };
            string eventinfo = "SELECT * FROM pksudb.events NATURAL JOIN pksudb.eventroomsused " +
                               "NATURAL JOIN pksudb.rooms NATURAL JOIN pksudb.eventtypes NATURAL JOIN pksudb.users " +
                               "WHERE eventId = " + id;
            MySqlConnect con = new MySqlConnect();
            DataTable table = con.ExecuteQuery(eventinfo);

            if (table != null)
            {
                DataRowCollection rows = table.Rows;
                result.Name = (string)rows[0]["eventName"];
                result.Start = (DateTime)rows[0]["eventStart"];
                result.End = (DateTime)rows[0]["eventEnd"];
                result.State = (int)rows[0]["state"];
                result.TypeId = (int)rows[0]["eventTypeId"];
                result.TypeName = (string)rows[0]["eventTypeName"];
                result.Visible = (bool)rows[0]["visible"];
                result.Creator = (string)rows[0]["userName"];
                result.CreatorId = (int)rows[0]["userId"];
                result.Rooms = new List<Room>();                       
                for (int i = 0; i < rows.Count; i++)
                {
                    result.Rooms.Add(new Room
                    {
                        ID = (int)rows[i]["roomId"],
                        Name = (string)rows[i]["roomName"]
                    });
                }
                CustomQuery query0 = new CustomQuery();
                query0.Cmd = "SELECT * FROM table_" + result.TypeId + " WHERE eventId = @eventId";
                query0.ArgNames = new string[] { "@eventId" };
                query0.Args = new object[] { id };
                CustomQuery query1 = new CustomQuery();
                query1.Cmd = "SELECT * FROM pksudb.eventtypefields WHERE eventTypeId = @eventTypeId";
                query1.ArgNames = new string[] { "@eventTypeId" };
                query1.Args = new object[] { result.TypeId };
                DataSet ds = con.ExecuteQuery(new CustomQuery[] { query0, query1 });

                result.TypeSpecifics = new List<FieldModel>();
                if (ds != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        FieldModel fm = new FieldModel
                        {
                            ID = (int)ds.Tables[1].Rows[i]["fieldId"],
                            Name = (string)ds.Tables[1].Rows[i]["fieldName"],
                            Datatype = (Fieldtype)ds.Tables[1].Rows[i]["fieldType"]
                        };
                        switch(fm.Datatype)
                        {
                            case Fieldtype.Float: fm.FloatValue = (float)ds.Tables[0].Rows[0]["field_" + fm.ID]; break;

                            case Fieldtype.File: if (DBNull.Value.Equals(ds.Tables[0].Rows[0]["field_" + fm.ID])) { fm.IntValue = 0; }
                                                 else { fm.IntValue = (int)ds.Tables[0].Rows[0]["field_" + fm.ID]; }
                                                 fm.StringValue = "File system not yet implemented."; break;

                            case Fieldtype.Text: fm.StringValue = ds.Tables[0].Rows[0]["field_" + fm.ID] as string; break;

                            case Fieldtype.Bool: fm.BoolValue = (bool)ds.Tables[0].Rows[0]["field_" + fm.ID]; break;

                            case Fieldtype.Datetime: fm.DateValue = (DateTime)ds.Tables[0].Rows[0]["field_" + fm.ID]; break;

                            case Fieldtype.User: if (DBNull.Value.Equals(ds.Tables[0].Rows[0]["field_" + fm.ID]))
                                                 {
                                                    fm.IntValue = 0;
                                                    fm.StringValue = "None";
                                                 }
                                                 else
                                                 {
                                                    fm.IntValue = (int)ds.Tables[0].Rows[0]["field_" + fm.ID]; 
                                                    fm.StringValue = UserModel.GetUser(fm.IntValue).UserName;
                                                 }
                                                 break;

                            case Fieldtype.Group: if (DBNull.Value.Equals(ds.Tables[0].Rows[0]["field_" + fm.ID]))
                                                  {
                                                        fm.IntValue = 0;
                                                        fm.StringValue = "None";
                                                  }
                                                  else
                                                  {
                                                        fm.IntValue = (int)ds.Tables[0].Rows[0]["field_" + fm.ID];
                                                        fm.StringValue = "Group name not implemented until Andreas has made a GetGroup function...";
                                                  }
                                                  break;
                        }
                        result.TypeSpecifics.Add(fm);
                    }
                }
            }
            else
            {
                TempData["errorMsg"] = con.ErrorMessage;
                result.ID = -1;
            }
            return View(result);
        }

        public ActionResult EditEvent(int id, int year, int month, int day)
        {
            EventEditModel eem = null;
            MySqlConnect msc = new MySqlConnect();
            DataTable dt = null;
            if (id == -1)
            {
                eem = new EventEditModel
                {
                    ID = id,
                    EventTypes = new List<SelectListItem>(),
                    SelectedEventType = "0", // Initial value -> Basic event
                    Start = new DateTime(year, month, day, 10, 0, 0),
                    End = new DateTime(year, month, day, 18, 0, 0),
                    Visible = true
                };
            }
            else
            {
                CustomQuery query = new CustomQuery
                {
                    Cmd = "SELECT userId,eventTypeId,eventName,eventStart,eventEnd,visible,state FROM pksudb.events WHERE eventId = @eid",
                    ArgNames = new[] { "@eid" },
                    Args = new[] { (object)id }
                };
                dt = msc.ExecuteQuery(query);
                if (dt != null)
                {
                    eem = new EventEditModel
                    {
                        ID = id,
                        Name = (string)dt.Rows[0]["eventName"],
                        CreatorId = (int)dt.Rows[0]["userId"],
                        EventTypes = new List<SelectListItem>(),
                        SelectedEventType = ((int)dt.Rows[0]["eventTypeId"]).ToString(),
                        Start = (DateTime)dt.Rows[0]["eventStart"],
                        End = (DateTime)dt.Rows[0]["eventEnd"],
                        Visible = (bool)dt.Rows[0]["visible"],
                        State = (int)dt.Rows[0]["state"]
                    };
                }
            }
            // Get list of rooms //
            eem.RoomSelectList = new List<SelectListItem>();
            string roomquery = eem.ID == -1 ? "SELECT roomId,roomName FROM pksudb.rooms"
                               : "SELECT roomId,roomName,eventId FROM pksudb.rooms NATURAL LEFT JOIN "
                                    + "(SELECT * FROM pksudb.eventroomsused WHERE eventId = " + eem.ID + ") AS r";
            dt = msc.ExecuteQuery(roomquery);
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    SelectListItem room = new SelectListItem
                    {
                        Value = ((int)dr["roomId"]).ToString(),
                        Text = (string)dr["roomName"]
                    };
                    if (eem.ID != -1)
                    {
                        room.Selected = !(dr["eventId"] is DBNull);
                    }
                    eem.RoomSelectList.Add(room);
                }
            }

            // Get list of users
            eem.UserEditorList = new List<SelectListItem>();
            string userquery = eem.ID == -1 ? "SELECT userId,userName FROM pksudb.users ORDER BY userName"
                               : "SELECT userId,userName,eventId FROM pksudb.users NATURAL LEFT JOIN "
                                + "(SELECT * FROM pksudb.eventeditorsusers WHERE eventId = " + eem.ID + ") AS e ORDER BY userName";
            dt = msc.ExecuteQuery(userquery);
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    SelectListItem user = new SelectListItem
                    {
                        Value = ((int)dr["userId"]).ToString(),
                        Text = (string)dr["userName"]
                    };
                    if (eem.ID != -1) { user.Selected = !(dr["eventId"] is DBNull); }
                    eem.UserEditorList.Add(user);
                }
            }

            // Get list of groups
            eem.GroupEditorList = new List<SelectListItem>();
            eem.GroupVisibleList = new List<SelectListItem>();
            if (eem.ID == -1)
            {
                string groupquery = "SELECT groupId,groupName FROM pksudb.groups ORDER BY groupName";
                dt = msc.ExecuteQuery(groupquery);
                if (dt != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        eem.GroupEditorList.Add(new SelectListItem
                        {
                            Value = ((int)dr["groupId"]).ToString(),
                            Text = (string)dr["groupName"]
                        });
                    }
                }
                eem.GroupVisibleList = eem.GroupEditorList;
            }
            else
            {
                string query0 = "SELECT groupId,groupName,eventId FROM pksudb.groups NATURAL LEFT JOIN "
                                    + "(SELECT * FROM pksudb.eventeditorsgroups WHERE eventId = " + eem.ID + ") AS g ORDER BY groupName";
                string query1 = "SELECT groupId,groupName,eventId FROM pksudb.groups NATURAL LEFT JOIN "
                                    + "(SELECT * FROM pksudb.eventvisibility WHERE eventId = " + eem.ID + ") AS g ORDER BY groupName";
                DataSet ds = msc.ExecuteQuery(new[] { query0, query1 });
                if (ds != null)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        eem.GroupEditorList.Add(new SelectListItem
                        {
                            Value = ((int)dr["groupId"]).ToString(),
                            Text = (string)dr["groupName"],
                            Selected = !(dr["eventId"] is DBNull)
                        });
                    }
                    foreach (DataRow dr in ds.Tables[1].Rows)
                    {
                        eem.GroupVisibleList.Add(new SelectListItem
                        {
                            Value = ((int)dr["groupId"]).ToString(),
                            Text = (string)dr["groupName"],
                            Selected = !(dr["eventId"] is DBNull)
                        });
                    }
                }
            }

            eem.EventTypes = this.GetEventTypes(msc);
            TempData["errorMsg"] = msc.ErrorMessage;
            if (id != -1)
            {
                eem.TypeSpecifics = this.GetTypeSpecifics(id, int.Parse(eem.SelectedEventType), msc);
            }
            
            return View(eem);
        }

        [HttpPost]
        public ActionResult EditEvent(EventEditModel eem)
        {
            MySqlEvent mse = new MySqlEvent();
            eem.CreatorId = UserModel.GetCurrentUserID();
            int id = mse.EditEvent(eem);
            if (id > 0)
            {
                return RedirectToAction("Index", "Event", new { id = id });
            }
            else {
                TempData["errorMsg"] = mse.ErrorMessage;
                eem.EventTypes = this.GetEventTypes(mse);
                return View(eem);
            }
        }

        /// <summary>
        /// Gets a partial view with the event specific fields. Called from js-AJAX
        /// </summary>
        /// <param name="type">The event type. Should always be > 0</param>
        /// <returns>A partial view containing a list of the (empty) specifics for the event type</returns>
        public ActionResult GetEventSpecific(int type)
        {
            EventEditModel eem = new EventEditModel { TypeSpecifics = new List<FieldModel>() };

            MySqlConnect msc = new MySqlConnect();
            eem.TypeSpecifics = this.GetTypeSpecifics(-1, type, msc);

            return PartialView("EventSpecificList", eem);
        }

        /// <summary>
        /// Get a list of the type specifics for a given event. If the event has id != -1, the values will be fetched too.
        /// </summary>
        /// <param name="id">Id of event, -1 for empty fields</param>
        /// <param name="type">Type id of event</param>
        /// <param name="msc">MySqlConnect</param>
        /// <returns>A list of FieldModels</returns>
        public List<FieldModel> GetTypeSpecifics(int id, int type, MySqlConnect msc)
        {
            List<FieldModel> result = new List<FieldModel>();
            List<SelectListItem> users = null;       // List for user list
            List<SelectListItem> groups = null;      // List for group list
            List<SelectListItem> usersDrop = null;   // List for user dropdown
            List<SelectListItem> groupsDrop = null;  // List for group dropdown
            string specQuery = "SELECT * FROM eventtypefields WHERE eventTypeId = " + type;
            DataTable dt = msc.ExecuteQuery(specQuery);
            DataTable value = null;
            if (id != -1)
            {
                value = msc.ExecuteQuery("SELECT * FROM table_" + type + " WHERE eventId = " + id);
            }
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    FieldModel fm = new FieldModel
                    {
                        ID = (int)dr["fieldId"],
                        Name = (string)dr["fieldName"],
                        Description = dr["fieldDescription"] as string,
                        RequiredCreate = (bool)dr["requiredCreation"],
                        RequiredApprove = (bool)dr["requiredApproval"],
                        Datatype = (Fieldtype)dr["fieldType"],
                        VarcharLength = (int)dr["varCharLength"]
                    };
                    if (id == -1 || value == null || value.Rows.Count == 0)
                    {
                        // We just set the values to standard values.
                        switch (fm.Datatype)
                        {
                            case Fieldtype.Float: fm.FloatValue = 0; break; //float
                            case Fieldtype.UserList: if (users == null) { users = this.GetUsers(msc, false); }
                                fm.List = users; break;
                            case Fieldtype.User: if (usersDrop == null) { usersDrop = this.GetUsers(msc, true); }
                                fm.List = usersDrop; fm.IntValue = 0; break;
                            case Fieldtype.GroupList: if (groups == null) { groups = this.GetGroups(msc, false); }
                                fm.List = groups; break;
                            case Fieldtype.Group: if (groupsDrop == null) { groupsDrop = this.GetGroups(msc, true); }
                                fm.List = groupsDrop; fm.IntValue = 0; break;
                            case Fieldtype.Text:
                            //case Fieldtype.FileList:
                            case Fieldtype.File: fm.StringValue = ""; fm.IntValue = 0; break;
                            case Fieldtype.Datetime: fm.DateValue = DateTime.Now; break;
                            case Fieldtype.Bool: fm.BoolValue = false; break; //bool
                        }
                    }
                    else
                    {
                        // We have to get the values too.
                        DataRow row = value.Rows[0];
                        switch (fm.Datatype)
                        {
                            case Fieldtype.Float: fm.FloatValue = row["field_" + fm.ID] is DBNull ? 0 : (float)row["field_" + fm.ID];
                                                break; //float
                            case Fieldtype.UserList: if (users == null) { users = this.GetUsers(msc, false); }
                                                fm.List = users; break;
                            case Fieldtype.User: if (usersDrop == null) { usersDrop = this.GetUsers(msc, true); }
                                                fm.List = usersDrop;
                                                fm.IntValue = row["field_" + fm.ID] is DBNull ? 0 : (int)row["field_" + fm.ID];
                                                break;
                            case Fieldtype.GroupList: if (groups == null) { groups = this.GetGroups(msc, false); }
                                                fm.List = groups; break;
                            case Fieldtype.Group: if (groupsDrop == null) { groupsDrop = this.GetGroups(msc, true); }
                                                fm.List = groupsDrop;
                                                fm.IntValue = row["field_" + fm.ID] is DBNull ? 0 : (int)row["field_" + fm.ID];
                                                break;
                            case Fieldtype.Text: fm.StringValue = row["field_" + fm.ID] is DBNull ? "" : (string)row["field_" + fm.ID];
                                                break;
                            //case Fieldtype.FileList:
                            case Fieldtype.File: fm.StringValue = ""; fm.IntValue = (int)row["field_" + fm.ID]; break;
                            case Fieldtype.Datetime: fm.DateValue = (DateTime)row["field_" + fm.ID]; break;
                            case Fieldtype.Bool: fm.BoolValue = (bool)row["field_" + fm.ID]; break; //bool
                        }
                    }
                    result.Add(fm);
                }
            }

            return result;
        }

        /// <summary>
        /// Getter for a SelectListItem list of all groups.
        /// </summary>
        /// <param name="msc">MySqlConnect object</param>
        /// <param name="nullVal">If true, includes a null-value</param>
        /// <returns>A SelecetListItem list of groups</returns>
        private List<SelectListItem> GetGroups(MySqlConnect msc, bool nullVal)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            if (nullVal) { result.Insert(0,new SelectListItem { Value = "0", Text = "Select group" }); }
            string groupquery = "SELECT groupId,groupName FROM pksudb.groups ORDER BY groupName";
            DataTable dt = msc.ExecuteQuery(groupquery);
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    result.Add(new SelectListItem
                    {
                        Value = ((int)dr["groupId"]).ToString(),
                        Text = (string)dr["groupName"]
                    });
                }
            }
            return result;
        }

        /// <summary>
        /// Getter for a SelectListItem list of all users.
        /// </summary>
        /// <param name="msc">MySqlConnect object</param>
        /// <param name="nullVal">If true, includes a null-value</param>
        /// <returns>A SelecetListItem list of users</returns>
        private List<SelectListItem> GetUsers(MySqlConnect msc, bool nullVal)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            if (nullVal) { result.Insert(0, new SelectListItem { Value = "0", Text = "Select user" }); }
            string userquery = "SELECT userId,userName FROM pksudb.users ORDER BY userName";
            DataTable dt = msc.ExecuteQuery(userquery);
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    result.Add(new SelectListItem
                    {
                        Value = ((int)dr["userId"]).ToString(),
                        Text = (string)dr["userName"]
                    });
                }
            }
            return result;
        }

        /// <summary>
        /// Getter for a list of all event types. This should be extended to check for create-authentication
        /// </summary>
        /// <param name="msc">MySqlConnect</param>
        /// <returns>A list of SelectListItems with event type names and ids</returns>
        private List<SelectListItem> GetEventTypes(MySqlConnect msc)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            result.Add(new SelectListItem { Value = "0", Text = "Select event type" });
            CustomQuery userquery = new CustomQuery { Cmd = "SELECT eventTypeId,eventTypeName FROM pksudb.eventtypes" };
            DataTable dt = msc.ExecuteQuery(userquery);
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    result.Add(new SelectListItem
                    {
                        Value = ((int)dr["eventTypeId"]).ToString(),
                        Text = (string)dr["eventTypeName"]
                    });
                }
            }
            return result;
        }
    }
}
