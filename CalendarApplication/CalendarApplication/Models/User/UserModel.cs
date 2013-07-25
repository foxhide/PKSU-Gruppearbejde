using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data;

using CalendarApplication.Database;
using CalendarApplication.Models.Event;
using CalendarApplication.Models.Group;

namespace CalendarApplication.Models.User
{
    /// <summary>
    /// Class UserModel holds data for a user and have a few methods for getting additional data.
    /// </summary>
    public class UserModel
    {
        public int ID { set; get; }

        [Display(Name = "User name:")]
        public string UserName { set; get; }

        [Display(Name = "First name:")]
        public string FirstName { set; get; }

        [Display(Name = "Last name:")]
        public string LastName { set; get; }

        [Display(Name = "E-mail")]
        public string Email { set; get; }

        [Display(Name = "Phone number")]
        public string Phone { set; get; }

        public bool Admin { set; get; }
        public bool Active { set; get; }

        /// <summary>
        /// Gets the data for the user specified by the user ID
        /// </summary>
        /// <param name="ID">The ID (userId) of the user in the database</param>
        /// <returns>A UserModel object with the data</returns>
        public static UserModel GetUser(int ID)
        {
            MySqlConnect msc = new MySqlConnect();
            string cmd = "SELECT userId,userName,firstName,lastName,phoneNum,email,admin FROM users WHERE userId = @uid";
            object[] argval = { ID };
            string[] argnam = { "@uid" };
            CustomQuery query = new CustomQuery { Cmd = cmd, ArgNames = argnam, Args = argval };
            DataTable dt = msc.ExecuteQuery(query);
            if(dt == null) { return null; }

            UserModel result = new UserModel
            {
                ID = ID,
                UserName = (string)dt.Rows[0]["userName"],
                FirstName = dt.Rows[0]["firstName"] as string,
                LastName = dt.Rows[0]["lastName"] as string,
                Phone = dt.Rows[0]["phoneNum"] as string,
                Email = dt.Rows[0]["email"] as string,
                Admin = (bool)dt.Rows[0]["admin"]
            };

            return result;
        }

        /// <summary>
        /// Gets the full name of the user
        /// </summary>
        /// <returns>Full name</returns>
        public string GetFullName()
        {
            return this.FirstName + " " + this.LastName;
        }

        /// <summary>
        /// Gets the current userId from the cookie
        /// </summary>
        /// <returns>The ID of the currently signed in user</returns>
        public static int GetCurrentUserID()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return int.Parse(HttpContext.Current.User.Identity.Name);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Gets a model of the currently signed in user
        /// </summary>
        /// <returns>A model of the currently signed in user</returns>
        public static UserModel GetCurrent()
        {
            int id = GetCurrentUserID();
            return (id >= 0 ? GetUser(id) : null);
        }

        /// <summary>
        /// Get the groups, that this user is a member of
        /// </summary>
        /// <returns>List of groups</returns>
        public List<GroupModel> GetGroups()
        {
            return GetGroups(this.ID);
        }

        public static List<GroupModel> GetGroups(int ID)
        {
            List<GroupModel> result = new List<GroupModel>();
            string[] argnames = { "@userId" };
            object[] args = { ID };
            CustomQuery query = new CustomQuery
            {
                Cmd = "SELECT * FROM groups NATURAL JOIN groupmembers WHERE userId = @userId ORDER BY groupName DESC", 
                ArgNames = argnames,
                Args = args
            };
            MySqlConnect msc = new MySqlConnect();
            DataTable table = msc.ExecuteQuery(query);
            if (table != null)
            {
                foreach (DataRow dt in table.Rows)
                {
                    result.Add(new GroupModel { ID = (int)dt["groupId"], Name = (string)dt["groupName"] });
                }
            }
            return result;
        }

        /// <summary>
        /// Get the events created by this user
        /// </summary>
        /// <returns>List of basic events created by this user</returns>
        public List<BasicEvent> GetEvents()
        {
            return GetEvents(this.ID);
        }

        /// <summary>
        /// Get events created by the user with the given ID
        /// </summary>
        /// <param name="ID">ID for the user</param>
        /// <returns>Events created by the user with id = ID</returns>
        public static List<BasicEvent> GetEvents(int userId)
        {
            List<BasicEvent> result = new List<BasicEvent>();
            string[] argnames = { "@userId" };
            object[] args = { userId };

            int currentUser = UserModel.GetCurrentUserID();
            
            string command;
            if (currentUser == -1)
            {
                // No user -> only visible events
                command = "SELECT eventId,eventName,eventTypeName,state"
                            + "FROM (events NATURAL JOIN users NATURAL JOIN eventtypes)"
                            + "WHERE userId = @userId AND visible = 1 ORDER BY eventStart";
            }
            else if (currentUser == userId || UserModel.GetCurrent().Admin)
            {
                // User is looking at his/her own events or user is admin
                command = "SELECT eventId,eventName,eventTypeName,state"
                            + " FROM (events NATURAL JOIN users NATURAL JOIN eventtypes)"
                            + " WHERE userId = @userId ORDER BY eventStart";
            }
            else
            {
                // Check if user in edit-group/user or visible-group or if event is visible.
                command = "SELECT e.eventId, e.eventName,eventTypeName, e.state"
                                + " FROM events AS e"
                                + " LEFT JOIN (SELECT eventId,userId"
                                + " FROM eventeditorsusers"
                                + " WHERE userId = @uid) AS edt_user ON e.eventId = edt_user.eventId"
                                + " LEFT JOIN (SELECT eventId,userId"
                                + " FROM eventvisibility NATURAL JOIN groupmembers"
                                + " WHERE userId = @uid) AS vis_group ON e.eventId = vis_group.eventId"
                                + "	LEFT JOIN (SELECT eventId,userId"
                                + " FROM eventeditorsgroups NATURAL JOIN groupmembers"
                                + "	WHERE userId = @uid) AS edt_group ON e.eventId = edt_group.eventId"
                                + " WHERE e.userId = @userId AND (visible = 1 OR edt_group.userId IS NOT NULL"
                                + " OR edt_user.userId IS NOT NULL OR vis_group.userId IS NOT NULL)"
                                + " ORDER BY eventStart";
                argnames = new[] { "@userId", "@uid" };
                args = new[] { (object)userId, (object)currentUser };
            }

            CustomQuery query = new CustomQuery
            {
                Cmd = command,
                ArgNames = argnames,
                Args = args
            };
            MySqlConnect msc = new MySqlConnect();
            DataTable table = msc.ExecuteQuery(query);
            if (table != null)
            {
                foreach (DataRow dt in table.Rows)
                {
                    result.Add(new BasicEvent
                        {
                            ID = (int)dt["eventId"],
                            Name = (string)dt["eventName"],
                            TypeName = (string)dt["eventTypeName"],
                            State = (int)dt["state"]
                        });
                }
            }
            return result;
        }

