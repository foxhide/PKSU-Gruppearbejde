﻿using System;
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
        public string StringValue { get; set; }
        public bool BoolValue { get; set; }
        public DateTime DateValue { get; set; }

        public List<SelectListItem> List { get; set; }

        public object GetDBValue()
        {
            switch (this.Datatype)
            {
                case Fieldtype.Integer: return this.IntValue;
                case Fieldtype.User: return this.IntValue; //int, userId
                case Fieldtype.Group: return this.IntValue; //int, groupId
                case Fieldtype.Text: return this.StringValue;
                case Fieldtype.File: return this.IntValue; //int, fileId
                case Fieldtype.Datetime: return DateValue;
                case Fieldtype.Bool: return this.BoolValue; //bool
            }
            return "";
        }
    }
}