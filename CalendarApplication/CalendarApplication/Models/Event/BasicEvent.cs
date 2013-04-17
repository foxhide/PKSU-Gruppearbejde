using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using CalendarApplication.Models.Shared;

namespace CalendarApplication.Models.Event
{
    public class BasicEvent
    {
        public int ID { set; get; }
        public string Name { set; get; }
        public string Creator { set; get; }
        public int TypeId { set; get; }
        public string TypeName { set; get; }

        [Display(Name = "Starting date")]
        public EditableDateTime Start { set; get; }
        [Display(Name = "Ending date")]
        public EditableDateTime End { set; get; }

        public int State { set; get; }
        public bool Visible { set; get; }
        public List<Room> Rooms { set; get; }

        public TimeSpan getDuration()
        {
            return End.GetDateTime() - Start.GetDateTime();
        }

        public string getColor()
        {
            return BasicEvent.getColor(this.State);
        }

        public string getStateText()
        {
            return BasicEvent.getStateText(this.State);
        }

        public static string getColor(int state)
        {
            switch (state)
            {
                case 0: return "red";
                case 1: return "yellow";
                case 2: return "#40FF00";
                default: return "white";
            }
        }

        public static string getStateText(int state)
        {
            switch (state)
            {
                case 0: return "Incomplete";
                case 1: return "Approved";
                case 2: return "Finished";
                default: return "NYI";
            }
        }
    }
}