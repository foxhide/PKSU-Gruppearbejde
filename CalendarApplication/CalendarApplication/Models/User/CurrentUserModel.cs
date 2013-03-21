using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace CalendarApplication.Models.User
{
    public class CurrentUserModel
    {
        private CurrentUserModel() { }

        public int Id { get; set; }

        public string UserName { get; set; }

        public string RealName { get; set; }

        public string Email { get; set; }

        public static CurrentUserModel GetCurrent()
        {
            string[] data = HttpContext.Current.User.Identity.Name.Split(new char[] { '|' });
            return new CurrentUserModel
            {
                Id = int.Parse(data[0]),
                UserName = data[1],
                RealName = data[2],
                Email = data[3]
            };
        }

        public static bool UpdateCurrent()
        {
            if (System.Web.HttpContext.Current.Request.Cookies.AllKeys.Contains(FormsAuthentication.FormsCookieName))
            {
                HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                CurrentUserModel cur = GetCurrent();
                FormsAuthenticationTicket oldTicket = FormsAuthentication.Decrypt(cookie.Value);
                FormsAuthenticationTicket newTicket = new FormsAuthenticationTicket(
                    oldTicket.Version, 
                    cur.Id + "|" + cur.UserName + "|" + cur.RealName + "|" + cur.Email,
                    oldTicket.IssueDate,
                    oldTicket.Expiration,
                    oldTicket.IsPersistent,
                    ""
                    );
                cookie.Value = FormsAuthentication.Encrypt(newTicket);
                return true;
            }
            return false;
        }
    }
}