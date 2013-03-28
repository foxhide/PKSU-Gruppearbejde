using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

using CalendarApplication.Models.User;
using CalendarApplication.Models.EventType;
using CalendarApplication.Models.Event;

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

        public bool ViewState0 { set; get; }
        public bool ViewState1 { set; get; }
        public bool ViewState2 { set; get; }

        public List<EventTypeModel> Eventtypes { set; get; }
    }
}