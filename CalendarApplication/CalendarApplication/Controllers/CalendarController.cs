﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Text.RegularExpressions;

using CalendarApplication.Models.Calendar;
using CalendarApplication.Models.Event;
using CalendarApplication.Models.User;
using CalendarApplication.Models.EventType;
using CalendarApplication.Models.Maintenance;
using CalendarApplication.Database;
using System.Text;

namespace CalendarApplication.Controllers
{
    public class CalendarController : Controller
    {
        //
        // GET: /Calendar/

        public ActionResult Index()
        {
            //Simply redirect to month
            return RedirectToAction("Month");
        }

        /// <summary>
        /// Gets the monthly view page
        /// </summary>
        /// <param name="date">Date in the desired month, if null -> today</param>
        /// <param name="state">State string, e.g. 111 or 010, if null -> all visible</param>
        /// <param name="types">Type string, e.g. 101101..., if null -> all visible</param>
        /// <param name="rooms">Room string, e.g. 1000111..., if null -> all visible</param>
        /// <returns>The monthly view page</returns>
        public ActionResult Month(string date, string state, string types, string rooms)
        {
            EventFilter f = this.GetFilter(state, types, rooms);
            DateTime dt = this.parseString(date);
            CalendarMonth cm = this.GetMonth(dt, f);
            cm.Filter = f;
            cm.Mode = CalendarMode.MONTH;
            return View(cm);
        }

        /// <summary>
        /// Post function for the monthly view
        /// </summary>
        /// <param name="cm">CalendarMonth model, with the posted data</param>
        /// <returns>A redirect to the month page</returns>
        [HttpPost]
        public ActionResult Month(CalendarMonth cm)
        {
            // Create string date
            string date = cm.Mode == CalendarMode.MONTH ? cm.Date.ToString("yyyy-MM") : cm.Date.ToString("yyyy-MM-dd");

            // Create state string
            string state = cm.Filter.GetStateString();

            // Create type
            string types = cm.Filter.GetTypeString();

            // Create rooms
            string rooms = cm.Filter.GetRoomString();

            // Determine action
            string action = cm.Mode == CalendarMode.MONTH ? "Month" : "Day";

            return RedirectToAction(action, new { date = date, state = state, types = types, rooms = rooms });
        }

        /// <summary>
        /// Gets the daily view page
        /// </summary>
        /// <param name="date">The desired date, today if null</param>
        /// <param name="state">State string, e.g. 111 or 010, if null -> all visible</param>
        /// <param name="types">Type string, e.g. 101101..., if null -> all visible</param>
        /// <param name="rooms">Room string, e.g. 1000111..., if null -> all visible</param>
        /// <returns>The daily view page</returns>
        public ActionResult Day(string date, string state, string types, string rooms)
        {
            EventFilter f = this.GetFilter(state, types, rooms);
            DateTime dt = this.parseString(date);
            CalendarDay cd = this.GetDay(dt, f);
            cd.Filter = f;
            cd.Mode = CalendarMode.DAY;
            return View(cd);
        }

        /// <summary>
        /// Post function for the daily view
        /// </summary>
        /// <param name="cd">CalendarDay model, with the posted data</param>
        /// <returns>A redirect to the daily page</returns>
        [HttpPost]
        public ActionResult Day(CalendarDay cd)
        {
            // Create string date
            string date = cd.Mode == CalendarMode.MONTH ? cd.Date.ToString("yyyy-MM") : cd.Date.ToString("yyyy-MM-dd");

            // Create state string
            string state = cd.Filter.GetStateString();

            // Create type
            string types = cd.Filter.GetTypeString();

            // Create rooms
            string rooms = cd.Filter.GetRoomString();

            // Determine action
            string action = cd.Mode == CalendarMode.MONTH ? "Month" : "Day";

            return RedirectToAction(action, new { date = date, state = state, types = types, rooms = rooms });
        }

