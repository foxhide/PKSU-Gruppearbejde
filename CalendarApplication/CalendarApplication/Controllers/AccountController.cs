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
            CustomQuery query = new CustomQuery();
            query.Cmd = "SELECT userId,needsApproval FROM pksudb.users WHERE userName = @usrnam AND password = @passw";
            query.ArgNames = new string[] { "@usrnam" , "@passw" };
            query.Args = new object[] { login.UserName , login.Password };
            MySqlConnect con = new MySqlConnect();
            DataTable table = con.ExecuteQuery(query);

            if (table != null)
            {
                DataRowCollection rows = table.Rows;
                if (rows.Count == 1)
                {
                    // User found
                    if ((bool)rows[0]["needsApproval"])
                    {
                        // User not approved
                        TempData["message"] = "Account found, but not yet approved. Contact your admin.";
                    }
                    else
                    {
                        // User approved -> log in
                        string userData = ((int)rows[0]["userId"]).ToString();
                        FormsAuthentication.SetAuthCookie(userData, login.RememberMe);
                        return RedirectToAction("Index", "Home", null);
                    }
                }
                else
                {
                    // User not found
                    TempData["message"] = "Login unsuccessful! There was no match between the username and password!";
                }
            }
            else
            {
                // DB error
                TempData["message"] = "Login unsuccessful! Some database error occured!";
            }
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
                //The user is not logged in, simply created, and needs approval
                return RedirectToAction("Index", "Home", null);
            }
            TempData["message"] = "There was an error processing your information. Please try again.";
            return View();
        }
    }
}
