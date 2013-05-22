using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CalendarApplication.Models.Calendar
{
    public enum CalendarMode
    {
        MONTH,
        DAY,
        LIST
    }

    public class Calendar
    {
        public static object InitialView = new
        {
            mode = 0,
            year = DateTime.Now.Year,
            month = DateTime.Now.Month,
            day = DateTime.Now.Day,
            range = 0
        };

        public CalendarMode Mode { set; get; }
        public EventFilter Filter { set; get; }
    }
}