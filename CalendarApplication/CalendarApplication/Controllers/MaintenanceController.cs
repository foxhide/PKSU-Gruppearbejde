using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;

using CalendarApplication.Models.EventType;
using CalendarApplication.Models.Maintenance;
using CalendarApplication.Controllers;
using CalendarApplication.Models.User;
using CalendarApplication.Models.Group;
using CalendarApplication.Database;
using System.Windows.Forms;

namespace CalendarApplication.Controllers
{
    public class MaintenanceController : Controller
    {

        //
        // GET: /Maintenance/

        public ActionResult Index()
        {
            // Check if user is logged in and is admin
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }
            else if (!UserModel.GetCurrent().Admin) { return RedirectToAction("Index", "Home", null); }

            MaintenanceModel mm = new MaintenanceModel
            {
                EventTypes = new List<SelectListItem>(),
                SelectedEventType = "0",
                Groups = new List<SelectListItem>(),
                SelectedGroup = "0",
                Rooms = new List<SelectListItem>(),
                SelectedRoom = "0"
            };

            MySqlConnect msc = new MySqlConnect();
            string etcmd = "SELECT eventTypeId,eventTypeName FROM eventtypes";
            CustomQuery etquery = new CustomQuery { Cmd = etcmd };
            string grcmd = "SELECT * FROM groups";
            CustomQuery grquery = new CustomQuery { Cmd = grcmd, ArgNames = { }, Args = { } };
            string rmcmd = "SELECT * FROM rooms";
            CustomQuery rmquery = new CustomQuery { Cmd = rmcmd, ArgNames = { }, Args = { } };
            CustomQuery[] queries = { etquery, grquery, rmquery };
            DataSet ds = msc.ExecuteQuery(queries);
            DataTable dt0 = ds.Tables[0];
            DataTable dt1 = ds.Tables[1];
            DataTable dt2 = ds.Tables[2];
            if (dt0 != null)
            {
                foreach (DataRow dr in dt0.Rows)
                {
                    mm.EventTypes.Add(new SelectListItem
                    {
                        Value = ((int)dr["eventTypeId"]).ToString(),
                        Text = (string)dr["eventTypeName"]
                    });
                }
                foreach (DataRow dr in dt1.Rows)
                {
                    mm.Groups.Add(new SelectListItem
                    {
                        Value = ((int)dr["groupId"]).ToString(),
                        Text = (string)dr["groupName"]
                    });
                }
                foreach (DataRow dr in dt2.Rows)
                {
                    mm.Rooms.Add(new SelectListItem
                    {
                        Value = ((int)dr["roomId"]).ToString(),
                        Text = (string)dr["roomName"]
                    });
                }
                return View(mm);
            }
            TempData["errorMsg"] = msc.ErrorMessage;
            return View(mm);
        }

