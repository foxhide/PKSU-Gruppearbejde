using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CalendarApplication.Models.Event;
using System.Data;

namespace CalendarApplication.Controllers
{
    public class EventController : Controller
    {
        //
        // GET: /Event/

        public ActionResult Index(int id)
        {
            EventWithDetails result = new EventWithDetails
            {
                ID = id
            };
            string eventinfo = "SELECT * FROM pksudb.events NATURAL JOIN pksudb.eventroomsused " +
                               "NATURAL JOIN pksudb.rooms NATURAL JOIN pksudb.eventtypes NATURAL JOIN pksudb.users " +
                               "WHERE eventId = " + id;
            MySqlConnect con = new MySqlConnect();
            DataTable table = con.ExecuteQuery(eventinfo);

            if (table != null)
            {
                DataRowCollection rows = table.Rows;
                result.Name = (string)rows[0]["eventName"];
                result.Start = (DateTime)rows[0]["eventStart"];
                result.End = (DateTime)rows[0]["eventEnd"];
                result.State = (int)rows[0]["state"];
                result.TypeId = (int)rows[0]["eventTypeId"];
                result.TypeName = (string)rows[0]["eventTypeName"];
                result.Visible = (bool)rows[0]["visible"];
                result.Creator = (string)rows[0]["userName"];
                result.Rooms = new List<Room>();                       
                for (int i = 0; i < rows.Count; i++)
                {
                    Room tmpRoom = new Room { ID = (int)rows[i]["roomId"], Name = (string)rows[i]["roomName"] };
                    result.Rooms.Add(tmpRoom);
                }
                if (result.TypeId != 1)
                {
                    DataSet ds = con.ExecuteQuery(new string[] {
                        "SELECT * FROM table_" + result.TypeId + " WHERE eventId = " + id,
                        "SELECT * FROM pksudb.eventtypefields WHERE eventTypeId = " + result.TypeId
                    });
                    result.EventSpecial = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        result.EventSpecial.Columns[i+1].ColumnName = (string)ds.Tables[1].Rows[i]["fieldName"];
                    }
                }
            }
            else
            {
                //negative event id upon error
                result.ID = -1;
            }
            return View(result);
        }

        public ActionResult EditEvent(int id)
        {
            EventEditModel eem = new EventEditModel
            {
                ID = id,
                EventTypes = new List<SelectListItem>(),
                SelectedEventType = "1" // Initial value -> Basic event
            };

            if (id == -1) { this.createModel(eem); }
            else { this.getModel(eem); }
            
            return View(eem);
        }

        [HttpPost]
        public ActionResult EditEvent(EventEditModel eem)
        {
            if (eem.SubmitType == 1)
            {
                return RedirectToAction("Index", "Home", null);
            }
            else
            {
                this.createModel(eem);
                return View(eem);
            }
        }

        public void createModel(EventEditModel eem)
        {
            MySqlConnect msc = new MySqlConnect();

            // Get list of event types //
            eem.EventTypes = new List<SelectListItem>();
            string etquery = "SELECT eventTypeId,eventTypeName FROM pksudb.eventtypes";
            DataTable dt = msc.ExecuteQuery(etquery);
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    eem.EventTypes.Add(new SelectListItem
                    {
                        Value = ((int)dr["eventTypeId"]).ToString(),
                        Text = (string)dr["eventTypeName"]
                    });
                }
            }

            if (!eem.SelectedEventType.Equals("1")) // Not a basic event, get the specefics
            {
                eem.TypeSpecefics = new List<FieldModel>();
                string specQuery = "SELECT * FROM eventtypefields WHERE eventTypeId = " + eem.SelectedEventType;
                dt = msc.ExecuteQuery(specQuery);
                if (dt != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        eem.TypeSpecefics.Add(new FieldModel
                        {
                            ID = (int)dr["fieldId"],
                            Name = (string)dr["fieldName"],
                            Description = (string)dr["fieldDescription"],
                            Required = (bool)dr["requiredField"],
                            Datatype = (int)dr["fieldType"],
                            VarcharLength = (int)dr["varCharLength"]
                        });
                    }
                }
            }
        }

        public bool getModel(EventEditModel eem)
        {
            if (eem.ID == -1) { return true; }

            MySqlConnect msc = new MySqlConnect();

            // Get list of event types //
            eem.EventTypes = new List<SelectListItem>();
            string etquery = "SELECT eventTypeId,eventTypeName FROM pksudb.eventtypes";
            DataTable dt = msc.ExecuteQuery(etquery);
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    eem.EventTypes.Add(new SelectListItem
                    {
                        Value = ((int)dr["eventTypeId"]).ToString(),
                        Text = (string)dr["eventTypeName"]
                    });
                }
            }

            string basic = "SELECT eventName,eventTypeId,eventStart,eventEnd,userName "
                            + "FROM pksudb.events NATURAL JOIN pksudb.users"
                            + "WHERE eventId == " + eem.ID;
            dt = msc.ExecuteQuery(basic);

            eem.Name = (string)dt.Rows[0]["eventName"];
            eem.Start = (DateTime)dt.Rows[0]["eventStart"];
            eem.End = (DateTime)dt.Rows[0]["eventEnd"];
            eem.Creator = (string)dt.Rows[0]["userName"];
            eem.SelectedEventType = ((int)dt.Rows[0]["eventTypeId"]).ToString();

            if (!eem.SelectedEventType.Equals("1"))
            {
                eem.TypeSpecefics = new List<FieldModel>();
                string specQuery = "SELECT * FROM eventtypefields WHERE eventTypeId = " + eem.SelectedEventType;
                string specData = "SELECT * FROM pksudb.table_" + eem.SelectedEventType
                                    + " WHERE eventId == " + eem.ID;

                DataSet ds = msc.ExecuteQuery(new[] { specQuery, specData });
                if (ds != null)
                {
                    DataTable fields = ds.Tables[0];
                    DataRow data = ds.Tables[1].Rows[0];
                    for (int i = 0; i < fields.Rows.Count; i++)
                    {
                        DataRow dr = fields.Rows[i];
                        FieldModel fm = new FieldModel
                        {
                            ID = (int)dr["fieldId"],
                            Name = (string)dr["fieldName"],
                            Description = (string)dr["fieldDescription"],
                            Required = (bool)dr["requiredField"],
                            Datatype = (int)dr["fieldType"],
                            VarcharLength = (int)dr["varCharLength"]
                        };

                        switch (fm.Datatype)
                        {
                            case 0:
                            case 3:
                            case 4: fm.IntValue = (int)data[i+1]; break; //int
                            case 1:
                            case 2:
                            case 5: fm.StringValue = (string)data[i+1]; break; //string
                            case 6: fm.BoolValue = (bool)data[i+1]; break; //bool
                        }

                        eem.TypeSpecefics.Add(fm);
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
