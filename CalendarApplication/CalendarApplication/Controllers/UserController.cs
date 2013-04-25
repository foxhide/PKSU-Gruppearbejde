using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CalendarApplication.Models.User;
using System.Data;

namespace CalendarApplication.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/

        public ActionResult Index(int userId)
        {
            UserViewModel result = new UserViewModel { ID = userId };
            string userinfo = "SELECT * FROM pksudb.users WHERE userId = " + userId;
            MySqlConnect con = new MySqlConnect();
            DataTable table = con.ExecuteQuery(userinfo);

            if (table != null)
            {
                DataRow row = table.Rows[0];
                result.UserName = (string)row["userName"];
                result.Password = (string)row["password"];
                result.RealName = (string)row["realName"];
                result.Admin = (bool)row["admin"];
                result.Email = (string)row["email"];
                result.Active = (bool)row["active"];
                result.NeedsApproval = (bool)row["needsApproval"];
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
