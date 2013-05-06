using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CalendarApplication.Models.User
{
    public class UserEditModel : UserModel
    {
        public int EditType { set; get; }
        public bool AdminEdit { set; get; }

        public string OldPassword { set; get; }
        public string NewPassword { set; get; }
        public string RepeatPassword { set; get; }

        public bool Active { set; get; }
        public bool NeedsApproval { set; get; }
    }
}