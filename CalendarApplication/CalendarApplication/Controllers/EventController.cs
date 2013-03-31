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
    }
}