        [HttpPost]
        public ActionResult Index(MaintenanceModel mm)
        {
            // Check if user is logged in and is admin
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }
            else if (!UserModel.GetCurrent().Admin) { return RedirectToAction("Index", "Home", null); }
            bool ok;
            switch (mm.SubmitValue)
            {
                //Edit event
                case 0: return RedirectToAction("EditEventType", new { eventId = int.Parse(mm.SelectedEventType) });
                //Create event
                case 1: return RedirectToAction("EditEventType", new { eventId = -1 });
                //Edit group
                case 2: return RedirectToAction("EditGroup", new { groupId = int.Parse(mm.SelectedGroup) });
                //Create group
                case 3: return RedirectToAction("EditGroup", new { groupId = -1 });
                //Edit room name
                case 4: return RedirectToAction("EditRoom", new { roomId = int.Parse(mm.SelectedRoom) });
                //Create Room
                case 5: return RedirectToAction("EditRoom", new { roomId = -1 });
                //Delete room
                case 6: MySqlRoom msr = new MySqlRoom();
                        ok = msr.DeleteRoom(int.Parse(mm.SelectedRoom));
                        if (!ok) { TempData["errorMsg"] = msr.ErrorMessage; }
                        return RedirectToAction("Index", "Maintenance", "");
                //Cancelled deletion of room
                case 7: return RedirectToAction("Index", "Maintenance", "");
                //Delete group
                case 8: MySqlGroup msg = new MySqlGroup();
                    ok = msg.DeleteGroup(int.Parse(mm.SelectedGroup));
                    if (!ok) { TempData["errorMsg"] = msg.ErrorMessage; }
                    return RedirectToAction("Index", "Maintenance", "");
                //Cancelled deletion of group
                case 9: return RedirectToAction("Index", "Maintenance", "");
                //Set group privileges
                case 10: return RedirectToAction("SetPrivileges", "Group", new { groupId = int.Parse(mm.SelectedGroup) });
            }
            return View(mm);
        }

        public ActionResult EditEventType(int eventTypeId)
        {
            // Check if user is logged in and is admin
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }
            else if (!UserModel.GetCurrent().Admin) { return RedirectToAction("Index", "Home", null); }

            EventTypeModel etm = new EventTypeModel { TypeSpecific = new List<FieldDataModel>() };
            if (eventTypeId == -1)
            {
                etm.ID = -1;
            }
            else
            {
                string getType = "SELECT * FROM (eventtypes NATURAL LEFT JOIN eventtypefields) WHERE eventTypeId = @eid ORDER BY fieldOrder";
                MySqlConnect msc = new MySqlConnect();
                object[] argval = { eventTypeId };
                string[] argnam = { "@eid" };
                CustomQuery query = new CustomQuery { Cmd = getType, ArgNames = argnam, Args = argval };
                DataTable dt = msc.ExecuteQuery(query);
                etm.ID = eventTypeId;
                etm.Name = (string)dt.Rows[0]["eventTypeName"];
                etm.ActiveFields = 0;

                if (dt.Rows[0]["fieldName"] as string != null)
                {
                    etm.ActiveFields = dt.Rows.Count;
                    foreach (DataRow dr in dt.Rows)
                    {
                        FieldDataModel fdm = new FieldDataModel
                        {
                            ID = (int)dr["fieldId"],
                            Name = (string)dr["fieldName"],
                            Description = dr["fieldDescription"] as string,
                            RequiredCreate = (bool)dr["requiredCreation"],
                            RequiredApprove = (bool)dr["requiredApproval"],
                            Datatype = (Fieldtype)dr["fieldType"],
                            ViewID = etm.TypeSpecific.Count,
                            VarcharLength = (int)dr["varCharLength"]
                        };
                        etm.TypeSpecific.Add(fdm);
                    }
                }
            }
            return View(etm);
        }

        [HttpPost]
        public ActionResult EditEventType(EventTypeModel etm)
        {
            // Check if user is logged in and is admin
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }
            else if (!UserModel.GetCurrent().Admin) { return RedirectToAction("Index", "Home", null); }

            MySqlEvent mse = new MySqlEvent();
            bool ok;
            if (etm.ID == -1)
            {
                ok = mse.CreateEventType(etm);
            }
            else
            {
                ok = mse.EditEventType(etm);
            }
            if (!ok)
            {
                TempData["errorMsg"] = mse.ErrorMessage;
                etm.TypeSpecific = new List<FieldDataModel>();
                return View(etm);
            }
            return RedirectToAction("Index","Maintenance",null);
        }

        // Returns the partial needed for making new fields in edit event type
        public ActionResult GetPartial(int id)
        {
            return PartialView("FieldDetails", new FieldDataModel(id));
        }

        public ActionResult EditGroup(int groupId)
        {
            // Check if user is logged in and is admin
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }
            else if (!(UserModel.GetCurrent().Admin || this.IsGroupLeader(groupId,UserModel.GetCurrentUserID())))
            {
                return RedirectToAction("Index", "Home", null);
            }

            GroupModel result = new GroupModel
            { ID = groupId,
                groupMembers = new List<SelectListItem>(), 
                groupLeaders = new List<SelectListItem>(),
                canCreate = new List<SelectListItem>()
            };
            
            MySqlConnect msc = new MySqlConnect();

            string cmd0 = "SELECT * FROM users WHERE needsApproval = @needsApproval";
            string[] argnames0 = { "@needsApproval" };
            object[] args0 = { 0 };
            CustomQuery query0 = new CustomQuery { Cmd = cmd0, ArgNames = argnames0, Args = args0 };
            string cmd1 = "SELECT * FROM groups NATURAL LEFT JOIN groupmembers NATURAL LEFT JOIN users WHERE groupId = @groupId";
            string[] argnames1 = { "@groupId" };
            object[] args1 = { groupId };
            CustomQuery query1 = new CustomQuery { Cmd = cmd1, ArgNames = argnames1, Args = args1 };

            DataSet ds = msc.ExecuteQuery(new CustomQuery[] { query0, query1 });
            DataTable dt0 = ds.Tables[0];
            DataTable dt1 = ds.Tables[1];

            List<int> groupMembers = new List<int>();
            for (int i = 0; i < dt1.Rows.Count; i++)
            {
                if (!(dt1.Rows[i]["userId"] is DBNull))
                {
                    groupMembers.Add((int)dt1.Rows[i]["userId"]);
                    result.groupLeaders.Add(new SelectListItem
                    {
                        Value = ((int)dt1.Rows[i]["userId"]).ToString(),
                        Text = (string)dt1.Rows[i]["firstName"] + " " + (string)dt1.Rows[i]["lastName"],
                        Selected = (bool)dt1.Rows[i]["groupLeader"]
                    });
                    result.canCreate.Add(new SelectListItem
                    {
                        Value = ((int)dt1.Rows[i]["userId"]).ToString(),
                        Text = (string)dt1.Rows[i]["firstName"] + " " + (string)dt1.Rows[i]["lastName"],
                        Selected = (bool)dt1.Rows[i]["canCreate"] || (bool)dt1.Rows[i]["groupLeader"]
                    });
                }
            }
                
            result.Name = (groupId < 0 ? "New Group" : (string)dt1.Rows[0]["groupName"]);

            for (int i = 0; i < dt0.Rows.Count; i++)
            {
                if (!(dt0.Rows[i]["userId"] is DBNull))
                {
                    result.groupMembers.Add(new SelectListItem
                        {
                            Value = ((int)dt0.Rows[i]["userId"]).ToString(),
                            Text = (string)dt0.Rows[i]["firstName"] + " " + (string)dt0.Rows[i]["lastName"],
                            Selected = groupMembers.Contains((int)dt0.Rows[i]["userId"])
                        });
                }

            }

            return View(result);
        }

        [HttpPost]
        public ActionResult EditGroup(GroupModel grm)
        {
            // Check if user is logged in and is admin
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }
            else if (!(UserModel.GetCurrent().Admin || this.IsGroupLeader(grm.ID, UserModel.GetCurrentUserID())))
            {
                return RedirectToAction("Index", "Home", null);
            }

            MySqlGroup msg = new MySqlGroup();
            bool ok;
            if (grm.ID == -1)
            {
                ok = msg.CreateGroup(grm);
            }
            else
            {
                ok = msg.EditGroup(grm);
            }
            if (!ok)
            {
                TempData["errorMsg"] = msg.ErrorMessage;
                return View(grm);
            }
            if (grm.ID != -1)
            {
                return RedirectToAction("EditGroup", "Maintenance", new { groupId = grm.ID });
            }
            return RedirectToAction("Index", "Maintenance", null);
        }

        /// <summary>
        /// Check if a user is a leader of a group
        /// </summary>
        /// <param name="groupId">Id of group</param>
        /// <param name="userId">Id of user</param>
        /// <returns>True if leader, otherwise false</returns>
        private bool IsGroupLeader(int groupId, int userId)
        {
            if (groupId == -1 || userId == -1) { return false; }

            MySqlConnect msc = new MySqlConnect();
            CustomQuery query = new CustomQuery
            {
                Cmd = "SELECT groupLeader FROM groupmembers WHERE groupId = @gid AND userId = @uid",
                ArgNames = new[] { "@gid", "@uid" },
                Args = new[] { (object)groupId, (object)userId }
            };
            DataTable dt = msc.ExecuteQuery(query);
            return dt != null && dt.Rows.Count == 1 && (bool)dt.Rows[0]["groupLeader"];
        }

        /// <summary>
        /// Gets the page for managing users - admins only
        /// </summary>
        /// <returns>The page for user management</returns>
        public ActionResult ManageUsers()
        {
            // Check if user is logged in and is admin
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }
            else if (!UserModel.GetCurrent().Admin) { return RedirectToAction("Index", "Home", null); }

            ManageUserModel mum = new ManageUserModel
            {
                UASelect = 0,
                UsersApproved = new List<SelectListItem>(),
                UNASelect = 0,
                UsersNotApproved = new List<SelectListItem>(),
                UISelect = 0,
                UsersInactive = new List<SelectListItem>()
            };

            MySqlConnect msc = new MySqlConnect();
            CustomQuery query = new CustomQuery { Cmd = "SELECT userId,firstName,lastName,needsApproval,active FROM users ORDER BY lastName" };
            DataTable dt = msc.ExecuteQuery(query);

            foreach (DataRow dr in dt.Rows)
            {
                SelectListItem sli = new SelectListItem { Value = ((int)dr["userId"]).ToString(), Text = (string)dr["firstName"] + " " + (string)dr["lastName"] };
                if (!((bool)dr["active"])) { mum.UsersInactive.Add(sli); }
                else if ((bool)dr["needsApproval"]) { mum.UsersNotApproved.Add(sli); }
                else { mum.UsersApproved.Add(sli); }
            }
            
            return View(mum);
        }

        /// <summary>
        /// Gets the page for managing event types
        /// </summary>
        /// <returns>The ManageEventTypes view</returns>
        public ActionResult ManageEventTypes()
        {
            ManageEventTypeModel met = new ManageEventTypeModel
            {
                ActiveEventTypes = new List<SelectListItem>(),
                InactiveEventTypes = new List<SelectListItem>()
            };

            MySqlConnect msc = new MySqlConnect();
            CustomQuery query = new CustomQuery();
            query.Cmd = "SELECT eventTypeId, eventTypeName, active FROM eventtypes";

            DataTable dt = msc.ExecuteQuery(query);
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    SelectListItem et = new SelectListItem { Value = ((int)dr["eventTypeId"]).ToString(), Text = (string)dr["eventTypeName"] };
                    
                    if ((bool)dr["active"]) { met.ActiveEventTypes.Add(et); }
                    else { met.InactiveEventTypes.Add(et); }
                }
            }
            else
            {
                TempData["errorMsg"] = msc.ErrorMessage;
            }

            return View(met);
        }

        /// <summary>
        /// Function for setting the active state of the given event type
        /// </summary>
        /// <param name="eventTypeId">The ID of the event type</param>
        /// <param name="active">The new active state, true or false</param>
        /// <returns>True on success, otherwise false</returns>
        public bool SetEventTypeActive(int eventTypeId, bool active)
        {
            if (UserModel.GetCurrentUserID() == -1 || !UserModel.GetCurrent().Admin) { return false; }

            MySqlEvent mse = new MySqlEvent();
            return mse.SetEventTypeActive(eventTypeId,active);
        }

        /// <summary>
        /// Gets the page for editing a room - admins only
        /// </summary>
        /// <param name="roomId">Id of the room to edit, -1 for new room</param>
        /// <returns>The page for editing the given room</returns>
        public ActionResult EditRoom(int roomId)
        {
            // Check if user is logged in and is admin
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }
            else if (!UserModel.GetCurrent().Admin) { return RedirectToAction("Index", "Home", null); }

            CalendarApplication.Models.Event.Room room = new CalendarApplication.Models.Event.Room{ ID = roomId, Name = "" };
            if (roomId != -1)
            {
                MySqlConnect sql = new MySqlConnect();
                string que = "SELECT roomName FROM rooms WHERE roomId = @id";
                string[] argsn = { "@id" };
                object[] args = { roomId };
                CustomQuery query = new CustomQuery { Cmd = que, Args = args, ArgNames = argsn };
                DataTable dt = sql.ExecuteQuery(query);
                string name = (string)dt.Rows[0]["roomName"];
                room.Name = name;
            }
            return View(room);
        }

        /// <summary>
        /// HttpPost for room edit
        /// </summary>
        /// <param name="room">Room model from the view</param>
        /// <returns>A redirect to Home/Index on success, otherwise a return to room view.</returns>
        [HttpPost]
        public ActionResult EditRoom(CalendarApplication.Models.Event.Room room)
        {
            // Check if user is logged in and is admin
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }
            else if (!UserModel.GetCurrent().Admin) { return RedirectToAction("Index", "Home", null); }

            int roomId = room.ID;
            bool worked = false;
            MySqlRoom sqlrm = new MySqlRoom();
            if (roomId == -1)
            {
                roomId = sqlrm.CreateRoom(room.Name);
            }
            else
            {
                worked = sqlrm.RenameRoom(roomId, room.Name);
            }

            if ((roomId != room.ID) || worked)
            {
                return RedirectToAction("Index","Maintenance",null);
            }
            TempData["errorMsg"] = sqlrm.ErrorMessage;
            return View(room);
        }
    }

}
