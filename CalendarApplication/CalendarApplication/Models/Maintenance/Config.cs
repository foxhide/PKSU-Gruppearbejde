using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace CalendarApplication.Models.Maintenance
{
    /// <summary>
    /// This class implements user configuration of the website.
    /// So far, only starting hour of day is implemented (hardcoded).
    /// </summary>
    public static class Config
    {
        public static string GetVersion()
        {
            return ConfigurationManager.AppSettings["CalendarApplication:Version"];
        }

        public static int GetStartingHourOfDay()
        {
            return int.Parse(ConfigurationManager.AppSettings["CalendarApplication:StartingHourOfDay"]);
        }
    }
}