using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using CalendarApplication.Models.EventType;

namespace CalendarApplication.Models.Event
{
    public class FieldModel : FieldDataModel
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
        public bool BoolValue { get; set; }
    }
}