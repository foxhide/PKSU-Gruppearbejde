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
        public string Type { set; get; }
        public DateTime Start { set; get; }
        public DateTime End { set; get; }
        public int State { set; get; }
        public int Visible { set; get; }
        public List<Room> Rooms { set; get; }

        public TimeSpan getDuration()
        {
            return End - Start;
        }

        public string getColor()
        {
            switch (State)
            {
                case 0:     return "red";
                case 1:     return "yellow";
                case 2:     return "green";
                default:    return "white";
            }
        }
    }
}