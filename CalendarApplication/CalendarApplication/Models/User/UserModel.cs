using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

using CalendarApplication.Controllers;
using CalendarApplication.Models.Event;

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

        /// <summary>
        /// Gets the data for the user specified by the user ID
        /// </summary>
        /// <param name="ID">The ID (userId) of the user in the database</param>
        /// <returns>A UserModel object with the data</returns>
        public static UserModel GetUser(int ID)
        {
            MySqlConnect msc = new MySqlConnect();
            return msc.GetUser(ID);
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
            MySqlConnect msc = new MySqlConnect();
            return msc.GetGroups("groups NATURAL JOIN groupmembers NATURAL JOIN users","userId = "+ID);
        }

        /// <summary>
        /// Get the events created by this user
        /// </summary>
        /// <returns>List of basic events created by this user</returns>
        public List<BasicEvent> GetEvents()
        {
            MySqlConnect msc = new MySqlConnect();
            return msc.GetEvents(false,"userId = "+ID,"eventStart");
        }
    }
}