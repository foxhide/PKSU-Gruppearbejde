using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CalendarApplication.Models.Account;
using CalendarApplication.Models.User;
using System.Data;

namespace CalendarApplication.Controllers
{
    public class AccountController : Controller
    {

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel login)
        {
            string userinf = "SELECT userId FROM pksudb.users WHERE userName = '" + login.UserName
                           + "' AND password = '" + login.Password+"'";
            MySqlConnect con = new MySqlConnect();
            DataTable table = con.ExecuteQuery(userinf);

            if (table != null)
            {
                DataRowCollection rows = table.Rows;
                if (rows.Count == 1)
                {
                    string userData = ""+(int)rows[0]["userId"];

                    FormsAuthentication.SetAuthCookie(userData, login.RememberMe);
                    
                    return RedirectToAction("Index","Home",null);
                }
            }
            TempData["message"] = "Login unsuccessful! There was no match between the username and password!";
            return RedirectToAction("Login");
        }

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home", null);
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(Register model)
        {
            MySqlConnect msc = new MySqlConnect();
            int userId = msc.CreateUser(model);
            if (userId >= 0)
            {
                //Remove this call if the user should not be logged in after registering.
                FormsAuthentication.SetAuthCookie(""+userId, false);

                return RedirectToAction("Index", "Home", null);
            }
            TempData["message"] = "There was an error processing your information. Please try again.";
            return View();
        }
    }
}
