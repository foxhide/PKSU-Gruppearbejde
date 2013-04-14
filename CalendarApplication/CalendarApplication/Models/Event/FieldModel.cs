using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using CalendarApplication.Models.EventType;

namespace CalendarApplication.Models.Event
{
    public class FieldModel : FieldDataModel
    {
        public object Value { get; set; }
    }
}