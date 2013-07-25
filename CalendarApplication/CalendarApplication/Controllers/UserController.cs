using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CalendarApplication.Models.User;
using System.Data;

using CalendarApplication.Database;

namespace CalendarApplication.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/

        public ActionResult Index(int userId)
        {
            // Check if user is logged in
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Index", "Home", null); }

            UserViewModel result = new UserViewModel { ID = userId };
            string userinfo = "SELECT * FROM users WHERE userId = @userId";

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
                result.FirstName = row["firstName"] is DBNull ? "Not available" : (string)row["firstName"];
                result.LastName = row["lastName"] is DBNull ? "Not available" : (string)row["lastName"];
                result.Admin = (bool)row["admin"];
                result.Phone = row["phoneNum"] is DBNull ? "Not available" : (string)row["phoneNum"];
                result.Email = row["email"] is DBNull ? "Not available" : (string)row["email"];
                result.Active = (bool)row["active"];
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
    }
}
