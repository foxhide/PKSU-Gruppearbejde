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
        public static string[] MONTHS = { "January", "Feburary", "March", "April", "May", "June", "July",
                                            "August", "September", "Oktober", "November", "December" };

        public static string[] DAYS = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        public static string GetDateTimeString(DateTime datetime)
        {
            return DAYS[((int)datetime.DayOfWeek + 6) % 7] + ", " + MONTHS[datetime.Month - 1] + " " + datetime.ToString("d, yyyy - HH:mm");
        }

        public CalendarMode Mode { set; get; }
        public EventFilter Filter { set; get; }
    }
}