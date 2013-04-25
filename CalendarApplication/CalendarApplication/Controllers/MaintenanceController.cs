using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Windows.Forms;

using CalendarApplication.Models.EventType;
using CalendarApplication.Models.Maintenance;
using CalendarApplication.Controllers;
using CalendarApplication.Models.User;

namespace CalendarApplication.Controllers
{
    public class MaintenanceController : Controller
    {
        public static int MAX_NUMBER_OF_FIELDS = 100;

        //
        // GET: /Maintenance/

        public ActionResult Index()
        {
            if (UserModel.GetCurrentUserID() == -1)
            {
                return RedirectToAction("Login", "Account", null);
            }
            else if (!UserModel.GetCurrent().Admin)
            {
                return RedirectToAction("Index", "Home", null);
            }

            MaintenanceModel mm = new MaintenanceModel
            {
                EventTypes = new List<SelectListItem>(),
                SelectedEventType = "0"
            };
            MySqlConnect msc = new MySqlConnect();
            string etquery = "SELECT eventTypeId,eventTypeName FROM pksudb.eventtypes";
            DataTable dt = msc.ExecuteQuery(etquery);
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    mm.EventTypes.Add(new SelectListItem
                    {
                        Value = ((int)dr["eventTypeId"]).ToString(),
                        Text = (string)dr["eventTypeName"]
                    });
                }
                return View(mm);
            }
            TempData["errorMsg"] = msc.ErrorMessage;
            return View(mm);
        }

        [HttpPost]
        public ActionResult Index(MaintenanceModel mm)
        {
            switch (mm.SubmitValue)
            {
                //Edit event
                case 0: return RedirectToAction("EditEventType", new { id = int.Parse(mm.SelectedEventType) });
                //Create event
                case 1: return RedirectToAction("EditEventType", new { id = -1 });
            }
            return View(mm);
        }

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
                string getType = "SELECT * FROM (eventtypes NATURAL JOIN eventtypefields) WHERE eventTypeId = " + id;
                MySqlConnect msc = new MySqlConnect();
                DataTable dt = msc.ExecuteQuery(getType);
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
                        Datatype = (Fieldtype)dr["fieldType"],
                        ViewID = etm.TypeSpecific.Count,
                        VarcharLength = (int)dr["varchar_length"]
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
            bool ok;
            if (etm.ID == -1)
            {
                ok = msc.CreateEventType(etm);
            }
            else
            {
                ok = msc.EditEventType(etm);
            }
            if (!ok)
            {
                TempData["errorMsg"] = msc.ErrorMessage;
                return View(etm);
            }
            return RedirectToAction("Index","Maintenance",null);
        }

        // Returns the partial needed for making new fields in edit event type
        public ActionResult GetPartial(int id)
        {
            return PartialView("FieldDetails", new FieldDataModel(id));
        }

    }
}