        /// <summary>
        /// Gets the list view page
        /// </summary>
        /// <param name="from">The from-date, if null -> 0001-01-01 is chosen</param>
        /// <param name="to">The to-date, if null -> 9999-01-01 is chosen</param>
        /// <param name="limit">The limit pr. page, if null -> 10 is chosen</param>
        /// <param name="efrom">The starting event, if null -> 0 is chosen. efrom is rounded down to a multiplum of limit</param>
        /// <param name="state">State string, e.g. 111 or 010, if null -> all visible</param>
        /// <param name="types">Type string, e.g. 101101..., if null -> all visible</param>
        /// <param name="rooms">Room string, e.g. 1000111..., if null -> all visible</param>
        /// <param name="order">Determines the sorting of the events, if null -> sort by start date</param>
        /// <param name="desc">Detemines desc/asc, if true the descending, else ascending.</param>
        /// <returns>The list view page</returns>
        public ActionResult List(string from,
                                 string to,
                                 string limit,
                                 string efrom,
                                 string state,
                                 string types,
                                 string rooms,
                                 string order,
                                 string desc)
        {
            // Get filter and dates
            EventFilter f = this.GetFilter(state, types, rooms);
            DateTime dtFrom = from == null ? new DateTime(1,1,1) : this.parseString(from);
            DateTime dtTo = to == null ? new DateTime(9999,12,31) : this.parseString(to);
            
            // Get limit and efrom
            int limitInt = limit == null ? 10 : Convert.ToInt32(limit);
            int efromInt = efrom == null ? 0 : Convert.ToInt32(efrom);

            // Get event order and descending
            EventOrder eo = EventOrder.START;
            if (order != null && Enum.IsDefined(typeof(EventOrder), order))
            {
                eo = (EventOrder)Enum.Parse(typeof(EventOrder), order, true);
            }
            bool descend = desc != null && desc.ToLower().Equals("true");

            CalendarList cl = this.GetList(dtFrom, dtTo, f, limitInt, efromInt, eo, descend);

            if (from == null && to == null)
            {
                // Display some nice dates to the user
                cl.All = true;
                cl.Start = DateTime.Today;
                cl.End = cl.Start.AddDays(10);
            }

            cl.Filter = f;
            cl.Mode = CalendarMode.LIST;
            cl.Limit = limitInt;
            cl.OldLimit = limitInt;
            cl.EventFrom = efromInt;
            cl.Order = eo;
            cl.Descending = descend;

            return View(cl);
        }

        /// <summary>
        /// Post function for the list view
        /// </summary>
        /// <param name="cd">CalendarList model, with the posted data</param>
        /// <returns>A redirect to the list page</returns>
        [HttpPost]
        public ActionResult List(CalendarList cl)
        {
            // Get state string
            string state = cl.Filter.GetStateString();

            // Get type string
            string types = cl.Filter.GetTypeString();

            // Create rooms
            string rooms = cl.Filter.GetRoomString();

            // If limit had changed -> set from to 0, else set from accordingly
            int from = cl.Limit != cl.OldLimit ? 0 : cl.EventFrom - (cl.EventFrom % cl.Limit);

            if (cl.All)
            {
                // If all, do not send the dates
                return RedirectToAction("List", new
                {
                    limit = cl.Limit.ToString(),
                    efrom = from.ToString(),
                    state = state,
                    types = types,
                    rooms = rooms,
                    order = cl.Order,
                    desc = cl.Descending.ToString()
                });
            }
            else
            {
                // Else, return the dates too
                return RedirectToAction("List", new
                {
                    from = cl.Start.ToString("yyyy-MM-dd"),
                    to = cl.End.ToString("yyyy-MM-dd"),
                    limit = cl.Limit.ToString(),
                    efrom = from.ToString(),
                    state = state,
                    types = types,
                    rooms = rooms,
                    order = cl.Order,
                    desc = cl.Descending.ToString()
                });
            }
        }

        /// <summary>
        /// Function for parsing string to DateTime
        /// </summary>
        /// <param name="date">The date as a string, format: yyyy-MM-dd</param>
        /// <returns>DateTime.Now on invalid input, else the DateTime represented by the date string</returns>
        private DateTime parseString(string date)
        {
            // Check null
            if(string.IsNullOrEmpty(date)) { return DateTime.Today; }

            // Regex check
            Match m = Regex.Match(date, @"[0-9]{4}-[0-9]{2}(-[0-9]{2})?");
            if (!m.Success) { TempData["errorMsg"] = "Bad date string!"; return DateTime.Today; }

            string[] vals = date.Split('-');
            int year = Convert.ToInt32(vals[0]);
            int month = Convert.ToInt32(vals[1]);
            int day = vals.Length > 2 ? Convert.ToInt32(vals[2]) : 1;

            DateTime result = DateTime.Today;
            // Try to create the dateTime (checks if valid date)
            try
            {
                result = new DateTime(year, month, day);
            }
            // Date was not valid
            catch (Exception ex) { TempData["errorMsg"] = ex.Message; }

            // Date was valid -> return it.
            return result;
        }

