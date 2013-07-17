using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using CalendarApplication.Models.EventType;
using System.Web.Mvc;

namespace CalendarApplication.Models.Calendar
{
    public class EventFilter
    {
        public bool ViewState0 { set; get; }
        public bool ViewState1 { set; get; }
        public bool ViewState2 { set; get; }

        public List<EventTypeModel> Eventtypes { set; get; }

        public List<SelectListItem> Rooms { set; get; }

        /// <summary>
        /// Get the state string
        /// </summary>
        /// <returns>A string indicating the selected values of the states</returns>
        public string GetStateString()
        {
            return (this.ViewState0 ? "1" : "0") + (this.ViewState1 ? "1" : "0") + (this.ViewState2 ? "1" : "0");
        }

        /// <summary>
        /// Get the type string
        /// </summary>
        /// <returns>A string indicating the selected values of the types</returns>
        public string GetTypeString()
        {
            string types = "";
            if (this.Eventtypes != null)
            {
                foreach (EventTypeModel etm in this.Eventtypes)
                {
                    types += etm.Selected ? "1" : "0";
                }
            }
            return types;
        }

        /// <summary>
        /// Get the room string
        /// </summary>
        /// <returns>A string indicating the selected values of the rooms</returns>
        public string GetRoomString()
        {
            string rooms = "";
            if (this.Rooms != null)
            {
                foreach (SelectListItem r in this.Rooms)
                {
                    rooms += r.Selected ? "1" : "0";
                }
            }
            return rooms;
        }
    }
}