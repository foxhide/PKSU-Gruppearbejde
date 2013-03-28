using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CalendarApplication.Models.Event
{
    public class BasicEvent
    {
        public int ID { set; get; }
        public string Name { set; get; }
        public string Creator { set; get; }
        public int TypeId { set; get; }
        public string TypeName { set; get; }
        public DateTime Start { set; get; }
        public DateTime End { set; get; }
        public int State { set; get; }
        public bool Visible { set; get; }
        public List<Room> Rooms { set; get; }

        public TimeSpan getDuration()
        {
            return End - Start;
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
                case 0: return "Needs Approval";
                case 1: return "Approved";
                case 2: return "Finished";
                default: return "NYI";
            }
        }
    }
}