using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CalendarApplication.Models.Event;

namespace CalendarApplication.Models.Calendar
{
    public class CalendarDay : Calendar
    {
        public DateTime Date { set; get; }
        public List<BasicEvent> Events { set; get; }
        public Boolean Active { set; get; }
    }
}