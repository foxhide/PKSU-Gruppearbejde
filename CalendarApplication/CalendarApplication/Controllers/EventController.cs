﻿using System;
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
            return View(GetEvent(eventId));
        }

        [HttpPost]
        public string PrintEvent(int eventId)
        {
            return EventBuilder.BuildPDF(GetEvent(eventId));
        }

        public EventWithDetails GetEvent(int eventId)
        {
            EventWithDetails result = new EventWithDetails
            {
                ID = eventId
            };
            string eventinfo = "SELECT * FROM pksudb.events NATURAL JOIN pksudb.eventroomsused " +
                               "NATURAL JOIN pksudb.rooms NATURAL JOIN pksudb.eventtypes NATURAL JOIN pksudb.users " +
                               "WHERE eventId = " + eventId;
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
                query0.Args = new object[] { eventId };
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
                        switch (fm.Datatype)
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
                                    fm.StringValue = MySqlGroup.getGroup(fm.IntValue).Name;
                                }
                                break;
                            case Fieldtype.UserList:
                            case Fieldtype.GroupList:
                            case Fieldtype.FileList: fm.List = GetList(fm.Datatype, eventId, fm.ID, con); break;
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
                query.Cmd = "SELECT userId,userName FROM pksudb.users NATURAL JOIN pksudb.userlist"
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
                query.Cmd = "SELECT groupId,groupName FROM pksudb.groups NATURAL JOIN pksudb.grouplist"
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

        public ActionResult EditEvent(int eventId, int year, int month, int day)
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
                eem = new EventEditModel
                {
                    ID = eventId,
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

            // Error //

            // Get the types again for the view
            eem.EventTypes = this.GetEventTypes(mse);

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
            string specQuery = "SELECT * FROM eventtypefields WHERE eventTypeId = " + type;
            DataTable dt = msc.ExecuteQuery(specQuery);
            DataTable value = null;
            if (eventId != -1)
            {
                value = msc.ExecuteQuery("SELECT * FROM table_" + type + " WHERE eventId = " + eventId);
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
                Cmd = "SELECT groupId,groupName,fieldId FROM pksudb.groups NATURAL LEFT JOIN"
                        + "(SELECT fieldId,groupId FROM pksudb.grouplist WHERE eventId = @eid AND fieldId = @fid) AS gl"
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
                Cmd = "SELECT userId,userName,fieldId FROM pksudb.users NATURAL LEFT JOIN"
                        + "(SELECT fieldId,userId FROM pksudb.userlist WHERE eventId = @eid AND fieldId = @fid) AS ul"
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
        /// Getter for a list of all event types. This should be extended to check for create-authentication
        /// </summary>
        /// <param name="msc">MySqlConnect</param>
        /// <returns>A list of SelectListItems with event type names and ids</returns>
        private List<SelectListItem> GetEventTypes(MySqlConnect msc)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            result.Add(new SelectListItem { Value = "0", Text = "Select event type" });
            CustomQuery userquery = new CustomQuery();
            if (UserModel.GetCurrentUserID() != -1 && UserModel.GetCurrent().Admin)
            {
                userquery.Cmd = "SELECT eventTypeId,eventTypeName FROM pksudb.eventtypes";
            }
            else
            {
                userquery.Cmd = "SELECT DISTINCT(eventTypeId),eventTypeName "
                        + "FROM pksudb.eventtypes NATURAL JOIN pksudb.eventcreationgroups NATURAL JOIN pksudb.groupmembers "
                        + "WHERE userId = @uid AND canCreate = 1";
                userquery.ArgNames = new[] { "@uid" };
                userquery.Args = new[] { (object)UserModel.GetCurrentUserID() };
            }
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
            List<RoomWithTimes> result = new List<RoomWithTimes>();
            string cmd = "SELECT roomName, eventStart, eventEnd FROM pksudb.events NATURAL JOIN pksudb.eventroomsused NATURAL JOIN pksudb.rooms"
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
    }
}
