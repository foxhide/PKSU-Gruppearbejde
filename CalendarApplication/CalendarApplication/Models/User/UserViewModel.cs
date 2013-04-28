using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CalendarApplication.Models.Event;

namespace CalendarApplication.Models.User
{
    public class UserViewModel : UserModel
    {
        //
        // GET: /UserViewModel/

        public string Password { set; get; }
        //TODO remove when implemented in usermodel
        public bool Active { set; get; }
        public bool NeedsApproval { set; get; }

        public List<GroupModel> Groups { set; get; }
        public List<BasicEvent> Events { set; get; }
    }
}
