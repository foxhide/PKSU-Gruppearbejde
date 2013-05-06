using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CalendarApplication.Models.Maintenance
{
    public class ManageUserModel
    {
        public int SubmitType { get; set; }

        public int UASelect { get; set; }
        public List<SelectListItem> UsersApproved { get; set; }

        public int UNASelect { get; set; }
        public List<SelectListItem> UsersNotApproved { get; set; }

        public int UISelect { get; set; }
        public List<SelectListItem> UsersInactive { get; set; }
    }
}