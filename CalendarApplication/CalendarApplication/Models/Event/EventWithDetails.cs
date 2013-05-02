using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace CalendarApplication.Models.Event
{
    public class EventWithDetails : BasicEvent
    {
        public List<FieldModel> TypeSpecifics { get; set; }
    }
}