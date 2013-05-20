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
    public enum CalendarMode
    {
        MONTH,
        DAY,
        LIST
    }

    public class EventViewModel
    {
        public CalendarMode Mode { set; get; }
        public Calendar CurrentModel { set; get; }

        public DateTime DateFrom { set; get; }
        public DateTime DateTo { set; get; }

        public bool ViewState0 { set; get; }
        public bool ViewState1 { set; get; }
        public bool ViewState2 { set; get; }

        public List<EventTypeModel> Eventtypes { set; get; }
    }
}