        /// <summary>
        /// Creates an EventFilter from the state string and type string
        /// </summary>
        /// <param name="state">State string</param>
        /// <param name="types">Type string</param>
        /// <returns>An EventFilter</returns>
        private EventFilter GetFilter(string state, string types, string rooms)
        {
            EventFilter f = new EventFilter
            {
                ViewState0 = true,
                ViewState1 = true,
                ViewState2 = true,
                Eventtypes = new List<EventTypeModel>(),
                Rooms = new List<SelectListItem>()
            };

            // Set state values
            if (state != null)
            {
                char[] stateArr = state.ToCharArray();
                f.ViewState0 = stateArr.Length < 1 || stateArr[0] == '1';
                f.ViewState1 = stateArr.Length < 2 || stateArr[1] == '1';
                f.ViewState2 = stateArr.Length < 3 || stateArr[2] == '1';
            }

            // Get event types
            MySqlConnect msc = new MySqlConnect();
            string etget = "SELECT eventTypeId, eventTypeName FROM eventtypes";
            object[] argval = { };
            string[] argnam = { };
            CustomQuery query = new CustomQuery { Cmd = etget, ArgNames = argnam, Args = argval };
            DataTable dt = msc.ExecuteQuery(query);

            char[] typeArr = types != null ? types.ToCharArray() : null;
            int tCount = 0;

            // Fill in event types and set their values
            foreach (DataRow dr in dt.Rows)
            {
                EventTypeModel etm = new EventTypeModel
                {
                    ID = (int)dr["eventTypeId"],
                    Name = (string)dr["eventTypeName"],
                    Selected = typeArr != null && tCount < typeArr.Length ? typeArr[tCount] == '1' : true
                };
                tCount++;
                f.Eventtypes.Add(etm);
            }

            // Add rooms to the filter
            query.Cmd = "SELECT roomId,roomName FROM rooms";
            dt = msc.ExecuteQuery(query);

            char[] roomArr = rooms != null ? rooms.ToCharArray() : null;
            int rCount = 0;

            if (dt != null)
            {
                foreach(DataRow dr in dt.Rows)
                {
                    SelectListItem room = new SelectListItem
                    {
                        Text = (string)dr["roomName"],
                        Value = ((int)dr["roomId"]).ToString(),
                        Selected = roomArr != null && rCount < roomArr.Length ? roomArr[rCount] == '1' : true
                    };
                    rCount++;
                    f.Rooms.Add(room);
                }
            }

            return f;
        }

        /// <summary>
        /// Creates a CalendarMonth
        /// </summary>
        /// <param name="dt">Date in the desired month</param>
        /// <param name="f">The current filter</param>
        /// <returns>A CalendarMonth with the right number of days for the view</returns>
        private CalendarMonth GetMonth(DateTime dt, EventFilter f)
        {
            List<CalendarDay> cdays = new List<CalendarDay>();

            // Calculate the needed number of days.
            DateTime first = new DateTime(dt.Year,dt.Month,1);
            int before = (int)first.DayOfWeek == 0 ? 6 : (int)first.DayOfWeek - 1;
            int days = DateTime.DaysInMonth(dt.Year, dt.Month) + before;
            days = days % 7 > 0 ? days + (7 - days % 7) : days;
            first = first.AddDays(-before);
            
            // Get all events between the calculated days
            List<BasicEvent> events = this.GetEvents(f, first, first.AddDays(days),CalendarMode.MONTH,EventOrder.START, false);

            // Create calendar days
            for (int i = 0; i < days; i++)
            {
                DateTime myDate = first.AddDays(i);
                CalendarDay cd = new CalendarDay
                {
                    Date = myDate,
                    Active = myDate.Month == dt.Month,
                    Events = new List<BasicEvent>()
                };
                cdays.Add(cd);
            }

            // Apply offset
            first = first.AddHours(Config.GetStartingHourOfDay());

            // Add event to its days
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

            return new CalendarMonth { Date = dt, Days = cdays };
        }

        /// <summary>
        /// Gets a day model for the given date
        /// </summary>
        /// <param name="date">The date to get</param>
        /// <param name="f">The current filter</param>
        /// <returns>A CalendarDay</returns>
        private CalendarDay GetDay(DateTime date, EventFilter f)
        {
            // Get events
            List<BasicEvent> events = this.GetEvents(f, date, date.AddDays(1),CalendarMode.DAY,EventOrder.START, false);

            CalendarDay result = new CalendarDay
            {
                Date = date.AddHours(Config.GetStartingHourOfDay()),
                Events = events
            };

            return result;
        }

