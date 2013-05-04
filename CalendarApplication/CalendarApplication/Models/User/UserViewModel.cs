using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CalendarApplication.Models.Event;
using CalendarApplication.Models.Group;

namespace CalendarApplication.Models.User
{
    public class UserViewModel : UserModel
    {
        public List<GroupModel> Groups { set; get; }
        public List<BasicEvent> Events { set; get; }
    }
}
