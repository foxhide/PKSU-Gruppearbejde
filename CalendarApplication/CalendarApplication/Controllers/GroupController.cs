﻿using System;
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
    public class GroupController : Controller
    {
        /*
        /// <summary>
        /// Making the group list
        /// </summary>
        /// <returns>view of list of GroupModel</returns>
        public ActionResult Index()
        {
            //Only for users
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }

            GroupListModel model = new GroupListModel { GroupList = new List<GroupModel>() };
            MySqlConnect msc = new MySqlConnect();
            string grcmd = "SELECT * FROM groups ORDER BY groupName";
            CustomQuery grquery = new CustomQuery { Cmd = grcmd, ArgNames = { }, Args = { } };
            DataTable dt = msc.ExecuteQuery(grquery);

            foreach (DataRow dr in dt.Rows)
            {
                model.GroupList.Add( new GroupModel
                {
                    ID = ((int)dr["groupId"]),
                    Name = (string)dr["groupName"] ,
                    Selected = false,
                    canCreate = null,
                    groupLeaders = null,
                    groupMembers = null
                });
            }

            return View(model);
        }
        */

        /// <summary>
        /// Viewing a specific group from ID
        /// </summary>
        /// <param name="groupId"> group to be viewed</param>
        /// <returns>view of GroupViewModel</returns>
        public ActionResult ViewGroup(int groupId)
        {
            //Only for users
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }

            GroupViewModel model = new GroupViewModel
            {
                ID = groupId,
                Members = new List<GroupViewModel.GroupUserModel>(),
                EventTypes = new List<EventTypeModel>()
            };
            string cmd1 = "SELECT eventTypeId, eventTypeName FROM eventcreationgroups "
                          + "NATURAL JOIN eventtypes WHERE groupId = @gid ORDER BY eventTypeName";
            string[] argnam1 = new string[] { "@gid" };
            object[] args1 = new object[] { groupId };
            CustomQuery query1 = new CustomQuery { Cmd = cmd1, ArgNames = argnam1, Args = args1 };

            string cmd2 = "SELECT groupId, groupName, groupLeader, canCreate, firstName, lastName, userId "
                         + "FROM groups NATURAL JOIN groupmembers NATURAL JOIN users "
                         + "WHERE groupId = @gid AND active = 1 ORDER BY userName";
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
                model.EventTypes.Add(new EventTypeModel { 
                    ID = (int)dr["eventTypeId"], 
                    Name = (string)dr["eventTypeName"].ToString() 
                });
            }

            foreach (DataRow dr in dt1.Rows)
            {
                model.Members.Add(new GroupViewModel.GroupUserModel
                {
                    ID = (int)dr["userId"],
                    Name = (string)dr["firstName"].ToString() + " " + (string)dr["lastName"].ToString(),
                    Leader = (bool)dr["groupLeader"],
                    Creator = (bool)dr["canCreate"]
                });
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

            string cmd1 = "SELECT eventTypeId FROM eventcreationgroups WHERE groupId = @gid";
            string[] argnam1 = new string[] { "@gid" };
            object[] args1 = new object[] { groupId };
            CustomQuery query1 = new CustomQuery { Cmd = cmd1, ArgNames = argnam1, Args = args1 };

            string cmd2 = "SELECT * FROM eventtypes";
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

            return RedirectToAction("ViewGroup", "Group", new {groupId = model.ID});
        }
    }
}
