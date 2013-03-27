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

        public ActionResult Index(int mode, int year, int month, int day)
        {
            EventViewModel evm = new EventViewModel
            {
                Mode = mode,
                Day = day,
                Month = month,
                Year = year,
                GroupsAvailable = new List<GroupModel> {
                    new GroupModel { ID = 0, Name = "MyGroup", Selected = false },
                    new GroupModel { ID = 1, Name = "OtherGroup", Selected = true },
                },
                GroupsSelected = new List<GroupModel>()
            };
            //evm.GroupsAvailable.Add(new GroupModel { ID = 0, Name = "MyGroup", Selected = false });
            //evm.GroupsAvailable.Add(new GroupModel { ID = 1, Name = "OtherGroup", Selected = true });

            switch (mode)
            {
                case 0:     evm.CurrentModel = GetMonth(year,month);        break;
                case 1:     evm.CurrentModel = GetDay(year, month, day);    break;
                case 2:     evm.CurrentModel = GetList();                   break;
            }

            //evm.MSL = new MultiSelectList(evm.GroupsAvailable, "ID", "Name", evm.GroupsSelected.Select(x => x.ID));

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
                case 0: evm.CurrentModel = GetMonth(evm.Year, evm.Month); break;
                case 1: evm.CurrentModel = GetDay(evm.Year, evm.Month, evm.Day); break;
                case 2: evm.CurrentModel = GetList(); break;
            }


            /*evm.GroupsAvailable = new List<GroupModel>();
            evm.GroupsAvailable.Add(new GroupModel { ID = 0, Name = "MyGroup" });
            evm.GroupsAvailable.Add(new GroupModel { ID = 1, Name = "OtherGroup" });

            evm.GroupsSelected = new List<GroupModel>();*/

            //evm.MSL = new MultiSelectList(evm.GroupsAvailable, "ID", "Name", evm.GroupsSelected.Select(x => x.ID));

            return View(evm);
        }

        private CalendarMonth GetMonth(int year, int month)
        {
            DateTime date = DateTime.Today;
            if (year != 0 && month != 0)
            {
                date = new DateTime(year, month, 1);
            }

            List<CalendarDay> cdays = new List<CalendarDay>();

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
                cdays.Add(cd);
            }

            foreach (BasicEvent ev in events)
            {
                TimeSpan end = ev.End - first;
                TimeSpan start = ev.Start - first;
                if (start.Days < 0){continue;}

                for (int i = start.Days; i <= end.Days && i < cdays.Count; i++)
                {
                    cdays[i].Events.Add(ev);
                }
            }

            return new CalendarMonth { Date = date, Days = cdays };
        }

        public CalendarDay GetDay(int year, int month, int day)
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

            return result;
        }

        public CalendarList GetList()
        {
            MySqlConnect msc = new MySqlConnect();
            List<BasicEvent> events = msc.GetEvents(false, "", "eventStart");

            CalendarList model = new CalendarList
            {
                Events = events,
                Start = DateTime.Now,
                End = DateTime.Now.AddDays(10),
            };

            return model;
        }
    }
}
