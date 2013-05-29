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
        public string Name { get; set; }
        public int ID { get; set; }
        [Display(Name = "Event types allowed to view, create and edit")]
        public List<SelectListItem> EventTypes { set; get; }
    }
}