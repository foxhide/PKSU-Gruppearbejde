using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CalendarApplication.Models.User;
using System.ComponentModel.DataAnnotations;

namespace CalendarApplication.Models.Calendar
{
    public class EventViewModel
    {
        public int Mode { set; get; }
        public Calendar CurrentModel { set; get; }

        [Display(Name = "Day")]
        public int Day { set; get; }

        [Display(Name = "Month")]
        public int Month { set; get; }

        [Display(Name = "Year")]
        public int Year { set; get; }

        public int Range { set; get; }

        public List<GroupModel> GroupsAvailable { set; get; }
        public List<GroupModel> GroupsSelected { set; get; }

        public int Eventtypes { set; get; }
    }
}