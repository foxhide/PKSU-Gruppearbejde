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
using CalendarApplication.Models.Maintenance;
using CalendarApplication.Database;

namespace CalendarApplication.Controllers
{
    public class CalendarController : Controller
    {
        //
        // GET: /Calendar/

        public ActionResult Index(int mode, int year, int month, int day, int range)
        {
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
                Eventtypes = new List<EventTypeModel>()
            };

            MySqlConnect msc = new MySqlConnect();
            string etget = "SELECT eventTypeId, eventTypeName FROM pksudb.eventtypes";
            DataTable dt = msc.ExecuteQuery(etget);
            foreach (DataRow dr in dt.Rows)
            {
                EventTypeModel etm = new EventTypeModel
                {
                    ID = (int)dr["eventTypeId"],
                    Name = (string)dr["eventTypeName"],
                    Selected = true
                };
                evm.Eventtypes.Add(etm);
            }


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
            first = first.AddDays(-before);
            
            List<BasicEvent> events = this.GetEvents(evm, first, first.AddDays(days),false);

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

            first = first.AddHours(Config.GetStartingHourOfDay());
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
            DateTime date = new DateTime(evm.Year, evm.Month, evm.Day);

            List<BasicEvent> events = this.GetEvents(evm, date, date.AddDays(1),true);

            CalendarDay result = new CalendarDay
            {
                Date = date.AddHours(Config.GetStartingHourOfDay()),
                Rooms = new List<Room>(),
                Events = events
            };

