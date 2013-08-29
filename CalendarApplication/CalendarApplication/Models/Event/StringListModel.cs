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

        // Place (for javascript and html)
        public int Place { get; set; }

        //ID for html and javascript functions
        public string ViewID { get; set; }

        //Name for html and javascript
        public string ViewName { get; set; }
    }
}