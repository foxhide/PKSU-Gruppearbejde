﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CalendarApplication.Models.Event
{
    public class EventEditModel : BasicEvent
    {
        [Display(Name = "Event type")]
        public string SelectedEventType { get; set; }
        public List<SelectListItem> EventTypes { get; set; }

        [Display(Name = "Rooms")]
        public List<SelectListItem> RoomSelectList { get; set; }

        public List<SelectListItem> UserEditorList { get; set; }
        public List<SelectListItem> GroupEditorList { get; set; }

        public List<SelectListItem> GroupVisibleList { get; set; }

        public List<FieldModel> TypeSpecifics { get; set; }
        public bool CanChangeType { get; set; }

        public bool DeleteFiles { get; set; }
    }
}