using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Data;

using CalendarApplication.Models.Account;
using CalendarApplication.Models.User;
using CalendarApplication.Database;

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
            query.Cmd = "SELECT userId,needsApproval,active FROM pksudb.users WHERE userName = @usrnam AND password = @passw";
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
                        TempData["message"] = "Account found, but is has not yet been approved. Contact your admin.";
                    }
                    else if (!(bool)rows[0]["active"])
                    {
                        // User not active
                        TempData["message"] = "The found account has been deactivated. Contact your admin.";
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
            MySqlUser msu = new MySqlUser();
            int userId = msu.CreateUser(model);
            if (userId >= 0)
            {
                //The user is not logged in, simply created, and needs approval
                return RedirectToAction("Index", "Home", null);
            }
            TempData["message"] = "There was an error processing your information. Please try again.";
            return View();
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

            AccountEditModel result = new AccountEditModel { ID = userId };
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
        public void EditUserString(string field, int userId, string value)
        {
            MySqlUser msu = new MySqlUser();
            msu.EditUser(userId, value, field);
        }

        [HttpPost]
        public void EditUserBool(string field, int userId, bool value)
        {
            MySqlUser msu = new MySqlUser();
            msu.EditUser(userId, value, field);
        }

        [HttpPost]
        public string EditUserPassword(int userId, string oldPass, string newPass)
        {
            MySqlUser msu = new MySqlUser();
            msu.EditUser(userId, newPass, "password");
            return "";
        }
    }
}
