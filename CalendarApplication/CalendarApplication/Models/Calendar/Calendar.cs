using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        public static string[] MONTHS = { "January", "February", "March", "April", "May", "June", "July",
                                            "August", "September", "October", "November", "December" };

        public static string[] DAYS = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        /// <summary>
        /// Function for printing a datetime with English day/month names (as defined above)
        /// </summary>
        /// <param name="datetime">The datetime to print</param>
        /// <returns>A string in the format DayOfWeek, MonthName d, yyyy - HH:mm</returns>
        public static string GetDateTimeString(DateTime datetime)
        {
            return DAYS[((int)datetime.DayOfWeek + 6) % 7] + ", " + MONTHS[datetime.Month - 1] + " " + datetime.ToString("d, yyyy - HH:mm");
        }

        /// <summary>
        /// Function for converting a datetime  to a string in the format yyyy/MM/dd HH:mm
        /// Used for creating string to be parsed by JS. Using ToString('yyyy/MM/dd HH:mm') only works in Chrome apparently.
        /// </summary>
        /// <param name="datetime">The datetime to be converted</param>
        /// <returns>A string in the format yyyy/MM/dd HH:mm</returns>
        public static string GetJSDate(DateTime datetime)
        {
            return datetime.Year + "/" + datetime.Month + "/" + datetime.Day + " " + datetime.Hour + ":" + datetime.Minute;
        }

        public CalendarMode Mode { set; get; }
        public EventFilter Filter { set; get; }
    }
}