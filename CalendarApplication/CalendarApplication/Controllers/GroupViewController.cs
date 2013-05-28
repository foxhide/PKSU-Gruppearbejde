using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CalendarApplication.Database;
using System.Data;
using CalendarApplication.Models.Group;

namespace CalendarApplication.Controllers
{
    public class GroupViewController : Controller
    {
        //
        // GET: /GroupView/

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

        //
        // GET: /GroupView/ViewGroup

        public ActionResult ViewGroup(int groupId)
        {
            GroupViewModel model = new GroupViewModel
            {
                ID = groupId,
                Members = new List<Models.User.UserModel>(),
                Leaders = new List<Models.User.UserModel>(),
                Creators = new List<Models.User.UserModel>()
            };
            
            string get = "SELECT groupId, groupName, groupLeader, canCreate, userName, userId "
                         + "FROM pksudb.groups NATURAL JOIN pksudb.groupmembers NATURAL JOIN pksudb.users "
                         + "WHERE groupId = @gid AND active = 1";
            //perhaps inactive members should be shown anyway?
            object[] arg = { groupId };
            string[] argn = { "@gid" };
            CustomQuery query = new CustomQuery { Cmd = get, ArgNames = argn, Args = arg };
            MySqlConnect con = new MySqlConnect();
            DataTable dt = con.ExecuteQuery(query);
            if (dt.Rows.Count > 0)
            {
                model.Name = (string)dt.Rows[0]["groupName"].ToString();
            }
            foreach (DataRow dr in dt.Rows)
            {
                //should maybe only add each to their own list instead of having all leaders in
                //creator list and all creators in in the memberlist
                if ((bool)dr["groupLeader"])
                {
                    model.Leaders.Add(new Models.User.UserModel { ID = (int)dr["userId"], UserName = (string)dr["userName"] });
                    model.Creators.Add(new Models.User.UserModel { ID = (int)dr["userId"], UserName = (string)dr["userName"] });
                    model.Members.Add(new Models.User.UserModel { ID = (int)dr["userId"], UserName = (string)dr["userName"] });
                }
                else if ((bool)dr["canCreate"])
                {
                    model.Creators.Add(new Models.User.UserModel { ID = (int)dr["userId"], UserName = (string)dr["userName"] });
                    model.Members.Add(new Models.User.UserModel { ID = (int)dr["userId"], UserName = (string)dr["userName"] });
                }
                else
                {
                    model.Members.Add(new Models.User.UserModel { ID = (int)dr["userId"], UserName = (string)dr["userName"] });
                }

            }
            return View(model);
        }

        //
        // GET: /GroupView/SetPrivileges

        public ActionResult SetPrivileges(int groupId)
        {
            return View();
        }

        //
        // SET: /GroupView/SetPrivileges

        public ActionResult SetPrivileges(GroupPrivilegesModel model)
        {
            return View();
        }
    }
}
