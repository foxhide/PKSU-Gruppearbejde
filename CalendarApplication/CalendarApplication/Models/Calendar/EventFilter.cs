using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using CalendarApplication.Models.EventType;

namespace CalendarApplication.Models.Calendar
{
    public class EventFilter
    {
        public bool ViewState0 { set; get; }
        public bool ViewState1 { set; get; }
        public bool ViewState2 { set; get; }

        public List<EventTypeModel> Eventtypes { set; get; }
    }
}