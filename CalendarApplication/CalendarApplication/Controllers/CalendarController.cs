using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;

using CalendarApplication.Models.Calendar;
using CalendarApplication.Models.Event;
using CalendarApplication.Models.User;
using CalendarApplication.Models.EventType;

namespace CalendarApplication.Controllers
{
    public class CalendarController : Controller
    {
        //
        // GET: /Calendar/

        public ActionResult Index(int mode, int year, int month, int day, int range)
        {
            MySqlConnect msc = new MySqlConnect();
            EventViewModel evm = new EventViewModel
            {
                Mode = mode,
                Day = day,
                Month = month,
                Year = year,
                Range = range,
                ViewState0 = true,
                ViewState1 = true,
                ViewState2 = true,
                Eventtypes = msc.GetEventTypes("")
            };

            switch (mode)
            {
                case 0:     evm.CurrentModel = GetMonth(evm); break;
                case 1:     evm.CurrentModel = GetDay(evm); break;
                case 2:     evm.CurrentModel = GetList(evm); break;
            }

            return View(evm);
        }

        [HttpPost]
        public ActionResult Index(EventViewModel evm)
        {
            evm.Year = evm.Year > 0 ? evm.Year : 1;
            evm.Month = evm.Month <= 0 ? 1 : (evm.Month > 12 ? 12 : evm.Month);
            int days = DateTime.DaysInMonth(evm.Year,evm.Month);
            evm.Day = evm.Day <= 0 ? 1 : (evm.Day > days ? days : evm.Day);

            switch (evm.Mode)
            {
                case 0: evm.CurrentModel = GetMonth(evm); break;
                case 1: evm.CurrentModel = GetDay(evm); break;
                case 2: evm.CurrentModel = GetList(evm); break;
            }

            return View(evm);
        }

        private CalendarMonth GetMonth(EventViewModel evm)
        {
            List<CalendarDay> cdays = new List<CalendarDay>();

            DateTime first = new DateTime(evm.Year, evm.Month, 1);
            int before = (int)first.DayOfWeek == 0 ? 6 : (int)first.DayOfWeek - 1;
            int days = DateTime.DaysInMonth(evm.Year, evm.Month) + before;
            days = days % 7 > 0 ? days + (7 - days % 7) : days;

            string where = this.GetFilter(evm,first,first.AddDays(days-1));
            
            MySqlConnect msc = new MySqlConnect();
            List<BasicEvent> events = msc.GetEvents(false,where,"eventStart");

            first = first.AddDays(-before);

            for (int i = 0; i < days; i++)
            {
                DateTime myDate = first.AddDays(i);
                CalendarDay cd = new CalendarDay
                {
                    Date = myDate,
                    Active = myDate.Month == evm.Month,
                    Events = new List<BasicEvent>()
                };
                cdays.Add(cd);
            }

            foreach (BasicEvent ev in events)
            {
                TimeSpan end = ev.End - first;
                TimeSpan start = ev.Start - first;
                int startDay = start.Days < 0 ? 0 : start.Days;

                for (int i = startDay; i <= end.Days && i < cdays.Count; i++)
                {
                    cdays[i].Events.Add(ev);
                }
            }

            return new CalendarMonth { Date = new DateTime(evm.Year, evm.Month, 1), Days = cdays };
        }

        private CalendarDay GetDay(EventViewModel evm)
        {
            MySqlConnect msc = new MySqlConnect();
            DateTime date = new DateTime(evm.Year, evm.Month, evm.Day);
            string where = this.GetFilter(evm, date, date);

            List<BasicEvent> events = msc.GetEvents(true, where, "eventStart");

            CalendarDay result = new CalendarDay
            {
                Date = date,
                Rooms = msc.GetRooms(),
                Events = events
            };

            return result;
        }

        private CalendarList GetList(EventViewModel evm)
        {
            DateTime start = new DateTime(evm.Year, evm.Month, evm.Day);
            DateTime end = start.AddDays(evm.Range);
            string where = this.GetFilter(evm, start, end);

            MySqlConnect msc = new MySqlConnect();
            List<BasicEvent> events = msc.GetEvents(false, where, "eventStart");

            CalendarList model = new CalendarList
            {
                Events = events,
                Start = start,
                End = end,
            };

            return model;
        }

        /// <summary>
        /// Helper function: gets the string needed to filter events based on EventTypes, Dates and States
        /// </summary>
        /// <param name="evm">EventViewModel</param>
        /// <param name="start">Start date</param>
        /// <param name="end">End date</param>
        /// <returns>A string used in the where part of a </returns>
        private string GetFilter(EventViewModel evm, DateTime start, DateTime end)
        {
            string morning = start.ToString("yyyy-MM-dd 00:00:00");
            string night = end.ToString("yyyy-MM-dd 23:59:59");
            string result = "((eventStart <= '" + night + "' AND eventStart >= '" + morning
                            + "') OR (eventEnd <= '" + night + "' AND eventEnd >= '" + morning
                            + "') OR (eventEnd >= '" + night + "' AND eventStart <= '" + morning + "'))";

            foreach (EventTypeModel etm in evm.Eventtypes)
            {
                if (!etm.Selected)
                {
                    result += " AND ";
                    result += "(eventTypeId != " + etm.ID + ")";
                }
            }

            result += (evm.ViewState0 ? "" : " AND (state != 0)");
            result += (evm.ViewState1 ? "" : " AND (state != 1)");
            result += (evm.ViewState2 ? "" : " AND (state != 2)");

            return "("+result+")";
        }
    }
}
