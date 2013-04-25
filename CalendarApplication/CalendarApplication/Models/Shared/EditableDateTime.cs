using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CalendarApplication.Models.Shared
{
    /// <summary>
    /// EditableDateTime is a 'wrapper' for C#'s DateTime, which allows for not only reading but also writing values.
    /// Thus, an EditableDateTime may be used to get input from the view, which is not allowed for DateTime.
    /// </summary>
    public class EditableDateTime
    {
        /// <summary>
        /// Standard constructor
        /// </summary>
        public EditableDateTime() { }

        /// <summary>
        /// Constructor using regular datetime
        /// </summary>
        /// <param name="dt">Datetime</param>
        public EditableDateTime(DateTime dt)
        {
            this.Year = dt.Year;
            this.Month = dt.Month;
            this.Day = dt.Day;
            this.Hour = dt.Hour;
            this.Minute = dt.Minute;
        }

        /// <summary>
        /// Constructor for EditableDateTime
        /// </summary>
        /// <param name="year">Year for this datetime</param>
        /// <param name="month">Month for this datetime</param>
        /// <param name="day">Day for this datetime</param>
        /// <param name="hour">Hour for this datetime</param>
        /// <param name="minute">Minute for this datetime</param>
        public EditableDateTime(int year, int month, int day, int hour, int minute)
        {
            this.Year = year;
            this.Month = month;
            this.Day = day;
            this.Hour = hour;
            this.Minute = minute;
        }

        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }

        /// <summary>
        /// Creates a regular datetime from this EditableDateTime
        /// </summary>
        /// <returns>DateTime</returns>
        public DateTime GetDateTime()
        {
            return new DateTime(this.Year, this.Month, this.Day, this.Hour, this.Minute, 0);
        }

        /// <summary>
        /// Creates a datetime string for MySql database insertion
        /// </summary>
        /// <returns>A string in the format yyyy-MM-dd hh:mm:ss</returns>
        public string GetDBString()
        {
            return this.GetDateTime().ToString("yyyy-MM-dd hh:mm:ss");
        }
    }
}