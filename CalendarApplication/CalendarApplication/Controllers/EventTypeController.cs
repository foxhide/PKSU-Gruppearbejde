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
                etm.ActiveFields = 0;
                etm.Name = (string)dt.Rows[0]["eventTypeName"];
                etm.ActiveFields = dt.Rows.Count;

                foreach (DataRow dr in dt.Rows)
                {
                    FieldDataModel fdm = new FieldDataModel
                    {
                        ID = (int)dr["fieldId"],
                        Name = (string)dr["fieldName"],
                        Description = (string)dr["fieldDescription"],
                        Required = (bool)dr["requiredField"],
                        Datatype = (int)dr["fieldType"],
                        ViewID = etm.TypeSpecific.Count
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
            MySqlConnect msc = new MySqlConnect();
            if (etm.ID == -1)
            {
                msc.CreateEventType(etm);
            }
            else
            {
                msc.EditEventType(etm);
            }
            return RedirectToAction("Index","Home",null);
        }

        public ActionResult GetPartial(int id)
        {
            return PartialView("FieldDetails",new FieldDataModel(id));
        }

    }
}
