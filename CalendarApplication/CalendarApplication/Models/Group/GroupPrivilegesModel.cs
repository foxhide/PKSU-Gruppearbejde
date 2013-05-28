using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CalendarApplication.Models.Group
{
    public class GroupPrivilegesModel
    {
        [Display(Name = "Event types allowed")]
        public List<SelectListItem> EventTypes { set; get; }

    }
}