            MySqlConnect msc = new MySqlConnect();
            CustomQuery query = new CustomQuery { Cmd = "SELECT * FROM pksudb.rooms" };
            DataTable dt = msc.ExecuteQuery(query);
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    result.Rooms.Add(new Room { ID = (int)dr["roomId"], Name = (string)dr["roomName"] });
                }
            }
            return result;
        }

        private CalendarList GetList(EventViewModel evm)
        {
            DateTime start = new DateTime(evm.Year, evm.Month, evm.Day);
            DateTime end = start.AddDays(evm.Range);

            List<BasicEvent> events = this.GetEvents(evm,start,end,false);

            CalendarList model = new CalendarList
            {
                Events = events,
                Start = start,
                End = end,
            };

            return model;
        }

        /// <summary>
        /// Gets a list of events. evm is the current EventViewModel (filter), start is the starting date,
        /// end is the ending date. User authentication is also applied:
        ///     - If no user, visible events are returned
        ///     - If admin, all events are returned
        ///     - If non-admin user, eventvisibility/editor rights/creator is checked
        /// </summary>
        /// <param name="evm">EventViewModel containing the filter</param>
        /// <param name="start">Starting date</param>
        /// <param name="end">Ending date</param>
        /// <returns>List of events</returns>
        private List<BasicEvent> GetEvents(EventViewModel evm, DateTime start, DateTime end, bool day)
        {
            string select = "SELECT e.eventId,e.userId,u.userName,e.eventTypeId,e.eventName,e.eventStart,"
                            + "e.eventEnd,e.state,e.visible,et.eventTypeName";
            string from = "FROM pksudb.events AS e NATURAL JOIN pksudb.users AS u NATURAL JOIN pksudb.eventtypes AS et";

            // Build the where string
            //   - Input from filter:

            // Sanitize:
            start = start.Date;
            end = end.Date;

            //     Calculate offset
            int offset = Config.GetStartingHourOfDay();
            string morning = start.AddHours(offset).ToString("yyyy-MM-dd HH:00:00");
            string night = end.AddHours(offset).ToString("yyyy-MM-dd HH:00:00");
            string where = "WHERE ((eventStart <= '" + night + "' AND eventStart >= '" + morning
                            + "') OR (eventEnd <= '" + night + "' AND eventEnd >= '" + morning
                            + "') OR (eventEnd >= '" + night + "' AND eventStart <= '" + morning + "'))";

            foreach (EventTypeModel etm in evm.Eventtypes)
            {
                if (!etm.Selected)
                {
                    where += " AND ";
                    where += "(eventTypeId != " + etm.ID + ")";
                }
            }

            where += (evm.ViewState0 ? "" : " AND (state != 0)");
            where += (evm.ViewState1 ? "" : " AND (state != 1)");
            where += (evm.ViewState2 ? "" : " AND (state != 2)");

            // If it is a day, we want the rooms too.
            if (day)
            {
                select += ",r.roomId,r.roomName";
                from += " NATURAL JOIN pksudb.eventroomsused AS eru NATURAL JOIN pksudb.rooms AS r";
            }

            //   User authentication
            UserModel cur = UserModel.GetCurrent();
            if(cur == null)
            {
                // no user -> show visible events
                where += " AND (visible = 1)";
            }
            else if (!cur.Admin)
            {
                // A user, that is not an admin -> get events that are authenticated.
                from += " LEFT JOIN (SELECT eventId,userId"
                        + " FROM eventeditorsusers"
                        + " WHERE userId = " + cur.ID + ") AS edt_user ON e.eventId = edt_user.eventId"
                        + " LEFT JOIN (SELECT eventId,userId"
                        + " FROM eventvisibility NATURAL JOIN groupmembers"
                        + " WHERE userId = " + cur.ID + ") AS vis_group ON e.eventId = vis_group.eventId"
                        + "	LEFT JOIN (SELECT eventId,userId"
                        + " FROM eventeditorsgroups NATURAL JOIN groupmembers"
                        + "	WHERE userId = " + cur.ID + ") AS edt_group ON e.eventId = edt_group.eventId";
                // Add extra fields for view authentication
                select += ",vis_group.userId AS group_vis,edt_group.userId AS group_edt, edt_user.userId AS user_edt";
            }
            //If admin, show all events.

            string query = select + " " + from + " " + where + " ORDER BY e.eventStart";

            List<BasicEvent> events = new List<BasicEvent>();

            MySqlConnect msc = new MySqlConnect();
            DataTable dt = msc.ExecuteQuery(query);
            if (dt != null)
            {
                int r = 0;
                while (r < dt.Rows.Count)
                {
                    DataRow dr = dt.Rows[r];
                    BasicEvent e = new BasicEvent
                    {
                        ID = (int)dr["eventId"],
                        Name = (string)dr["eventName"],
                        CreatorId = (int)dr["userId"],
                        Creator = (string)dr["userName"],
                        Start = (DateTime)dr["eventStart"],
                        End = (DateTime)dr["eventEnd"],
                        State = (int)dr["state"],
                        TypeId = (int)dr["eventTypeId"],
                        TypeName = (string)dr["eventTypeName"],
                        Visible = (bool)dr["visible"],
                    };
                    // If no user and invisible event, do not add
                    if (cur == null && !e.Visible) { continue; }
                    // Set ViewVisble
                    e.ViewVisible = e.Visible || (cur != null &&
                                                   (   cur.Admin 
                                                    || cur.ID == e.CreatorId
                                                    || !(dr["user_edt"] is DBNull)
                                                    || !(dr["group_edt"] is DBNull)
                                                    || !(dr["group_vis"] is DBNull)
                                                   )
                                                 );
                    // If day: get rooms
                    if (day)
                    {
                        e.Rooms = new List<Room>();
                        while (r < dt.Rows.Count && (int)dt.Rows[r]["eventId"] == e.ID)
                        {
                            e.Rooms.Add(new Room { ID = (int)dt.Rows[r]["roomId"], Name = (string)dt.Rows[r]["roomName"] });
                            r++;
                        }
                        events.Add(e);
                        continue;
                    }
                    // If ViewVisible or day, add the event
                    else if (e.ViewVisible)
                    {
                        events.Add(e);
                    }
                    r++;
                }
            }
            else
            {
                TempData["errorMsg"] = msc.ErrorMessage;
            }
            return events;
        }
    }
}
