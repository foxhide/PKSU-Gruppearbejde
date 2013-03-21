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
            string userinf = "SELECT * FROM pksudb.users WHERE userName = '" + login.UserName
                           + "' AND password = '" + login.Password+"'";
            MySqlConnect con = new MySqlConnect();
            DataTable table = con.ExecuteQuery(userinf);

            if (table != null)
            {
                DataRowCollection rows = table.Rows;
                if (rows.Count == 1)
                {
                    string userData = (int)rows[0]["userId"] + "|" +
                                        (string)rows[0]["userName"] + "|" +
                                        (string)rows[0]["realName"] + "|" +
                                        (string)rows[0]["email"];

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
                string userData = userId + "|" + model.UserName + "|" + model.RealName + "|" + model.Email;

                FormsAuthentication.SetAuthCookie(userData, false);
                return RedirectToAction("Index", "Home", null);
            }
            TempData["message"] = "This user name is all ready taken, please choose a new one...";
            return View();
        }
    }
}
