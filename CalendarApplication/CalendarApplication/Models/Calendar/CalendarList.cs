using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using CalendarApplication.Models.Event;

namespace CalendarApplication.Models.Calendar
{
    public enum EventOrder
    {
        START,
        END,
        TYPE,
        NAME,
        CREATOR,
        STATE
    }

    public class CalendarList : Calendar
    {
        // Allowed limits for the list
        public static int[] LIMITS = { 10, 20, 50 };

        public int TotalEventCount { get; set; }
        public List<BasicEvent> Events { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool All { get; set; }

        public int Limit { get; set; }
        public int OldLimit { get; set; }
        public int EventFrom { get; set; }

        public EventOrder Order { get; set; }
        public bool Descending { get; set; }
    }
}