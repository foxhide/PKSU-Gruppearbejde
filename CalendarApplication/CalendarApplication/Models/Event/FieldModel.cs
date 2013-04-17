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

        public string GetDBValue()
        {
            switch (this.Datatype)
            {
                case 0:
                case 3:
                case 4: return this.IntValue.ToString(); //int
                case 1:
                case 2:
                case 5: return this.StringValue; //string
                case 6: return (this.BoolValue ? "1" : "0"); //bool
            }
            return "";
        }
    }
}