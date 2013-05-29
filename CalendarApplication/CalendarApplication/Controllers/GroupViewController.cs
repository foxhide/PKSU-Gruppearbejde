using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CalendarApplication.Database;
using System.Data;
using CalendarApplication.Models.Group;
using CalendarApplication.Models.User;
using CalendarApplication.Models.EventType;

namespace CalendarApplication.Controllers
{
    public class GroupViewController : Controller
    {
        /// <summary>
        /// Making the group list
        /// </summary>
        /// <returns>view of list of GroupModel</returns>
        public ActionResult Index()
        {
            GroupListModel model = new GroupListModel { GroupList = new List<GroupModel>() };
            MySqlConnect msc = new MySqlConnect();
            string grcmd = "SELECT * FROM pksudb.groups";
            CustomQuery grquery = new CustomQuery { Cmd = grcmd, ArgNames = { }, Args = { } };
            DataTable dt = msc.ExecuteQuery(grcmd);

            foreach (DataRow dr in dt.Rows)
            {
                model.GroupList.Add( new GroupModel
                {
                    ID = ((int)dr["groupId"]),
                    Name = (string)dr["groupName"] /*,
                    Selected = false,
                    canCreate = null,
                    groupLeaders = null,
                    groupMembers = null, */
                });
            }

            return View(model);
        }

        /// <summary>
        /// Viewing a specific group from ID
        /// </summary>
        /// <param name="groupId"> group to be viewed</param>
        /// <returns>view of GroupViewModel</returns>
        public ActionResult ViewGroup(int groupId)
        {
            GroupViewModel model = new GroupViewModel
            {
                ID = groupId,
                Members = new List<Models.User.UserModel>(),
                Leaders = new List<Models.User.UserModel>(),
                Creators = new List<Models.User.UserModel>(),
                EventTypes = new List<EventTypeModel>()
            };
            string cmd1 = "SELECT eventTypeId, eventTypeName FROM pksudb.eventcreationgroups "
                          + "NATURAL JOIN pksudb.eventtypes WHERE groupId = @gid";
            string[] argnam1 = new string[] { "@gid" };
            object[] args1 = new object[] { groupId };
            CustomQuery query1 = new CustomQuery { Cmd = cmd1, ArgNames = argnam1, Args = args1 };

            string cmd2 = "SELECT groupId, groupName, groupLeader, canCreate, userName, userId "
                         + "FROM pksudb.groups NATURAL JOIN pksudb.groupmembers NATURAL JOIN pksudb.users "
                         + "WHERE groupId = @gid AND active = 1";
            //perhaps inactive members should be shown anyway?
            string[] argnam2 = { "@gid" };
            object[] args2 = { groupId };
            CustomQuery query2 = new CustomQuery { Cmd = cmd2, ArgNames = argnam2, Args = args2 };
            MySqlConnect con = new MySqlConnect();
            CustomQuery[] queries = new CustomQuery[] { query1 , query2 };
            DataSet ds = con.ExecuteQuery(queries);
            DataTable dt0 = ds.Tables[0];
            DataTable dt1 = ds.Tables[1];
            if (dt1.Rows.Count > 0)
            {
                model.Name = (string)dt1.Rows[0]["groupName"].ToString();
            }

            foreach (DataRow dr in dt0.Rows)
            {
                model.EventTypes.Add(new EventTypeModel { ID = (int)dr["eventTypeId"], Name = (string)dr["eventTypeName"].ToString() });
            }

            foreach (DataRow dr in dt1.Rows)
            {
                //should maybe only add each to their own list instead of having all leaders in
                //creator list and having everybody in the memberlist
                if ((bool)dr["groupLeader"])
                {
                    model.Leaders.Add(new UserModel { ID = (int)dr["userId"], UserName = (string)dr["userName"].ToString() });
                    model.Creators.Add(new UserModel { ID = (int)dr["userId"], UserName = (string)dr["userName"].ToString() });
                    model.Members.Add(new UserModel { ID = (int)dr["userId"], UserName = (string)dr["userName"].ToString() });
                }
                else if ((bool)dr["canCreate"])
                {
                    model.Creators.Add(new UserModel { ID = (int)dr["userId"], UserName = (string)dr["userName"].ToString() });
                    model.Members.Add(new UserModel { ID = (int)dr["userId"], UserName = (string)dr["userName"].ToString() });
                }
                else
                {
                    model.Members.Add(new UserModel { ID = (int)dr["userId"], UserName = (string)dr["userName"].ToString() });
                }

            }
            return View(model);
        }

        /// <summary>
        /// get view for SetPrivileges page
        /// </summary>
        /// <param name="groupId">id of group have privileges changed</param>
        /// <returns>view of GroupPrivilegesModel</returns>
        public ActionResult SetPrivileges(int groupId)
        {
            // Check if user is logged in and is admin
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }
            else if (!(UserModel.GetCurrent().Admin))
            {
                return RedirectToAction("Index", "Home", null);
            }

            GroupModel grp = MySqlGroup.getGroup(groupId);
            GroupPrivilegesModel model = new GroupPrivilegesModel{
                ID = grp.ID,
                Name = grp.Name,
                EventTypes = new List<SelectListItem>()
            };

            string cmd1 = "SELECT eventTypeId FROM pksudb.eventcreationgroups WHERE groupId = @gid";
            string[] argnam1 = new string[] { "@gid" };
            object[] args1 = new object[] { groupId };
            CustomQuery query1 = new CustomQuery { Cmd = cmd1, ArgNames = argnam1, Args = args1 };

            string cmd2 = "SELECT * FROM pksudb.eventtypes";
            CustomQuery query2 = new CustomQuery { Cmd = cmd2, ArgNames = { }, Args = { } };

            CustomQuery[] queries = new CustomQuery[] { query1, query2 };
            MySqlConnect msc = new MySqlConnect();
            DataSet ds = msc.ExecuteQuery(queries);
            DataTable dt0 = ds.Tables[0];
            DataTable dt1 = ds.Tables[1];

            List<int> currentEt = new List<int>();

            for (int i = 0; i < dt0.Rows.Count; i++)
            {
                if (!(dt0.Rows[i]["eventTypeId"] is DBNull))
                {
                    currentEt.Add((int)dt0.Rows[i]["eventTypeId"]);
                }
            }

            for (int i = 0; i < dt1.Rows.Count; i++)
            {
                if (!(dt1.Rows[i]["eventTypeId"] is DBNull))
                {
                    model.EventTypes.Add(new SelectListItem
                    {
                        Text = (string)dt1.Rows[i]["eventTypeName"],
                        Value = (string)dt1.Rows[i]["eventTypeId"].ToString(),
                        Selected = currentEt.Contains((int)dt1.Rows[i]["eventTypeId"])
                    });
                }

            }

            return View(model);
        }

        /// <summary>
        /// HttpPost for SetPrivileges
        /// </summary>
        /// <param name="model">GroupPrivilegesModel to change in database</param>
        /// <returns>Goes back to view of group</returns>
        [HttpPost]
        public ActionResult SetPrivileges(GroupPrivilegesModel model)
        {
            // Check if user is logged in and is admin
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }
            else if (!(UserModel.GetCurrent().Admin))
            {
                return RedirectToAction("Index", "Home", null);
            }

            MySqlGroup msg = new MySqlGroup();
            bool ok = msg.SetPrivileges(model);
            if (!ok)
            {
                TempData["errorMsg"] = msg.ErrorMessage;
                return View(model);
            }

            return RedirectToAction("ViewGroup", "GroupView", new {groupId = model.ID});
        }
    }
}
