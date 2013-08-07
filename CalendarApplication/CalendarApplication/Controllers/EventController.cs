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
using CalendarApplication.PDFBuilder;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace CalendarApplication.Controllers
{
    public class EventController : Controller
    {
        //
        // GET: /Event/

        public ActionResult Index(int eventId)
        {
            // Check if current user may view this event.
            if (!UserModel.ViewAuthentication(eventId,UserModel.GetCurrentUserID()))
            {
                if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }
                else { return RedirectToAction("Index", "Home", null); }
            }
            return View(this.GetEvent(eventId));
        }

        public ActionResult PrintEvent(int eventId)
        {
            // Check if current user may view this event.
            if (!UserModel.ViewAuthentication(eventId, UserModel.GetCurrentUserID()))
            {
                if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }
                else { return RedirectToAction("Index", "Home", null); }
            }
            return View(this.GetEvent(eventId));
        }

        public EventWithDetails GetEvent(int eventId)
        {
            EventWithDetails result = new EventWithDetails
            {
                ID = eventId
            };
            string eventinfo = "SELECT e.eventName,e.eventStart,e.eventEnd,e.state,e.visible,e.creation," +
                               "et.eventTypeId,et.eventTypeName,u.firstName,u.lastName,u.userId,r.roomId,r.roomName " +
                               "FROM events AS e NATURAL LEFT JOIN eventroomsused NATURAL LEFT JOIN rooms AS r " +
                               "JOIN eventtypes AS et ON e.eventTypeId = et.eventTypeId JOIN users AS u ON u.userId = e.userId " +
                               "WHERE eventId = @eid";
            MySqlConnect con = new MySqlConnect();
            object[] argval = { eventId };
            string[] argnam = { "@eid" };
            CustomQuery query = new CustomQuery { Cmd = eventinfo, ArgNames = argnam, Args = argval };
            DataTable table = con.ExecuteQuery(query);

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
                result.Creator = (string)rows[0]["firstName"] + " " + (string)rows[0]["lastName"];
                result.CreatorId = (int)rows[0]["userId"];
                result.CreationDate = rows[0]["creation"] is DBNull ? new DateTime() : (DateTime)rows[0]["creation"];
                result.Rooms = new List<Room>();
                for (int i = 0; i < rows.Count; i++)
                {
                    if(!(rows[i]["roomId"] is DBNull || rows[i]["roomName"] is DBNull))
                    {
                        result.Rooms.Add(new Room
                        {
                            ID = (int)rows[i]["roomId"],
                            Name = (string)rows[i]["roomName"]
                        });
                    }
                }
                CustomQuery query0 = new CustomQuery();
                query0.Cmd = "SELECT * FROM table_" + result.TypeId + " WHERE eventId = @eventId";
                query0.ArgNames = new string[] { "@eventId" };
                query0.Args = new object[] { eventId };
                CustomQuery query1 = new CustomQuery();
                query1.Cmd = "SELECT * FROM eventtypefields WHERE eventTypeId = @eventTypeId ORDER BY fieldOrder";
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
                        switch (fm.Datatype)
                        {
                            case Fieldtype.Float: fm.FloatValue = ds.Tables[0].Rows[0]["field_" + fm.ID] as float?; break;

                            case Fieldtype.File: if (DBNull.Value.Equals(ds.Tables[0].Rows[0]["field_" + fm.ID])) { fm.IntValue = 0; }
                                else { fm.IntValue = (int)ds.Tables[0].Rows[0]["field_" + fm.ID]; }
                                fm.StringValue = "File system not yet implemented."; break;

                            case Fieldtype.Text: fm.StringValue = ds.Tables[0].Rows[0]["field_" + fm.ID] as string; break;

                            case Fieldtype.Bool: fm.BoolValue = ds.Tables[0].Rows[0]["field_" + fm.ID] as bool? ?? false; break;

                            case Fieldtype.Datetime: fm.DateValue = ds.Tables[0].Rows[0]["field_" + fm.ID] as DateTime? ?? new DateTime(1,1,1); break;

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
                                    fm.StringValue = MySqlGroup.getGroup(fm.IntValue).Name;
                                }
                                break;
                            case Fieldtype.UserList: fm.List = GetList(fm.Datatype, eventId, fm.ID, con); break; // userlist, grouplist and filelist handled in GetList
                            case Fieldtype.GroupList: fm.List = GetList(fm.Datatype, eventId, fm.ID, con); break;
                            case Fieldtype.FileList: fm.List = GetList(fm.Datatype, eventId, fm.ID, con); break;
                            case Fieldtype.TextList: fm.StringList = GetStringList(fm.Datatype, eventId, fm.ID, con); break; // textlist handled in GetStringList

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
            return result;
        }

        private List<SelectListItem> GetList(Fieldtype type, int eventId, int fieldId, MySqlConnect msc)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            CustomQuery query = new CustomQuery();
            query.ArgNames = new[] { "@eid", "@fid" };
            query.Args = new[] { (object)eventId, (object)fieldId };

            if (type == Fieldtype.FileList)
            {
                // Do something here!
            }
            else if (type == Fieldtype.UserList)
            {
                query.Cmd = "SELECT userId,userName FROM users NATURAL JOIN userlist"
                            + " WHERE eventId = @eid AND fieldId = @fid";
                DataTable dt = msc.ExecuteQuery(query);
                if (dt != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        result.Add(new SelectListItem
                        {
                            Text = (string)dr["userName"],
                            Value = ((int)dr["userId"]).ToString()
                        });
                    }
                }
                else
                {
                    TempData["errorMsg"] = msc.ErrorMessage;
                }
            }
            else if (type == Fieldtype.GroupList)
            {
                query.Cmd = "SELECT groupId,groupName FROM groups NATURAL JOIN grouplist"
                            + " WHERE eventId = @eid AND fieldId = @fid";
                DataTable dt = msc.ExecuteQuery(query);
                if (dt != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        result.Add(new SelectListItem
                        {
                            Text = (string)dr["groupName"],
                            Value = ((int)dr["groupId"]).ToString()
                        });
                    }
                }
                else
                {
                    TempData["errorMsg"] = msc.ErrorMessage;
                }
            }
            else
            {
                TempData["errorMsg"] = "Wrong argument for GetList...";
            }
            return result;
        }

        private List<StringListModel> GetStringList(Fieldtype type, int eventId, int fieldId, MySqlConnect msc)
        {
            List<StringListModel> result = new List<StringListModel>();
            CustomQuery query = new CustomQuery();
            query.ArgNames = new[] { "@eid", "@fid" };
            query.Args = new[] { (object)eventId, (object)fieldId };

            if (type == Fieldtype.TextList)
            {
                query.Cmd = "SELECT text,stringListId FROM stringlist"
                            + " WHERE eventId = @eid AND fieldId = @fid";
                DataTable dt = msc.ExecuteQuery(query);
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        result.Add(new StringListModel
                        {
                            Text = (string)dt.Rows[i]["text"],
                            ID = (int)dt.Rows[i]["stringListId"],
                            Active = true,
                            Place = i
                        });
                    }
                }
                else
                {
                    TempData["errorMsg"] = msc.ErrorMessage;
                }
            }
            else
            {
                TempData["errorMsg"] = "Wrong argument for GetStringList...";
            }
            return result;
        }

        public DateTime ParseDateString(string date)
        {
            if (string.IsNullOrEmpty(date)) { return DateTime.Now; }

            DateTime result = DateTime.Now.AddMinutes(10);

            if (Regex.Match(date, @"[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}").Success)
            {
                string[] vals = date.Split(new char[] { '-', 'T', ':' });
                try
                {
                    result = new DateTime(Convert.ToInt32(vals[0]),
                                Convert.ToInt32(vals[1]),
                                Convert.ToInt32(vals[2]),
                                Convert.ToInt32(vals[3]),
                                Convert.ToInt32(vals[4]), 0);
                }
                catch (Exception e)
                {
                    TempData["errorMsg"] = e.Message;
                }
            }
            else { TempData["errorMsg"] = "Bad date string!"; }
            return result;
        }

        public ActionResult EditEvent(int eventId, string from, string to, string rooms)
        {
            // Check if that there is a user, or if it is an old event and user may edit this event
            if(UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }
            else if (eventId != -1 && !UserModel.EditAuthentication(eventId, UserModel.GetCurrentUserID()))
            {
                return RedirectToAction("Index", "Home", null);
            }

            EventEditModel eem = null;
            MySqlConnect msc = new MySqlConnect();
            DataTable dt = null;

            if (eventId == -1)
            {
                DateTime start = this.ParseDateString(from);
                DateTime end = this.ParseDateString(to);
                start = start < DateTime.Now.AddMinutes(10) ? DateTime.Now.AddMinutes(10) : start;
                end = end < start ? start.AddHours(2) : end;
                eem = new EventEditModel
                {
                    ID = eventId,
                    EventTypes = new List<SelectListItem>(),
                    SelectedEventType = "0", // Initial value -> No selection
                    Start = start,
                    End = end,
                    Visible = true
                };
            }
            else
            {
                CustomQuery query = new CustomQuery
                {
                    Cmd = "SELECT userId,eventTypeId,eventName,eventStart,eventEnd,visible,state FROM events WHERE eventId = @eid",
                    ArgNames = new[] { "@eid" },
                    Args = new[] { (object)eventId }
                };
                dt = msc.ExecuteQuery(query);
                if (dt != null)
                {
                    eem = new EventEditModel
                    {
                        ID = eventId,
                        Name = (string)dt.Rows[0]["eventName"],
                        CreatorId = (int)dt.Rows[0]["userId"],
                        EventTypes = new List<SelectListItem>(),
                        SelectedEventType = ((int)dt.Rows[0]["eventTypeId"]).ToString(),
                        Start = (DateTime)dt.Rows[0]["eventStart"],
                        End = (DateTime)dt.Rows[0]["eventEnd"],
                        Visible = (bool)dt.Rows[0]["visible"],
                        State = (int)dt.Rows[0]["state"],
                        Approved = (int)dt.Rows[0]["state"] > 0
                    };
                }
            }
            // Get list of rooms //
            List<int> roomsAdded = new List<int>();
            if (!string.IsNullOrEmpty(rooms))
            {
                string[] roomAry = rooms.Split(':');
                foreach (string r in roomAry) { roomsAdded.Add(Convert.ToInt32(r)); }
            }

            eem.RoomSelectList = new List<SelectListItem>();
            string roomcmd = eem.ID == -1 ? "SELECT roomId,roomName FROM rooms"
                               : "SELECT roomId,roomName,eventId FROM rooms NATURAL LEFT JOIN "
                                    + "(SELECT * FROM eventroomsused WHERE eventId = @eid ) AS r";
            CustomQuery roomquery = new CustomQuery
            {
                Cmd = roomcmd,
                ArgNames = new[] { "@eid" },
                Args = new[] { (object)eem.ID }
            };
            dt = msc.ExecuteQuery(roomquery);
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    SelectListItem room = new SelectListItem
                    {
                        Value = ((int)dr["roomId"]).ToString(),
                        Text = (string)dr["roomName"],
                        Selected = (eem.ID != -1 && !(dr["eventId"] is DBNull)) || (eem.ID == -1 && roomsAdded.Contains((int)dr["roomId"]))
                    };
                    eem.RoomSelectList.Add(room);
                }
            }

            // Get list of users
            eem.UserEditorList = new List<SelectListItem>();
            string usercmd = eem.ID == -1 ? "SELECT userId,userName FROM users WHERE admin = 0 AND userId != @uid ORDER BY userName"
                               : "SELECT userId,userName,eventId FROM ( SELECT * FROM users WHERE admin = 0 AND userId != @uid ) AS u  NATURAL LEFT JOIN "
                                + " ( SELECT * FROM eventeditorsusers WHERE eventId = @eid ) AS e ORDER BY userName";
            int creatorID = eem.ID == -1 ? UserModel.GetCurrentUserID() : eem.CreatorId;
            CustomQuery userquery = new CustomQuery
            {
                Cmd = usercmd,
                ArgNames = new[] { "@eid", "@uid" },
                Args = new[] { (object)eem.ID, creatorID }
            };

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
                string groupcmd = "SELECT groupId,groupName FROM groups ORDER BY groupName";
                CustomQuery groupquery = new CustomQuery
                {
                    Cmd = groupcmd,
                    ArgNames = new string[] { },
                    Args = new object[] { }
                };

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
                string cmd0 = "SELECT groupId,groupName,eventId FROM groups NATURAL LEFT JOIN "
                                    + "(SELECT * FROM eventeditorsgroups WHERE eventId = @eid ) AS g ORDER BY groupName";
                string cmd1 = "SELECT groupId,groupName,eventId FROM groups NATURAL LEFT JOIN "
                                    + "(SELECT * FROM eventvisibility WHERE eventId = @eid ) AS g ORDER BY groupName";
                CustomQuery query0 = new CustomQuery
                {
                    Cmd = cmd0,
                    ArgNames = new[] { "@eid" },
                    Args = new[] { (object)eem.ID }
                };

                CustomQuery query1 = new CustomQuery
                {
                    Cmd = cmd1,
                    ArgNames = new[] { "@eid" },
                    Args = new[] { (object)eem.ID }
                };

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

            // Get event types and type specifics
            this.GetEventTypes(eem, msc);

            TempData["errorMsg"] = msc.ErrorMessage;
            if (eventId != -1)
            {
                eem.TypeSpecifics = this.GetTypeSpecifics(eventId, int.Parse(eem.SelectedEventType), msc);
            }
            
            return View(eem);
        }

        [HttpPost]
        public ActionResult EditEvent(EventEditModel eem)
        {
            // Check if that there is a user, or if it is an old event and user may edit this event
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }
            else if (eem.ID != -1 && !UserModel.EditAuthentication(eem.ID, UserModel.GetCurrentUserID()))
            {
                return RedirectToAction("Index", "Home", null);
            }

            MySqlEvent mse = new MySqlEvent();
            eem.CreatorId = UserModel.GetCurrentUserID();

            // Serverside check for required fields
            bool state = true;
            bool req = false;
            bool rooms = false;

            // Check for name
            if (string.IsNullOrEmpty(eem.Name))
            {
                TempData["errorMsg"] = "The event must have a Name!";
                req = true;
            }
            // Check for type
            else if (eem.SelectedEventType.Equals("0"))
            {
                TempData["errorMsg"] = "The event must have a type!";
                req = true;
            }

            // Check for TypeSpecifics
            if (eem.TypeSpecifics != null && !req)
            {
                foreach (FieldModel fm in eem.TypeSpecifics)
                {
                    if (fm.RequiredApprove) { state = state && fm.GetDBValue() != null; }
                    if (fm.RequiredCreate) { req = fm.GetDBValue() == null; }
                    if (req) { TempData["errorMsg"] = "The field " + fm.Name + " must be filled to create this event!"; break; }
                }
            }

            // If no error, check rooms
            if (!req)
            {
                // Check that at least one room has been chosen
                foreach (SelectListItem room in eem.RoomSelectList)
                {
                    if (room.Selected) {
                        rooms = true;
                        break;
                    }
                }
                // If at least one room has been chosen, check availability 
                if (rooms)
                {
                    List<RoomWithTimes> roomCheck = this.CheckDates(eem.ID, eem.Start, eem.End, eem.RoomSelectList);
                    rooms = roomCheck != null && roomCheck.Count == 0;
                    if (!rooms)
                    {
                        // No room at given time
                        TempData["errorMsg"] = "There is an overlap with other rooms in the period chosen.";
                    }
                }
                else { TempData["errorMsg"] = "No rooms selected!"; } // No room chosen
            }
            

            if (!req && rooms)
            {
                // All required fields were filled -> try to create the event
                // Set state:
                eem.State = eem.Approved ? (state ? 2 : 1) : 0;

                // Create
                int id = mse.EditEvent(eem);
                if (id > 0)
                {
                    return RedirectToAction("Index", "Event", new { eventId = id });
                }
                else
                {
                    TempData["errorMsg"] = mse.ErrorMessage;
                }
            }

            // An error has occurred if we made it this far //

            // Get the types again for the view
            this.GetEventTypes(eem, mse);

            // Fill all the dropdown lists again, if any.
            List<SelectListItem> users = null;
            List<SelectListItem> groups = null;
            if (eem.TypeSpecifics != null)
            {
                foreach (FieldModel fm in eem.TypeSpecifics)
                {
                    if (fm.Datatype == Fieldtype.Group)
                    {
                        groups = groups == null ? this.GetGroups(mse, true) : groups;
                        fm.List = groups;
                    }
                    else if (fm.Datatype == Fieldtype.User)
                    {
                        users = users == null ? this.GetUsers(mse, true) : users;
                        fm.List = users;
                    }
                }
            }
            return View(eem);
        }

        /// <summary>
        /// Gets a partial view with the event specific fields. Called from js-AJAX
        /// </summary>
        /// <param name="type">The event type. Should always be > 0</param>
        /// <returns>A partial view containing a list of the (empty) specifics for the event type</returns>
        public ActionResult GetEventSpecific(int type)
        {
            // Check if this request is made by a user (should also have a check for event creation ok!)
            if (UserModel.GetCurrentUserID() == -1) { return null; }

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
        private List<FieldModel> GetTypeSpecifics(int eventId, int type, MySqlConnect msc)
        {
            List<FieldModel> result = new List<FieldModel>();
            List<SelectListItem> users = null;       // List for user list
            List<SelectListItem> groups = null;      // List for group list
            List<SelectListItem> usersDrop = null;   // List for user dropdown
            List<SelectListItem> groupsDrop = null;  // List for group dropdown
            List<StringListModel> stringList = null;  // List for text list
            string specQuery = "SELECT * FROM eventtypefields WHERE eventTypeId = @etid ORDER BY fieldOrder";
            object[] argval = { type };
            string[] argnam = { "@etid" };
            CustomQuery query = new CustomQuery { Cmd = specQuery, ArgNames = argnam, Args = argval };
            DataTable dt = msc.ExecuteQuery(query);
            DataTable value = null;
            if (eventId != -1)
            {
                string valcmd = "SELECT * FROM table_" + type + " WHERE eventId = @eid";
                object[] args = { eventId };
                string[] argnm = { "@eid" };
                CustomQuery valquery = new CustomQuery { Cmd = valcmd, ArgNames = argnm, Args = args };
                value = msc.ExecuteQuery(valquery);
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
                    if (eventId == -1 || value == null || value.Rows.Count == 0)
                    {
                        // We just set the values to standard values.
                        switch (fm.Datatype)
                        {
                            case Fieldtype.Float: fm.FloatValue = null; break; //float
                            case Fieldtype.UserList: if (users == null) { users = this.GetUsers(msc, false); }
                                fm.List = users; break;
                            case Fieldtype.User: if (usersDrop == null) { usersDrop = this.GetUsers(msc, true); }
                                fm.List = usersDrop; fm.IntValue = 0; break;
                            case Fieldtype.GroupList: if (groups == null) { groups = this.GetGroups(msc, false); }
                                fm.List = groups; break;
                            case Fieldtype.Group: if (groupsDrop == null) { groupsDrop = this.GetGroups(msc, true); }
                                fm.List = groupsDrop; fm.IntValue = 0; break;
                            case Fieldtype.TextList: if (stringList == null) { stringList = new List<StringListModel>(); }
                                fm.StringList = stringList; break;
                            case Fieldtype.Text: fm.StringValue = ""; break;
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
                            case Fieldtype.Float: fm.FloatValue = row["field_" + fm.ID] is DBNull ? null : (float?)row["field_" + fm.ID];
                                                break; //float
                            case Fieldtype.UserList: fm.List = this.GetUsersValues(msc, eventId, fm.ID); break;
                            case Fieldtype.User: if (usersDrop == null) { usersDrop = this.GetUsers(msc, true); }
                                                fm.List = usersDrop;
                                                fm.IntValue = row["field_" + fm.ID] is DBNull ? 0 : (int)row["field_" + fm.ID];
                                                break;
                            case Fieldtype.GroupList: fm.List = this.GetGroupsValues(msc, eventId, fm.ID); break;
                            case Fieldtype.Group: if (groupsDrop == null) { groupsDrop = this.GetGroups(msc, true); }
                                                fm.List = groupsDrop;
                                                fm.IntValue = row["field_" + fm.ID] is DBNull ? 0 : (int)row["field_" + fm.ID];
                                                break;
                            case Fieldtype.TextList: fm.StringList = this.GetStringValues(msc, eventId, fm.ID); break;
                            case Fieldtype.Text: fm.StringValue = row["field_" + fm.ID] is DBNull ? "" : (string)row["field_" + fm.ID];
                                                break;
                            //case Fieldtype.FileList:
                            case Fieldtype.File: fm.StringValue = ""; fm.IntValue = (int)row["field_" + fm.ID]; break;
                            case Fieldtype.Datetime: fm.DateValue = row["field_" + fm.ID] is DBNull ? new DateTime(1, 1, 1) : (DateTime)row["field_" + fm.ID]; break;
                            case Fieldtype.Bool: fm.BoolValue = row["field_" + fm.ID] is DBNull ? false : (bool)row["field_" + fm.ID]; break; //bool
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
            string groupcmd = "SELECT groupId,groupName FROM groups ORDER BY groupName";
            CustomQuery groupquery = new CustomQuery { Cmd = groupcmd, ArgNames = new string[] { }, Args = new object[] { } };
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
            string usercmd = "SELECT userId,userName FROM users ORDER BY userName";
            CustomQuery userquery = new CustomQuery { Cmd = usercmd, ArgNames = new string[] { }, Args = new object[] { } };
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
        /// Getter for a StringListModel list of string values pertaining to a particular event and field.
        /// </summary>
        /// <param name="msc">MySqlConnect object</param>
        /// <param name="eventId">Id of event</param>
        /// <param name="fieldId">Id of field</param>
        /// <returns>A StringListModel list of strings and stringListIds</returns>
        private List<StringListModel> GetStringValues(MySqlConnect msc, int eventId, int fieldId)
        {
            List<StringListModel> result = new List<StringListModel>();
            string cmd = "SELECT text,stringListId FROM stringlist WHERE eventId = @eid AND fieldId = @fid ORDER BY stringListId";
            string[] argnam = new string[] { "@eid", "@fid" };
            object[] args = new object[] { eventId, fieldId };
            CustomQuery query = new CustomQuery { Cmd = cmd, ArgNames = argnam, Args = args };
            DataTable dt = msc.ExecuteQuery(query);
            if (dt != null)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i] != null) {
                        result.Add(new StringListModel
                        {
                            ID = (int)dt.Rows[i]["stringListId"],
                            Text = dt.Rows[i]["text"] as string,
                            Active = true,
                            Place = i
                        });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Similar to GetGroups, but sets the selected field if the group has been added
        /// </summary>
        /// <param name="msc">MySqlConnect</param>
        /// <param name="eventId">Id of the event</param>
        /// <param name="fieldId">Id of the field</param>
        /// <returns>A list of all groups, with selected set</returns>
        private List<SelectListItem> GetGroupsValues(MySqlConnect msc, int eventId, int fieldId)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            CustomQuery query = new CustomQuery
            {
                Cmd = "SELECT groupId,groupName,fieldId FROM groups NATURAL LEFT JOIN"
                        + "(SELECT fieldId,groupId FROM grouplist WHERE eventId = @eid AND fieldId = @fid) AS gl"
                        + " ORDER BY groupName",
                ArgNames = new[] { "@eid", "@fid" },
                Args = new[] { (object)eventId, (object)fieldId }
            };
            DataTable dt = msc.ExecuteQuery(query);
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    result.Add(new SelectListItem
                    {
                        Value = ((int)dr["groupId"]).ToString(),
                        Text = (string)dr["groupName"],
                        Selected = !(dr["fieldId"] is DBNull) // Selected if in userlist
                    });
                }
            }
            else
            {
                TempData["errorMsg"] = msc.ErrorMessage;
            }
            return result;
        }

        /// <summary>
        /// Similar to GetUsers, but sets the selected field if the user has been added
        /// </summary>
        /// <param name="msc">MySqlConnect</param>
        /// <param name="eventId">Id of the event</param>
        /// <param name="fieldId">Id of the field</param>
        /// <returns>A list of all users, with selected set</returns>
        private List<SelectListItem> GetUsersValues(MySqlConnect msc, int eventId, int fieldId)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            CustomQuery query = new CustomQuery
            {
                Cmd = "SELECT userId,userName,fieldId FROM users NATURAL LEFT JOIN"
                        + "(SELECT fieldId,userId FROM userlist WHERE eventId = @eid AND fieldId = @fid) AS ul"
                        + " ORDER BY userName",
                ArgNames = new[] { "@eid", "@fid" },
                Args = new[] { (object)eventId, (object)fieldId }
            };
            DataTable dt = msc.ExecuteQuery(query);
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    result.Add(new SelectListItem
                    {
                        Value = ((int)dr["userId"]).ToString(),
                        Text = (string)dr["userName"],
                        Selected = !(dr["fieldId"] is DBNull) // Selected if in userlist
                    });
                }
            }
            else
            {
                TempData["errorMsg"] = msc.ErrorMessage;
            }
            return result;
        }

        /// <summary>
        /// Getter for a list of all event types.
        /// </summary>
        /// <param name="msc">MySqlConnect</param>
        /// <returns>A list of SelectListItems with event type names and ids</returns>
        private void GetEventTypes(EventEditModel eem, MySqlConnect msc)
        {
            eem.EventTypes = new List<SelectListItem>();
            if (eem.ID == -1) { eem.EventTypes.Add(new SelectListItem { Value = "0", Text = "Select event type" }); }
            CustomQuery userquery = new CustomQuery();
            if (UserModel.GetCurrentUserID() != -1 && UserModel.GetCurrent().Admin)
            {
                // Admin -> get all events
                userquery.Cmd = "SELECT eventTypeId,eventTypeName FROM eventtypes WHERE active = 1";
            }
            else
            {
                // Not admin -> get events allowed for creation by this user + the current selected type.
                userquery.Cmd = "SELECT DISTINCT(eventTypeId),eventTypeName "
                        + "FROM eventtypes NATURAL LEFT JOIN eventcreationgroups NATURAL LEFT JOIN groupmembers "
                        + "WHERE (userId = @uid AND canCreate = 1 AND active = 1) OR eventTypeId = @ti";
                userquery.ArgNames = new[] { "@uid", "@ti" };
                userquery.Args = new[] { (object)UserModel.GetCurrentUserID(), Convert.ToInt32(eem.SelectedEventType) };
            }
            DataTable dt = msc.ExecuteQuery(userquery);
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    eem.EventTypes.Add(new SelectListItem
                    {
                        Value = ((int)dr["eventTypeId"]).ToString(),
                        Text = (string)dr["eventTypeName"]
                    });
                }
            }

            // If old event and not admin, check if we are allowed to create the current event type
            if (eem.ID != -1 && !UserModel.GetCurrent().Admin)
            {
                CustomQuery cq = new CustomQuery
                {
                    Cmd = "SELECT eventTypeId FROM eventcreationgroups NATURAL JOIN groupmembers "
                            + "WHERE eventTypeId = @eti AND userId = @uid AND canCreate = 1",
                    ArgNames = new[] { "@eti", "@uid" },
                    Args = new object[] { Convert.ToInt32(eem.SelectedEventType), UserModel.GetCurrentUserID() }
                };
                dt = msc.ExecuteQuery(cq);
                if (dt != null)
                {
                    // Set can change type - if any eventTypeIds were found, we may change the type, else not.
                    // The extra type has been added in GetEventTypes(...), no matter if we have creation rights or not
                    eem.CanChangeType = dt.Rows.Count != 0;
                }
                else
                {
                    TempData["errorMsg"] = msc.ErrorMessage;
                }
            }
            else { eem.CanChangeType = true; } // New event or admin
        }

        /// <summary>
        /// Checks if the given rooms are available in the given time span. Returns a list of rooms used, empty list if none.
        /// </summary>
        /// <param name="eventId">Id of the event wanting to use the rooms</param>
        /// <param name="start">Start time</param>
        /// <param name="end">End time</param>
        /// <param name="rooms">List of desired rooms</param>
        /// <returns>List of rooms used</returns>
        public List<RoomWithTimes> CheckDates(int eventId, DateTime start, DateTime end, List<SelectListItem> rooms)
        {
            // Sanity check
            if (rooms == null) { return null; }

            List<RoomWithTimes> result = new List<RoomWithTimes>();
            string cmd = "SELECT roomName, eventStart, eventEnd FROM events NATURAL JOIN eventroomsused NATURAL JOIN rooms"
                            + " WHERE ((@start >= eventStart AND @start < eventEnd) OR "
                            + "(@end > eventStart AND @end <= eventEnd) OR (@start <= eventStart AND @end >= eventEnd))"
                            + " AND eventId != @id AND (";
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].Selected)
                {
                    cmd += "roomId = " + rooms[i].Value + " OR ";
                }
            }
            cmd = cmd.Substring(0, cmd.Length - 4) + ") ORDER BY eventStart";  // Remove last OR
            CustomQuery query = new CustomQuery
            {
                Cmd = cmd,
                ArgNames = new[] { "@start", "@end", "@id" },
                Args = new[] { (object)start, (object)end, eventId }
            };
            MySqlConnect msc = new MySqlConnect();
            DataTable dt = msc.ExecuteQuery(query);

            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    result.Add(new RoomWithTimes
                    {
                        Name = (string)dr["roomName"],
                        Start = ((DateTime)dr["eventStart"]).ToString("dd/MM/yyyy HH:mm"),
                        End = ((DateTime)dr["eventEnd"]).ToString("dd/MM/yyyy HH:mm")
                    });
                }
                return result;
            }
            else
            {
                TempData["errorMsg"] = msc.ErrorMessage;
                return null;
            }
        }

        /// <summary>
        /// Returns a JSON string with the list of rooms obtained from CheckDates
        /// </summary>
        /// <param name="eventId">Id of the event wanting to use the rooms</param>
        /// <param name="start">Start time</param>
        /// <param name="end">End time</param>
        /// <param name="rooms">List of desired rooms</param>
        /// <returns>A JSON-string with a list of the rooms</returns>
        public string CheckDatesJson(int eventId, DateTime start, DateTime end, List<SelectListItem> rooms)
        {
            return JsonConvert.SerializeObject(this.CheckDates(eventId, start, end, rooms));
        }

        /// <summary>
        /// Function for setting the state of an event. Called through ajax
        /// </summary>
        /// <param name="eventId">ID of the event</param>
        /// <param name="approved">If the event should be approved or not</param>
        /// <returns>New state on success, -1 on error</returns>
        public int SetState(int eventId, bool approved)
        {
            // Authority check
            if (UserModel.GetCurrentUserID() == -1 || !UserModel.GetCurrent().Admin) { return -1; }

            MySqlEvent mse = new MySqlEvent();
            // Remove approval
            if (!approved)
            {
                return mse.SetEventState(eventId, 0) ? 0 : -1;
            }

            // Get event and field data
            CustomQuery query = new CustomQuery
            {
                Cmd = "SELECT * FROM events NATURAL JOIN eventtypefields WHERE eventId = @eid AND requiredApproval = 1",
                ArgNames = new string[] { "@eid" },
                Args = new object[] { eventId }
            };
            DataTable eventData = mse.ExecuteQuery(query);

            // Check for error
            if (eventData == null) { return -1; }

            // Check if event has any required for approval specific fields
            if (eventData.Rows.Count == 0)
            {
                return mse.SetEventState(eventId,2) ? 2 : -1;
            }

            // Get specific values
            query.Cmd = "SELECT * FROM table_" + (int)eventData.Rows[0]["eventTypeId"] + " WHERE eventId = @eid";
            DataTable specifics = mse.ExecuteQuery(query);

            // Check for error
            if (specifics == null || specifics.Rows.Count == 0) { return -1; }

            foreach (DataRow dr in eventData.Rows)
            {
                // Check if required field is not set
                if (specifics.Rows[0]["field_" + (int)dr["fieldId"]] is DBNull)
                {
                    // Was not set, set state = 1 and return
                    return mse.SetEventState(eventId, 1) ? 1 : -1;
                }
            }

            // All fields filled, set state = 2 and return
            return mse.SetEventState(eventId, 2) ? 2 : -1;
        }

        /// <summary>
        /// Deletes an event
        /// </summary>
        /// <param name="model">Model containing Id of the event to be deleted</param>
        /// <param name="delFiles">Whether all files associated with the event should also be deleted</param>
        /// <returns>calendar view, view of this event on error</returns>
        [HttpPost]
        public ActionResult Index(EventWithDetails model, bool delFiles)
        {
            // Check if user is logged in and is admin
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }
            else if (!(UserModel.GetCurrent().Admin))
            {
                return RedirectToAction("Index", "Home", null);
            }

            if (delFiles)
            {
                return DeleteEventAndFiles(model.ID);
            }
            else
            {
                return DeleteEvent(model.ID);
            }
        }

        /// <summary>
        /// Deletes an event
        /// </summary>
        /// <param name="eventId">Id of the event to be deleted</param>
        /// <returns>calendar view, view of this event on error</returns>
        private ActionResult DeleteEvent(int eventId)
        {
            MySqlEvent mse = new MySqlEvent();
            bool ok = mse.DeleteEvent(eventId);
            if (!ok)
            {
                TempData["errorMsg"] = mse.ErrorMessage;
                return View(GetEvent(eventId));
            }

            return RedirectToAction("", "Calendar", null);
        }

        /// <summary>
        /// Deletes an event and all files associated with event (FILE DELETION NOT IMPLEMENTED YET)
        /// </summary>
        /// <param name="eventId">Id of the event to be deleted</param>
        /// <returns>calendar view, view of this event on error</returns>
        private ActionResult DeleteEventAndFiles(int eventId)
        {
            MySqlEvent mse = new MySqlEvent();
            bool ok = mse.DeleteEvent(eventId);
            if (!ok)
            {
                TempData["errorMsg"] = mse.ErrorMessage;
                return View(GetEvent(eventId));
            }

            return RedirectToAction("", "Calendar", null);
        }

        // Returns the partial for adding a textbox to a string list
        public ActionResult GetStringListPartial(string viewName, string viewId, int place)
        {
            return PartialView("StringListPartial", new StringListModel { ID = -1, Active = true, Place = place, Text = "", ViewID = viewId, ViewName = viewName});
        }
    }
}
