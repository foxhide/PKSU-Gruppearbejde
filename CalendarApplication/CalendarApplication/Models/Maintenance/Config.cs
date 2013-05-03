using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CalendarApplication.Models.Maintenance
{
    /// <summary>
    /// This class implements user configuration of the website.
    /// So far, only starting hour of day is implemented (hardcoded).
    /// </summary>
    public static class Config
    {
        public static int GetStartingHourOfDay()
        {
            return 6;
        }
    }
}