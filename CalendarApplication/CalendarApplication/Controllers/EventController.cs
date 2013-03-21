using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CalendarApplication.Models.Event;

namespace CalendarApplication.Controllers
{
    public class EventController : Controller
    {
        //
        // GET: /Event/

        public ActionResult Index(int id)
        {
            BasicEvent result = new BasicEvent
            {
                ID = id
            };
            return View(result);
        }

    }
}
