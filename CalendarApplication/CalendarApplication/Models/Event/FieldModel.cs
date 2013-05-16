using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CalendarApplication.Models.EventType;

namespace CalendarApplication.Models.Event
{
    public class FieldModel : FieldDataModel
    {
        public int IntValue { get; set; }
        public float FloatValue { get; set; }
        public string StringValue { get; set; }
        public bool BoolValue { get; set; }
        public DateTime DateValue { get; set; }

        public List<SelectListItem> List { get; set; }

        public object GetDBValue()
        {
            switch (this.Datatype)
            {
                case Fieldtype.Float: return this.FloatValue;
                case Fieldtype.User: if (this.IntValue < 1) { return null; } else { return this.IntValue; } //int or null, userId
                case Fieldtype.Group: if (this.IntValue < 1) { return null; } else { return this.IntValue; } //int or null, groupId
                case Fieldtype.Text: return this.StringValue;
                case Fieldtype.File: if (this.IntValue < 1) { return null; } else { return this.IntValue; } //int or null, fileId
                case Fieldtype.Datetime: return this.DateValue;
                case Fieldtype.Bool: return this.BoolValue; //bool
                case Fieldtype.FileList:
                case Fieldtype.GroupList:
                case Fieldtype.UserList: return false;
            }
            return "";
        }
    }
}