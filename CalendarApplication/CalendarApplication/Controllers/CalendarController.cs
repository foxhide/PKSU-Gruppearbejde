using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using CalendarApplication.Models.Calendar;
using CalendarApplication.Models.Event;
using CalendarApplication.Models.User;

namespace CalendarApplication.Controllers
{
    public class CalendarController : Controller
    {
        //
        // GET: /Calendar/

        public ActionResult Month(int year, int month, ViewCheckModel vcm)
        {
            DateTime date = DateTime.Today;
            if (year != 0 && month != 0)
            {
                date = new DateTime(year, month, 1);
            }
            List<CalendarDay> dayList = getDays(date.Month,date.Year);

            if (true)
            {
                vcm = new ViewCheckModel
                {
                    Month = month,
                    Year = year,
                    Eventtypes = 0,
                    GroupsAvailable = new List<GroupModel>(),
                    GroupsSelected = new List<GroupModel>()
                };
                vcm.GroupsAvailable.Add(new GroupModel { ID = 1, Name = "Admins" });
                vcm.GroupsAvailable.Add(new GroupModel { ID = 2, Name = "PokerPlayers" });
                vcm.GroupsAvailable.Add(new GroupModel { ID = 3, Name = "Band" });
                vcm.GroupsSelected.Add(new GroupModel { ID = 3, Name = "Band" });
            }

            return View(new CalendarMonth { Days = dayList, Date = date, VCM = vcm });
        }

        public ActionResult ViewCheck(ViewCheckModel vcm)
        {
            return RedirectToAction("Month", new { vcm.Year, vcm.Month, vcm });
        }

        private List<CalendarDay> getDays(int month, int year)
        {
            List<CalendarDay> result = new List<CalendarDay>();

            DateTime first = new DateTime(year,month,1);
            int before = (int)first.DayOfWeek == 0 ? 6 : (int)first.DayOfWeek - 1;
            int days = DateTime.DaysInMonth(year, month) + before;
            days = days % 7 > 0 ? days + (7 - days % 7) : days;

            MySqlConnect msc = new MySqlConnect();
            List<BasicEvent> events = msc.GetEvents(false,"","eventStart");

            first = first.AddDays(-before);

            for (int i = 0; i < days; i++)
            {
                DateTime myDate = first.AddDays(i);
                CalendarDay cd = new CalendarDay
                {
                    Date = myDate,
                    Active = myDate.Month == month,
                    Events = new List<BasicEvent>()
                };
                result.Add(cd);
            }

            foreach (BasicEvent ev in events)
            {
                TimeSpan end = ev.End - first;
                TimeSpan start = ev.Start - first;
                if (start.Days < 0){continue;}

                for (int i = start.Days; i <= end.Days && i < result.Count; i++)
                {
                    result[i].Events.Add(ev);
                }
            }

            return result;
        }

        public ActionResult Day(int year, int month, int day)
        {
            MySqlConnect msc = new MySqlConnect();
            DateTime temp = new DateTime(year, month, day);
            string morning = temp.ToString("yyyy-MM-dd HH:mm:ss");
            string night = temp.AddDays(1).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss");
            string where = "(eventStart <= '" + night + "' AND eventStart >= '" + morning
                            + "') OR (eventEnd <= '" + night + "' AND eventEnd >= '" + morning
                            + "') OR (eventEnd >= '" + night + "' AND eventStart <= '" + morning + "')";
            List<BasicEvent> events = msc.GetEvents(true, where, "eventStart");

            CalendarDay result = new CalendarDay
            {
                Date = new DateTime(year,month,day),
                Rooms = msc.GetRooms(),
                Events = events
            };

            return View(result);
        }

        public ActionResult List()
        {
            MySqlConnect msc = new MySqlConnect();
            List<BasicEvent> events = msc.GetEvents(false, "", "eventStart");

            CalendarList model = new CalendarList
            {
                Events = events,
                Start = DateTime.Now,
                End = DateTime.Now.AddDays(10),
                VCM = null
            };

            return View(model);
        }
    }
}
