using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CalendarApplication.Models.Event
{
    public class BasicEvent
    {
        // Std. constructor: ViewVisble is enabled. This way event getters don't have to set it
        public BasicEvent()
        {
            this.ViewVisible = true;
            this.Approved = false;
        }

        public int ID { set; get; }

        public string Name { set; get; }

        public string Creator { set; get; }
        public int CreatorId { set; get; }
        public int TypeId { set; get; }
        public string TypeName { set; get; }

        [Display(Name = "Starting date")]
        public DateTime Start { set; get; }
        [Display(Name = "Ending date")]
        public DateTime End { set; get; }

        public int State { set; get; }
        public bool Visible { set; get; }
        public List<Room> Rooms { set; get; }

        public bool Approved { set; get; }

        // Used in CalendarView to display grey events
        public bool ViewVisible { set; get; }

        public TimeSpan getDuration()
        {
            return End - Start;
        }

        public string getColor()
        {
            if (!this.ViewVisible) { return "lightgrey"; }
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