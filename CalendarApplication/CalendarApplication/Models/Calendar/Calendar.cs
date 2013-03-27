using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CalendarApplication.Models.Calendar
{
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
    }
}