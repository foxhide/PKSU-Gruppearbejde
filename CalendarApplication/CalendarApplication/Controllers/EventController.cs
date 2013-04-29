using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Windows.Forms;

using CalendarApplication.Models.User;
using CalendarApplication.Models.Event;
using CalendarApplication.Models.EventType;

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
                result.CreatorId = (int)rows[0]["userId"];
                result.Rooms = new List<Room>();                       
                for (int i = 0; i < rows.Count; i++)
                {
                    result.Rooms.Add(new Room
                    {
                        ID = (int)rows[i]["roomId"],
                        Name = (string)rows[i]["roomName"]
                    });
                }
                CustomQuery query0 = new CustomQuery();
                query0.Cmd = "SELECT * FROM table_" + result.TypeId + " WHERE eventId = @eventId";
                query0.ArgNames = new string[] { "@eventId" };
                query0.Args = new object[] { id };
                CustomQuery query1 = new CustomQuery();
                query1.Cmd = "SELECT * FROM pksudb.eventtypefields WHERE eventTypeId = @eventTypeId";
                query1.ArgNames = new string[] { "@eventTypeId" };
                query1.Args = new object[] { result.TypeId };
                DataSet ds = con.ExecuteQuery(new CustomQuery[] { query0, query1 });

                /*
                DataSet ds = con.ExecuteQuery(new string[] {
                    "SELECT * FROM table_" + result.TypeId + " WHERE eventId = " + id,
                    "SELECT * FROM pksudb.eventtypefields WHERE eventTypeId = " + result.TypeId
                });*/

                if (ds != null)
                {
                    result.EventSpecial = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        result.EventSpecial.Columns[i + 1].ColumnName = (string)ds.Tables[1].Rows[i]["fieldName"];
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

        public ActionResult EditEvent(int id, int year, int month, int day)
        {
            EventEditModel eem = new EventEditModel
            {
                ID = id,
                EventTypes = new List<SelectListItem>(),
                SelectedEventType = "1", // Initial value -> Basic event
                Start = new DateTime(year, month, day, 10, 0, 0),
                End = new DateTime(year, month, day, 18, 0, 0)
            };

            MySqlConnect msc = new MySqlConnect();

            // Get list of rooms //
            eem.RoomSelectList = new List<SelectListItem>();
            string roomquery = "SELECT roomId,roomName FROM pksudb.rooms";
            DataTable dt = msc.ExecuteQuery(roomquery);
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    eem.RoomSelectList.Add(new SelectListItem
                    {
                        Value = ((int)dr["roomId"]).ToString(),
                        Text = (string)dr["roomName"]
                    });
                }
            }

            // Get list of users
            eem.UserEditorList = new List<SelectListItem>();
            string userquery = "SELECT userId,userName FROM pksudb.users";
            dt = msc.ExecuteQuery(userquery);
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    eem.UserEditorList.Add(new SelectListItem
                    {
                        Value = ((int)dr["userId"]).ToString(),
                        Text = (string)dr["userName"]
                    });
                }
            }

            // Get list of groups
            eem.GroupEditorList = new List<SelectListItem>();
            eem.GroupVisibleList = new List<SelectListItem>();
            string groupquery = "SELECT groupId,groupName FROM pksudb.groups";
            dt = msc.ExecuteQuery(groupquery);
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    eem.GroupEditorList.Add(new SelectListItem
                    {
                        Value = ((int)dr["groupId"]).ToString(),
                        Text = (string)dr["groupName"]
                    });
                    eem.GroupVisibleList.Add(new SelectListItem
                    {
                        Value = ((int)dr["groupId"]).ToString(),
                        Text = (string)dr["groupName"]
                    });
                }
            }

            TempData["errorMsg"] = msc.ErrorMessage;
            if (id == -1) { this.createModel(eem); }
            else { this.getModel(eem); }
            
            return View(eem);
        }

        [HttpPost]
        public ActionResult EditEvent(EventEditModel eem)
        {
            if (eem.SubmitType == 1)
            {
                MySqlConnect msc = new MySqlConnect();
                eem.CreatorId = UserModel.GetCurrentUserID();
                if (msc.EditEvent(eem)) { return RedirectToAction("Index", "Home", null); }
                else {
                    TempData["errorMsg"] = msc.ErrorMessage;
                    this.createModel(eem);
                    return View(eem);
                }
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
            
            eem.TypeSpecifics = new List<FieldModel>();
            string specQuery = "SELECT * FROM eventtypefields WHERE eventTypeId = " + eem.SelectedEventType;
            dt = msc.ExecuteQuery(specQuery);
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    FieldModel fm = new FieldModel
                    {
                        ID = (int)dr["fieldId"],
                        Name = (string)dr["fieldName"],
                        Description = dr["fieldDescription"] as string,
                        Required = (bool)dr["requiredField"],
                        Datatype = (Fieldtype)dr["fieldType"],
                        VarcharLength = (int)dr["varCharLength"]
                    };
                    switch (fm.Datatype)
                    {
                        case Fieldtype.Integer: fm.IntValue = 0; break; //int
                        case Fieldtype.User: fm.List = eem.UserEditorList; fm.IntValue = 0; break;
                        case Fieldtype.Group: fm.List = eem.GroupEditorList; fm.IntValue = 0; break;
                        case Fieldtype.Text:
                        case Fieldtype.File: fm.StringValue = ""; break; //string
                        case Fieldtype.Datetime: fm.DateValue = DateTime.Now; break;
                        case Fieldtype.Bool: fm.BoolValue = false; break; //bool
                    }
                    eem.TypeSpecifics.Add(fm);
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
                eem.TypeSpecifics = new List<FieldModel>();
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
                            Datatype = (Fieldtype)dr["fieldType"],
                            VarcharLength = (int)dr["varCharLength"]
                        };

                        switch (fm.Datatype)
                        {
                            case Fieldtype.Integer:
                            case Fieldtype.User:
                            case Fieldtype.Group: fm.IntValue = (int)data[i + 1]; break; //int
                            case Fieldtype.Text:
                            case Fieldtype.File: fm.StringValue = (string)data[i + 1]; break; //string
                            case Fieldtype.Datetime: fm.DateValue = (DateTime)data[i + 1]; break;
                            case Fieldtype.Bool: fm.BoolValue = (bool)data[i + 1]; break; //bool
                        }

                        eem.TypeSpecifics.Add(fm);
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
