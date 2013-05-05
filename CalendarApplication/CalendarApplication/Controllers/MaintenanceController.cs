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
using CalendarApplication.Models.Group;

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
                SelectedEventType = "0",
                Groups = new List<SelectListItem>(),
                SelectedGroup = "0"
            };

            MySqlConnect msc = new MySqlConnect();
            string etcmd = "SELECT eventTypeId,eventTypeName FROM pksudb.eventtypes";
            CustomQuery etquery = new CustomQuery { Cmd = etcmd };
            string grcmd = "SELECT * FROM pksudb.groups";
            CustomQuery grquery = new CustomQuery { Cmd = grcmd, ArgNames = { }, Args = { } };
            CustomQuery[] queries = { etquery, grquery };
            DataSet ds = msc.ExecuteQuery(queries);
            DataTable dt0 = ds.Tables[0];
            DataTable dt1 = ds.Tables[1];
            if (dt0 != null)
            {
                foreach (DataRow dr in dt0.Rows)
                {
                    mm.EventTypes.Add(new SelectListItem
                    {
                        Value = ((int)dr["eventTypeId"]).ToString(),
                        Text = (string)dr["eventTypeName"]
                    });
                }
                foreach (DataRow dr in dt1.Rows)
                {
                    mm.Groups.Add(new SelectListItem
                    {
                        Value = ((int)dr["groupId"]).ToString(),
                        Text = (string)dr["groupName"]
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
                //Edit group
                case 2: return RedirectToAction("EditGroup", new { groupId = int.Parse(mm.SelectedGroup) });
                //Create group
                case 3: return RedirectToAction("EditGroup", new { groupId = -1 });
            }
            return View(mm);
        }

        public ActionResult EditEventType(int id)
        {
            EventTypeModel etm = new EventTypeModel { TypeSpecific = new List<FieldDataModel>() };
            if (id == -1)
            {
                etm.ID = -1;
                //etm.Name = "Event type name here";
            }
            else
            {
                string getType = "SELECT * FROM (eventtypes NATURAL LEFT JOIN eventtypefields) WHERE eventTypeId = " + id;
                MySqlConnect msc = new MySqlConnect();
                DataTable dt = msc.ExecuteQuery(getType);
                etm.ID = id;
                etm.Name = (string)dt.Rows[0]["eventTypeName"];
                etm.ActiveFields = 0;

                if (dt.Rows[0]["fieldName"] as string != null)
                {
                    etm.ActiveFields = dt.Rows.Count;
                    foreach (DataRow dr in dt.Rows)
                    {
                        FieldDataModel fdm = new FieldDataModel
                        {
                            ID = (int)dr["fieldId"],
                            Name = (string)dr["fieldName"],
                            Description = dr["fieldDescription"] as string,
                            RequiredCreate = (bool)dr["requiredCreation"],
                            RequiredApprove = (bool)dr["requiredApproval"],
                            Datatype = (Fieldtype)dr["fieldType"],
                            ViewID = etm.TypeSpecific.Count,
                            VarcharLength = (int)dr["varCharLength"]
                        };
                        etm.TypeSpecific.Add(fdm);
                    }
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
                etm.TypeSpecific = new List<FieldDataModel>();
                return View(etm);
            }
            return RedirectToAction("Index","Maintenance",null);
        }

        // Returns the partial needed for making new fields in edit event type
        public ActionResult GetPartial(int id)
        {
            return PartialView("FieldDetails", new FieldDataModel(id));
        }

        public ActionResult EditGroup(int groupId)
        {
            GroupModel result = new GroupModel { ID = groupId };
        
            string cmd0 = "SELECT * FROM users WHERE needsApproval = @needsApproval";
            string[] argnames0 = { "@needsApproval" };
            object[] args0 = { 0 };
            CustomQuery query0 = new CustomQuery { Cmd = cmd0, ArgNames = argnames0, Args = args0 };
            string cmd1 = "SELECT * FROM groups NATURAL JOIN groupmembers NATURAL JOIN users WHERE groupId = @groupId";
            string[] argnames1 = { "@groupId" };
            object[] args1 = { groupId };
            CustomQuery query1 = new CustomQuery { Cmd = cmd1, ArgNames = argnames1, Args = args1 };
            CustomQuery[] queries = new CustomQuery[] { query0, query1 };
            MySqlConnect msc = new MySqlConnect();
            DataSet ds = msc.ExecuteQuery(queries);

            DataTable dt0 = ds.Tables[0];
            DataTable dt1 = ds.Tables[1];

            List<SelectListItem> members = new List<SelectListItem>();
            List<SelectListItem> leaders = new List<SelectListItem>();

            List<int> groupMembers = new List<int>();
            for (int i = 0; i < dt1.Rows.Count; i++)
            {
                groupMembers.Add((int)dt1.Rows[i]["userId"]);
                leaders.Add(new SelectListItem
                {
                    Value = ((int)dt1.Rows[i]["userId"]).ToString(),
                    Text = (string)dt1.Rows[i]["userName"],
                    Selected = (bool)dt1.Rows[i]["groupLeader"]
                });
            }

            result.Name = (string)dt1.Rows[0]["groupName"];

            for (int i = 0; i < dt0.Rows.Count; i++)
            {
                members.Add(new SelectListItem
                    {
                        Value = ((int)dt0.Rows[i]["userId"]).ToString(),
                        Text = (string)dt0.Rows[i]["userName"],
                        Selected = groupMembers.Contains((int)dt0.Rows[i]["userId"])
                    });

            }

            result.groupMembers = members;
            result.groupLeaders = leaders;

            return View(result);
        }

        [HttpPost]
        public ActionResult EditGroup(GroupModel grm)
        {
            MySqlConnect msc = new MySqlConnect();
            bool ok;
            if (grm.ID == -1)
            {
                ok = msc.CreateGroup(grm);
            }
            else
            {
                ok = msc.EditGroup(grm);
            }
            if (!ok)
            {
                TempData["errorMsg"] = msc.ErrorMessage;
                return View(grm);
            }
            return RedirectToAction("Index", "Maintenance", null);
        }

        public ActionResult ManageUsers()
        {
            ManageUserModel mum = new ManageUserModel
            {
                UASelect = 0,
                UsersApproved = new List<SelectListItem>(),
                UNASelect = 0,
                UsersNotApproved = new List<SelectListItem>(),
                UISelect = 0,
                UsersInactive = new List<SelectListItem>()
            };

            MySqlConnect msc = new MySqlConnect();
            CustomQuery query = new CustomQuery { Cmd = "SELECT userId,userName,needsApproval,active FROM pksudb.users ORDER BY userName" };
            DataTable dt = msc.ExecuteQuery(query);

            foreach (DataRow dr in dt.Rows)
            {
                SelectListItem sli = new SelectListItem { Value = ((int)dr["userId"]).ToString(), Text = (string)dr["userName"] };
                if (!((bool)dr["active"])) { mum.UsersInactive.Add(sli); }
                else if ((bool)dr["needsApproval"]) { mum.UsersNotApproved.Add(sli); }
                else { mum.UsersApproved.Add(sli); }
            }
            
            return View(mum);
        }

        [HttpPost]
        public void EditUser(string type, int userId)
        {
            MySqlConnect msc = new MySqlConnect();
            if (type == "active-add")
            {
                msc.EditUser(userId, true, "active");
            }
            else if (type == "active-rem")
            {
                msc.EditUser(userId, false, "active");
            }
            else if (type == "approval")
            {
                msc.EditUser(userId, false, "needsApproval");
            }
        }

    }

}
