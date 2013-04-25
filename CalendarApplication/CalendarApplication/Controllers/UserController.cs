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
            UserViewModel result = new UserViewModel { UserId = userId };
            string userinfo = "SELECT * FROM pksudb.users WHERE userId = " + userId;
            MySqlConnect con = new MySqlConnect();
            DataTable table = con.ExecuteQuery(userinfo);

            if (table != null)
            {
                DataRowCollection rows = table.Rows;
                result.UserName = (string)rows[0]["userName"];
                result.Password = (string)rows[0]["password"];
                result.RealName = (string)rows[0]["realName"];
                result.Admin = (bool)rows[0]["admin"];
                result.Email = (string)rows[0]["email"];
                result.Active = (bool)rows[0]["active"];
                result.NeedsApproval = (bool)rows[0]["needsApproval"];
            }
            else
            {
                //negative user id upon error
                result.UserId = -1;
            }
            return View(result);
        }

    }
}
