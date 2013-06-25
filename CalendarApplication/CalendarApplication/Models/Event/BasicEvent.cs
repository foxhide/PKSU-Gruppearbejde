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

        /// <summary>
        /// Gets the duration for the event as a TimeSpan.
        /// </summary>
        /// <returns>Duration of the event as a TimeSpan</returns>
        public TimeSpan getDuration()
        {
            return End - Start;
        }

        /// <summary>
        /// Gets the color associated with the current state of this event
        /// </summary>
        /// <returns>A string with the name of the color</returns>
        public string getColor()
        {
            if (!this.ViewVisible) { return "grey"; }
            return BasicEvent.getColor(this.State);
        }

        /// <summary>
        /// Gets the state text associated with the current state of this event
        /// </summary>
        /// <returns>A string with the state text</returns>
        public string getStateText()
        {
            return BasicEvent.getStateText(this.State);
        }

        /// <summary>
        /// Gets the color associated with the param state
        /// </summary>
        /// <param name="state">The desired state</param>
        /// <returns>A string with the name of the color</returns>
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

        /// <summary>
        /// Gets the text associated with the param state
        /// </summary>
        /// <param name="state">The desired state</param>
        /// <returns>A string with the text</returns>
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