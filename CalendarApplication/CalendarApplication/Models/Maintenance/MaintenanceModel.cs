using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CalendarApplication.Models.Maintenance
{
    public class MaintenanceModel
    {
        public int SubmitValue { get; set; }

        public string SelectedEventType { get; set; }
        public List<SelectListItem> EventTypes { get; set; }

        public string SelectedGroup { get; set; }
        public List<SelectListItem> Groups { get; set; }

        public string SelectedRoom { get; set; }
        public List<SelectListItem> Rooms { get; set; }

    }
}