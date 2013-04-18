using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using CalendarApplication.Models.EventType;
using CalendarApplication.Models.Shared;

namespace CalendarApplication.Models.Event
{
    public class FieldModel : FieldDataModel
    {
        public object Value { get; set; }
        //public EditableDateTime DateValue { get; set; } // Needed because a complex type cannot be saved into object form view...

        public string GetDBValue()
        {
            switch (this.Datatype)
            {
                case Fieldtype.Integer:
                case Fieldtype.User:
                case Fieldtype.Group: return ((int)this.Value).ToString(); //int
                case Fieldtype.Text:
                case Fieldtype.File: return "'" + (string)this.Value + "'"; //string
                case Fieldtype.Datetime: return "'" +new EditableDateTime(DateTime.Now).ToString()+"'";//"'" + this.DateValue.GetDBString() + "'";
                case Fieldtype.Bool: return ((bool)this.Value ? "1" : "0"); //bool
            }
            return "";
        }
    }
}