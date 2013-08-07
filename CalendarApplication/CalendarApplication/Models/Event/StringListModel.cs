using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CalendarApplication.Models.Event
{
    public class StringListModel
    {
        public string Text { get; set; }

        public int ID { get; set; }

        public bool Active { get; set; }

        public int Place { get; set; }

        public string ViewID { get; set; }

        public string ViewName { get; set; }
    }
}