using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CalendarApplication.Models.EventType;

namespace CalendarApplication.Controllers
{
    public class EventTypeController : Controller
    {
        //
        // GET: /EventType/

        public ActionResult EditEventType()
        {
            EventTypeModel etm = new EventTypeModel
            {
                ID = -1,
                Name = "<Event type name here>",
                TypeSpecific = new List<FieldDataModel>()
            };
            return View(etm);
        }

        public ActionResult AddField()
        {

        }

    }
}