        /// <summary>
        /// Get the CalendarList model
        /// </summary>
        /// <param name="start">The from-date</param>
        /// <param name="end">The to-date</param>
        /// <param name="f">The current filter</param>
        /// <param name="limit">The limit</param>
        /// <param name="starting">The starting event</param>
        /// <param name="o">Determines the sorting</param>
        /// <param name="desc">Determines if sorting should be ascending or descending (true -> descending)</param>
        /// <returns></returns>
        private CalendarList GetList(DateTime start,
                                     DateTime end,
                                     EventFilter f,
                                     int limit,
                                     int starting,
                                     EventOrder o,
                                     bool desc)
        {
            // Get all events between the dates
            List<BasicEvent> eventsFull = this.GetEvents(f,start,end,CalendarMode.LIST,o,desc);

            // Sanity checks of starting and limit (limit must be one of the predefined)
            starting = starting < 0 || starting > eventsFull.Count ? 0 : starting;
            limit = CalendarList.LIMITS.Contains(limit) ? limit : CalendarList.LIMITS[0];

            // Limit the number of events.
            List<BasicEvent> events = new List<BasicEvent>();
            for (int i = 0; i < limit && starting+i < eventsFull.Count; i++)
            {
                events.Add(eventsFull[starting+i]);
            }

            CalendarList model = new CalendarList
            {
                Events = events,
                Start = start,
                End = end,
                TotalEventCount = eventsFull.Count
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
        /// <param name="f">EventFilter</param>
        /// <param name="start">Starting date</param>
        /// <param name="end">Ending date</param>
        /// <param name="day">If true, rooms will be filled too and invisible events will be added</param>
        /// <param name="order">Determines the sorting order</param>
        /// <param name="desc">Determines ascending or descending order (true -> descending)</param>
        /// <returns>List of events</returns>
        private List<BasicEvent> GetEvents(EventFilter f, DateTime start, DateTime end, CalendarMode mode, EventOrder order, bool desc)
        {
            StringBuilder select = new StringBuilder();
            StringBuilder from = new StringBuilder();
            StringBuilder where = new StringBuilder();

            select.Append("SELECT e.eventId,e.userId,e.creation,u.firstName,u.lastName,e.eventTypeId,e.eventName,e.eventStart,");
            select.Append("e.eventEnd,e.state,e.visible,et.eventTypeName,r.roomId,r.roomName");
            from.Append("FROM events AS e JOIN users AS u ON e.userId = u.userId JOIN eventtypes AS et ON e.eventTypeId = et.eventTypeId");
            from.Append(" NATURAL JOIN eventroomsused AS eru NATURAL JOIN rooms AS r");

            List<string> argNames = new List<String>();
            List<object> args = new List<Object>();

            // Build the where string
            //   - Input from filter:

            // Sanitize:
            start = start.Date;
            end = end.Date;

            //     Calculate offset
            int offset = Config.GetStartingHourOfDay();
            start = start.AddHours(offset);
            end = end.AddHours(offset);

            //     Add the date limits (with offset)
            where.Append("WHERE ((eventStart <= @end AND eventStart >= @start)");
            where.Append(" OR (eventEnd <= @end AND eventEnd >= @start)");
            where.Append(" OR (eventEnd >= @end AND eventStart <= @start))");

            argNames.Add("@end");
            args.Add(end);
            argNames.Add("@start");
            args.Add(start);

            //    Event types
            foreach (EventTypeModel etm in f.Eventtypes)
            {
                if (!etm.Selected)
                {
                    string arg = "@etm_" + etm.ID;
                    where.Append(" AND (eventTypeId != " + arg + ")");
                    argNames.Add(arg);
                    args.Add(etm.ID);
                }
            }

            //    Rooms
            foreach (SelectListItem room in f.Rooms)
            {
                if (!room.Selected)
                {
                    string arg = "@room_" + room.Value;
                    where.Append(" AND (roomId != " + arg + ")");
                    argNames.Add(arg);
                    args.Add(Convert.ToInt32(room.Value));
                }
            }

            //   States
            where.Append((f.ViewState0 ? "" : " AND (state != 0)"));
            where.Append((f.ViewState1 ? "" : " AND (state != 1)"));
            where.Append((f.ViewState2 ? "" : " AND (state != 2)"));

            //   User authentication
            UserModel cur = UserModel.GetCurrent();
            if(cur == null)
            {
                // no user -> show visible events
                where.Append(" AND (visible = 1)");
            }
            else if (!cur.Admin)
            {
                // A user, that is not an admin -> get events that are authenticated.
                from.Append(" LEFT JOIN (SELECT eventId,userId");
                from.Append(" FROM eventeditorsusers");
                from.Append(" WHERE userId = @uid) AS edt_user ON e.eventId = edt_user.eventId");
                from.Append(" LEFT JOIN (SELECT eventId,userId");
                from.Append(" FROM eventvisibility NATURAL JOIN groupmembers");
                from.Append(" WHERE userId = @uid) AS vis_group ON e.eventId = vis_group.eventId");
                from.Append("	LEFT JOIN (SELECT eventId,userId");
                from.Append(" FROM eventeditorsgroups NATURAL JOIN groupmembers");
                from.Append("	WHERE userId = @uid) AS edt_group ON e.eventId = edt_group.eventId");
                // Add extra fields for view authentication
                select.Append(",vis_group.userId AS group_vis,edt_group.userId AS group_edt, edt_user.userId AS user_edt");

                argNames.Add("@uid");
                args.Add(cur.ID);
            }
            //If admin, show all events.

            // Combine the select, from and where
            select.Append(" ");
            select.Append(from);
            select.Append(" ");
            select.Append(where);

            // If monthly view, group by eventId to remove room-duplicates.
            if (mode == CalendarMode.MONTH) { select.Append(" GROUP BY e.eventId"); }

            // Add ORDER BY
            select.Append(" ORDER BY ");
            switch (order)
            {
                case EventOrder.START: select.Append("e.eventStart"); break;
                case EventOrder.END: select.Append("e.eventEnd"); break;
                case EventOrder.NAME: select.Append("e.eventName"); break;
                case EventOrder.TYPE: select.Append("et.eventTypeName"); break;
                case EventOrder.STATE: select.Append("e.state"); break;
                case EventOrder.CREATOR: select.Append("u.lastName"); break;
                case EventOrder.CREATIONDATE: select.Append("e.creation"); break;
                default: select.Append("e.eventStart"); break;
            }

            // Check for descending
            if (desc) { select.Append(" DESC"); }

            // Apply second ordering to make sure that duplicate (several rooms) entries for the same event are grouped together.
            select.Append(", e.eventId");
            
            // If list view, order by rooms too (to make the room appear ordered in the column)
            if (mode == CalendarMode.LIST) { select.Append(", r.roomName"); }

            List<BasicEvent> events = new List<BasicEvent>();

            MySqlConnect msc = new MySqlConnect();
            CustomQuery calquery = new CustomQuery { Cmd = select.ToString(), ArgNames = argNames.ToArray() , Args = args.ToArray() };
            
            DataTable dt = msc.ExecuteQuery(calquery);
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
                        Creator = (string)dr["firstName"] + " " + (string)dr["lastName"],
                        CreationDate = dr["creation"] is DBNull ? new DateTime() : (DateTime)dr["creation"],
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
                                                   (cur.Admin
                                                    || cur.ID == e.CreatorId
                                                    || !(dr["user_edt"] is DBNull)
                                                    || !(dr["group_edt"] is DBNull)
                                                    || !(dr["group_vis"] is DBNull)
                                                   )
                                                 );

                    // If day or list view (and visible) get the rooms and add event
                    if (mode == CalendarMode.DAY || (mode == CalendarMode.LIST && e.ViewVisible))
                    {
                        e.Rooms = new List<Room>();
                        while (r < dt.Rows.Count && (int)dt.Rows[r]["eventId"] == e.ID)
                        {
                            e.Rooms.Add(new Room { ID = (int)dt.Rows[r]["roomId"], Name = (string)dt.Rows[r]["roomName"] });
                            r++;
                        }
                        events.Add(e);
                    }
                    // If list view but not ViewVisible -> we have to run through all room duplicate entries
                    else if (mode == CalendarMode.LIST)
                    {
                        for (; r < dt.Rows.Count && (int)dt.Rows[r]["eventId"] == e.ID; r++) ;
                    }
                    // if month and visible, simply add the event
                    else if (mode == CalendarMode.MONTH && e.ViewVisible)
                    {
                        events.Add(e);
                        r++;
                    }
                    // Else simply increment counter - we are in monthly view, so there should be no room duplicates because of group by
                    else
                    {
                        r++;
                    }
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
