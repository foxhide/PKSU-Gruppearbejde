﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CalendarApplication.Models.Event
{
    public class EventEditModel : BasicEvent
    {
        public int CreatorID { get; set; }

        public string SelectedEventType { get; set; }
        public List<SelectListItem> EventTypes { get; set; }

        public List<FieldModel> TypeSpecefics { get; set; }

        public int SubmitType { get; set; }
    }
}