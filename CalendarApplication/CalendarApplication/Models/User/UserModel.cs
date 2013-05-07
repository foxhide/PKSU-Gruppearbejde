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

        [Display(Name = "Real name:")]
        public string RealName { set; get; }

        [Display(Name = "E-mail")]
        public string Email { set; get; }

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
            DataTable dt = msc.ExecuteQuery("SELECT userId,userName,realName,email,admin FROM pksudb.users WHERE userId = " + ID);
            if(dt == null) { return null; }

            UserModel result = new UserModel
            {
                ID = ID,
                UserName = (string)dt.Rows[0]["userName"],
                RealName = (string)dt.Rows[0]["realName"],
                Email = dt.Rows[0]["email"] as string,
                Admin = (bool)dt.Rows[0]["admin"]
            };

            return result;
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
                Cmd = "SELECT * FROM groups NATURAL JOIN groupmembers NATURAL JOIN users WHERE userId = @userId ORDER BY groupId DESC", 
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
            //MySqlConnect msc = new MySqlConnect();
            //return msc.GetEvents(false,"userId = "+ID,"eventStart");

            return GetEvents(this.ID);
        }
        public static List<BasicEvent> GetEvents(int ID)
        {
            List<BasicEvent> result = new List<BasicEvent>();
            string[] argnames = { "@userId" };
            object[] args = { ID };
            CustomQuery query = new CustomQuery
            {
                Cmd = "SELECT * FROM (events NATURAL JOIN users NATURAL JOIN eventtypes) " +
                      "WHERE userId = @userId ORDER BY eventStart",
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
                            Creator = (string)dt["username"],
                            CreatorId = (int)dt["userId"],
                            TypeName = (string)dt["eventTypeName"],
                            Start = (DateTime)dt["eventStart"],
                            End = (DateTime)dt["eventEnd"],
                            State = (int)dt["state"]
                        });
                }
            }
            return result;
        }
    }
}