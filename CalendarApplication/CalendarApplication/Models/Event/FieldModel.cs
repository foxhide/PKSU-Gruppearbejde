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
        public int IntValue { get; set; }
        public string StringValue { get; set; }
        public bool BoolValue { get; set; }
        public DateTime DateValue { get; set; }

        public string GetDBValue()
        {
            switch (this.Datatype)
            {
                case Fieldtype.Integer:
                case Fieldtype.User:
                case Fieldtype.Group: return this.IntValue.ToString(); //int
                case Fieldtype.Text:
                case Fieldtype.File: return "'" + this.StringValue + "'"; //string
                case Fieldtype.Datetime: return "'" + DateValue.ToString("yyyy-MM-dd hh:mm:ss") + "'";
                case Fieldtype.Bool: return (this.BoolValue ? "1" : "0"); //bool
            }
            return "";
        }
    }
}