        /// <summary>
        /// Checks if a user may view a given event
        /// </summary>
        /// <param name="eventId">Event to be checked</param>
        /// <param name="userId">User to be checked</param>
        /// <returns>True if view is allowed, otherwise false</returns>
        public static bool ViewAuthentication(int eventId, int userId)
        {
            MySqlConnect msc = new MySqlConnect();
            if (userId == -1)
            {
                CustomQuery checkVisibility = new CustomQuery
                {
                    Cmd = "SELECT visible FROM events WHERE eventId = @eid",
                    ArgNames = new[] { "@eid" },
                    Args = new[] { (object)eventId }
                };
                DataTable d = msc.ExecuteQuery(checkVisibility);
                return d != null && d.Rows.Count == 1 && (bool)d.Rows[0]["visible"];
            }
            else
            {
                string command = "SELECT e.visible,e.userId,vis_group.userId AS group_vis,"
                                    + "edt_group.userId AS group_edt, edt_user.userId AS user_edt"
                                    + " FROM events AS e"
                                    + " LEFT JOIN (SELECT eventId,userId"
                                    + " FROM eventeditorsusers"
                                    + " WHERE userId = @uid) AS edt_user ON e.eventId = edt_user.eventId"
                                    + " LEFT JOIN (SELECT eventId,userId"
                                    + " FROM eventvisibility NATURAL JOIN groupmembers"
                                    + " WHERE userId = @uid) AS vis_group ON e.eventId = vis_group.eventId"
                                    + "	LEFT JOIN (SELECT eventId,userId"
                                    + " FROM eventeditorsgroups NATURAL JOIN groupmembers"
                                    + "	WHERE userId = @uid) AS edt_group ON e.eventId = edt_group.eventId"
                                    + " WHERE e.eventId = @eid";

                CustomQuery query = new CustomQuery
                {
                    Cmd = command,
                    ArgNames = new[] { "@eid", "@uid" },
                    Args = new[] { (object)eventId, (object)userId }
                };
                DataTable dt = msc.ExecuteQuery(query);
                if (dt == null || dt.Rows.Count == 0) { return false; }
                return (bool)dt.Rows[0]["visible"]
                        || (int)dt.Rows[0]["userId"] == userId
                        || !(dt.Rows[0]["group_vis"] is DBNull)
                        || !(dt.Rows[0]["group_edt"] is DBNull)
                        || !(dt.Rows[0]["user_edt"] is DBNull)
                        || UserModel.GetCurrent().Admin;
            }
        }

        /// <summary>
        /// Checks if a user may edit a given event
        /// </summary>
        /// <param name="eventId">Event to be checked</param>
        /// <param name="userId">User to be checked</param>
        /// <returns>True if edit is allowed, otherwise false</returns>
        public static bool EditAuthentication(int eventId, int userId)
        {
            if (userId == -1) { return false; }
            string command = "SELECT e.userId,edt_group.userId AS group_edt, edt_user.userId AS user_edt"
                                + " FROM events AS e"
                                + " LEFT JOIN (SELECT eventId,userId"
                                + " FROM eventeditorsusers"
                                + " WHERE userId = @uid) AS edt_user ON e.eventId = edt_user.eventId"
                                + "	LEFT JOIN (SELECT eventId,userId"
                                + " FROM eventeditorsgroups NATURAL JOIN groupmembers"
                                + "	WHERE userId = @uid) AS edt_group ON e.eventId = edt_group.eventId"
                                + " WHERE e.eventId = @eid";

            MySqlConnect msc = new MySqlConnect();
            CustomQuery query = new CustomQuery
            {
                Cmd = command,
                ArgNames = new[] { "@eid", "@uid" },
                Args = new[] { (object)eventId, (object)userId }
            };
            DataTable dt = msc.ExecuteQuery(query);
            if (dt == null || dt.Rows.Count == 0) { return false; }
            return (int)dt.Rows[0]["userId"] == userId
                    || !(dt.Rows[0]["group_edt"] is DBNull)
                    || !(dt.Rows[0]["user_edt"] is DBNull)
                    || UserModel.GetCurrent().Admin;
        }
    }
}