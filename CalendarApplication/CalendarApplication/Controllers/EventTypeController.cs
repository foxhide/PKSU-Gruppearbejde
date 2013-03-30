using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;

using CalendarApplication.Models.EventType;
using CalendarApplication.Controllers;

namespace CalendarApplication.Controllers
{
    public class EventTypeController : Controller
    {
        public static int MAX_NUMBER_OF_FIELDS = 100;

        //
        // GET: /EventType/

        public ActionResult EditEventType(int id)
        {
            EventTypeModel etm = new EventTypeModel { TypeSpecific = new List<FieldDataModel>() };
            if (id == -1)
            {
                etm.ID = -1;
                etm.Name = "Event type name here";
            }
            else
            {
                MySqlConnect msc = new MySqlConnect();
                DataTable dt = msc.ExecuteQuery("SELECT * FROM (eventtypes NATURAL JOIN eventtypefields) WHERE eventTypeId = " + id);
                etm.ID = id;
                etm.Name = (string)dt.Rows[0]["eventTypeName"];
                foreach (DataRow dr in dt.Rows)
                {
                    FieldDataModel fdm = new FieldDataModel
                    {
                        ID = etm.TypeSpecific.Count,
                        Name = (string)dr["fieldName"],
                        Description = (string)dr["fieldDescription"],
                        Required = (bool)dr["requiredField"],
                        Datatype = (int)dr["fieldType"]
                    };
                    etm.TypeSpecific.Add(fdm);
                }
            }


            for (int i = etm.TypeSpecific.Count; i < MAX_NUMBER_OF_FIELDS; i++)
            {
                etm.TypeSpecific.Add(new FieldDataModel());
            }
            return View(etm);
        }

        [HttpPost]
        public ActionResult EditEventType(EventTypeModel etm)
        {
            return RedirectToAction("Index","Home",null);
        }

        public ActionResult GetPartial(int id)
        {
            return PartialView("FieldDetails",FieldDataModel.GetEmptyModel(id));
        }

    }
}
