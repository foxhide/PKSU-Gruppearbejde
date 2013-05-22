using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using CalendarApplication.Models.Event;

namespace CalendarApplication.Models.Calendar
{
    public class CalendarList : Calendar
    {
        public List<BasicEvent> Events { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool All { get; set; }

        public int Limit { get; set; }
        public int From { get; set; }
    }
}