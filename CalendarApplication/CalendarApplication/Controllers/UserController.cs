using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CalendarApplication.Models.User;
using System.Data;
using MySql.Data.MySqlClient;

namespace CalendarApplication.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/

        public ActionResult Index(int userId)
        {
            UserViewModel result = new UserViewModel { ID = userId };
            string userinfo = "SELECT * FROM pksudb.users WHERE userId = @userId";

            MySqlConnect con = new MySqlConnect();
            CustomQuery query = new CustomQuery();
            query.Cmd = userinfo;
            query.ArgNames = new string[] { "@userId" };
            query.Args = new object[] { userId };
            DataTable table = con.ExecuteQuery(query);

            if (table != null)
            {
                DataRow row = table.Rows[0];
                result.UserName = (string)row["userName"];
                //result.Password = (string)row["password"];
                result.RealName = (string)row["realName"];
                result.Admin = (bool)row["admin"];
                result.Email = (string)row["email"];
                //result.Active = (bool)row["active"];
                //result.NeedsApproval = (bool)row["needsApproval"];
                result.Groups = UserModel.GetGroups(userId);
                result.Events = UserModel.GetEvents(userId);
            }
            else
            {
                //negative user id upon error
                result.ID = -1;
            }
            return View(result);
        }

        public ActionResult EditUser(int userId)
        {
            // If no user -> go to home page...
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Index", "Home", null); }

            bool isAdmin = false;
            if (UserModel.GetCurrentUserID() != userId)
            {
                // Only an admin may edit another user.
                isAdmin = UserModel.GetCurrent().Admin;
                if (!isAdmin)
                {
                    TempData["errorMsg"] = "You are not admin, untermensch...";
                    return RedirectToAction("Index", new { userId = userId });
                }
            }

            UserEditModel result = new UserEditModel { ID = userId, AdminEdit = isAdmin };
            string userinfo = "SELECT * FROM pksudb.users WHERE userId = @userId";

            MySqlConnect con = new MySqlConnect();
            CustomQuery query = new CustomQuery();
            query.Cmd = userinfo;
            query.ArgNames = new string[] { "@userId" };
            query.Args = new object[] { userId };
            DataTable table = con.ExecuteQuery(query);

            if (table != null)
            {
                DataRow row = table.Rows[0];
                result.UserName = (string)row["userName"];
                result.RealName = (string)row["realName"];
                result.Admin = (bool)row["admin"];
                result.Email = (string)row["email"];
                if (isAdmin)
                {
                    result.Active = (bool)row["active"];
                    result.NeedsApproval = (bool)row["needsApproval"];
                }
            }
            else
            {
                //negative user id upon error
                result.ID = -1;
                TempData["errorMsg"] = con.ErrorMessage;
            }
            return View(result);
        }

        [HttpPost]
        public ActionResult EditUser(UserEditModel uem)
        {
            MySqlConnect msc = new MySqlConnect();
            bool result = false;
            if(uem.EditType == 1) // Edit name
            {
                result = msc.EditUser(uem.ID, uem.RealName, "realName");
            }
            else if(uem.EditType == 2) // Edit email
            {
                result = msc.EditUser(uem.ID, uem.Email, "email");
            }
            else if(uem.EditType == 3 && !uem.AdminEdit) // Edit password
            {
                CustomQuery check = new CustomQuery
                {
                    Cmd = "SELECT userId FROM pksudb.users WHERE userId = @uid AND password = @pw",
                    ArgNames = new[] { "@uid", "@pw" },
                    Args = new[] { uem.ID.ToString(), uem.OldPassword }
                };
                DataTable dt = msc.ExecuteQuery(check);
                if (dt != null)
                {
                    if (dt.Rows.Count == 1)
                    {
                        result = msc.EditUser(uem.ID, uem.NewPassword, "password");
                    }
                    else
                    {
                        TempData["errorMsg"] = "Your password doesn't match...";
                    }
                }
            }
            else if (uem.EditType == 4 && uem.AdminEdit)  // Admin
            {
                result = msc.EditUser(uem.ID, uem.Admin, "admin");
            }
            else if (uem.EditType == 5 && uem.AdminEdit)  // Active
            {
                result = msc.EditUser(uem.ID, uem.Active, "active");
            }
            else if (uem.EditType == 6 && uem.AdminEdit)  // Approved
            {
                result = msc.EditUser(uem.ID, uem.NeedsApproval, "needsApproval");
            }
            if (!result) { TempData["errorMsg"] = msc.ErrorMessage; }
            return View(uem);
        }

    }
}
