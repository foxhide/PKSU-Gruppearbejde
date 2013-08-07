using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CalendarApplication.Models.Maintenance
{
    public class ManageEventTypeModel
    {
        public List<SelectListItem> ActiveEventTypes { get; set; }
        public List<SelectListItem> InactiveEventTypes { get; set; }
    }
}