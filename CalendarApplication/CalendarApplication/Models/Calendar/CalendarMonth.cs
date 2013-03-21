using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace CalendarApplication.Models.Calendar
{
    public class CalendarMonth : Calendar
    {
        public List<CalendarDay> Days { get; set; }
        public DateTime Date { get; set; }

    }
}