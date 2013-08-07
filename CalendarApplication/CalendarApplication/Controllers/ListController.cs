using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CalendarApplication.Database;
using CalendarApplication.Models.Group;
using CalendarApplication.Models.User;
using CalendarApplication.Models.Event;

namespace CalendarApplication.Controllers
{
    public class ListController : Controller
    {
        //
        // GET: /List/

        //public ActionResult Index()
        //{
        //    return View();
        //}

        /// <summary>
        /// Making the group list
        /// </summary>
        /// <returns>view of list of GroupModel</returns>
        public ActionResult GroupList()
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
                    Name = (string)dr["groupName"],
                    Open = dr["open"] as bool? ?? false
                });
            }
            return View(model);
        }

        /// <summary>
        /// Making the room list
        /// </summary>
        /// <returns>view of list of Room models</returns>
        public ActionResult RoomList()
        {
            //Only for users
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }

            RoomListModel model = new RoomListModel { RoomList = new List<Room>() };
            MySqlConnect msc = new MySqlConnect();
            string rmcmd = "SELECT * FROM rooms ORDER BY roomName";
            CustomQuery rmquery = new CustomQuery { Cmd = rmcmd, ArgNames = { }, Args = { } };
            DataTable dt = msc.ExecuteQuery(rmquery);

            foreach (DataRow dr in dt.Rows)
            {
                model.RoomList.Add(new Room
                {
                    ID = ((int)dr["roomId"]),
                    Name = (string)dr["roomName"],
                    Capacity = dr["capacity"] as int?,
                    Description = dr["description"] as string
                });
            }

            return View(model);
        }

        /// <summary>
        /// Making the user list
        /// </summary>
        /// <returns>view of list of UserModel</returns>
        public ActionResult UserList()
        {
            //Only for users
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }

            UserListModel model = new UserListModel { UserList = new List<UserModel>() };
            MySqlConnect msc = new MySqlConnect();
            string usrcmd = "SELECT * FROM users WHERE active = 1 ORDER BY firstName";
            CustomQuery usrquery = new CustomQuery { Cmd = usrcmd, ArgNames = { }, Args = { } };
            DataTable dt = msc.ExecuteQuery(usrquery);

            foreach (DataRow dr in dt.Rows)
            {
                model.UserList.Add(new UserModel
                {
                    ID = ((int)dr["userId"]),
                    UserName = (string)dr["userName"] ,
                    FirstName = dr["firstName"] as string,
                    LastName = dr["lastName"] as string,
                    Admin = dr["admin"] as bool? ?? false,
                    Email = dr["email"] as string,
                    Phone = dr["phoneNum"] as string
                });
            }

            return View(model);
        }
    }
}