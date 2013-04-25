using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CalendarApplication.Models.User
{
    public class UserViewModel
    {
        //
        // GET: /UserViewModel/

        public int UserId { set; get; }
        public string UserName { set; get; }
        public string Password { set; get; }
        public string RealName { set; get; }
        public bool Admin { set; get; }
        public string Email { set; get; }
        public bool Active { set; get; }
        public bool NeedsApproval { set; get; }
    }
}
