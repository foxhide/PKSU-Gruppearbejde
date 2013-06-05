using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;
using System.Web.Security;
using System.Data;
using Recaptcha;

using CalendarApplication.Models.Account;
using CalendarApplication.Models.User;
using CalendarApplication.Database;
using System.Windows.Forms;

namespace CalendarApplication.Controllers
{
    public class AccountController : Controller
    {

        public ActionResult Login()
        {
            // Check if user is logged in
            if (UserModel.GetCurrentUserID() != -1) { return RedirectToAction("Index", "Home", null); }

            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel login)
        {
            // Check if user is logged in
            if (UserModel.GetCurrentUserID() != -1) { return RedirectToAction("Index", "Home", null); }

            CustomQuery query = new CustomQuery();
            query.Cmd = "SELECT userId,needsApproval,active,password FROM pksudb.users WHERE userName = @usrnam";
            query.ArgNames = new string[] { "@usrnam" };
            query.Args = new object[] { login.UserName };
            MySqlConnect con = new MySqlConnect();
            DataTable table = con.ExecuteQuery(query);

            if (table != null)
            {
                DataRowCollection rows = table.Rows;
                if (rows.Count == 1 && PasswordHashing.ValidatePassword(login.Password, (string)rows[0]["password"]))
                {
                    // User found
                    if ((bool)rows[0]["needsApproval"])
                    {
                        // User not approved
                        TempData["message"] = "Account found, but it has not yet been approved. Contact your admin.";
                    }
                    else if (!(bool)rows[0]["active"])
                    {
                        // User not active
                        TempData["message"] = "That account has been deactivated. Contact your admin.";
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
                TempData["message"] = "Login unsuccessful! " + con.ErrorMessage;
            }
            return RedirectToAction("Login");
        }

        public ActionResult LogOut()
        {
            // Check if user is not logged in
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Index", "Home", null); }

            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home", null);
        }

        public ActionResult Register()
        {
            // Check if user is logged in
            if (UserModel.GetCurrentUserID() != -1) { return RedirectToAction("Index", "Home", null); }

            return View();
        }

        [HttpPost]
        [RecaptchaControlMvc.CaptchaValidator]
        public ActionResult Register(Register model, bool captchaValid)
        {
            // Check if user is logged in
            if (UserModel.GetCurrentUserID() != -1) { return RedirectToAction("Index", "Home", null); }

            // Check if captcha was validated
            if (!captchaValid)
            {
                TempData["message"] = "Captcha invalid.";

                return View();
            }
            
            MySqlUser msu = new MySqlUser();
            int userId = msu.CreateUser(model);
            if (userId >= 0)
            {
                //The user is not logged in, simply created, and needs approval
                TempData["message"] = "Registration successful. You will need to be approved by an administrator before you can log in.";
                return RedirectToAction("Login", "Account", null);
            }
            TempData["message"] = "There was an error processing your information: " + msu.ErrorMessage;
            
            return View();
        }

        public ActionResult EditUser(int userId)
        {
            // If no user or user trying to edit other user's profile -> go to home page...
            if (UserModel.GetCurrentUserID() == -1
                || UserModel.GetCurrentUserID() != userId) { return RedirectToAction("Index", "Home", null); }

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
                result.RealName = row["realName"] is DBNull ? "" : (string)row["realName"];
                result.Admin = (bool)row["admin"];
                result.Email = row["email"] is DBNull ? "" : (string)row["email"];
                result.Phone = row["phoneNum"] is DBNull ? "" : (string)row["phoneNum"];
            }
            else
            {
                //negative user id upon error
                result.ID = -1;
                TempData["errorMsg"] = con.ErrorMessage;
            }
            return View(result);
        }

        /* Edit a user string value (other than password) */
        [HttpPost]
        public bool EditUserString(string field, int userId, string value)
        {
            // If no user or user trying to edit other user's profile -> return
            if (UserModel.GetCurrentUserID() == -1 || UserModel.GetCurrentUserID() != userId) { return false; }

            MySqlUser msu = new MySqlUser();
            return msu.EditUser(userId, value, field);
        }

        /* Edit a user boolean value - only admins! */
        [HttpPost]
        public bool EditUserBool(string field, int userId, bool value)
        {
            // If no user or non-admin trying to edit other user's boolean -> return
            if (UserModel.GetCurrentUserID() == -1 || !UserModel.GetCurrent().Admin) { return false; }

            MySqlUser msu = new MySqlUser();
            return msu.EditUser(userId, value, field);
        }

        /* Edit the password */
        [HttpPost]
        public int EditUserPassword(int userId, string oldPass, string newPass)
        {
            // If no user or user trying to edit other user's profile -> return error message
            if (UserModel.GetCurrentUserID() == -1
                || UserModel.GetCurrentUserID() != userId) { return -1; }

            MySqlUser msu = new MySqlUser();
            CustomQuery query = new CustomQuery
            {
                Cmd = "SELECT password FROM pksudb.users WHERE userId = @uid",
                ArgNames = new[] { "@uid" },
                Args = new[] { (object)userId }
            };
            DataTable dt = msu.ExecuteQuery(query);
            if (dt == null) { return -11; }
            else if (dt.Rows.Count == 0) { return -12; }

            string goodHash = (string)dt.Rows[0]["password"];
            if (!PasswordHashing.ValidatePassword(oldPass, goodHash)) { return -2; }

            string newPassHashed = PasswordHashing.CreateHash(newPass);
            if (!msu.EditUser(userId, newPassHashed, "password")) { return -13; }
            return 0;
        }

        /* Reset a user password */
        [HttpPost]
        public string PasswordReset(int userId)
        {
            // Only admins may request a password reset.
            if (UserModel.GetCurrentUserID() == -1 || !UserModel.GetCurrent().Admin) { return null; }

            MySqlUser msu = new MySqlUser();

            // Generate a new password
            char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890_*?!#%".ToCharArray();
            string newPass = "";
            Random r = new Random();
            for (int i = 0; i < 16; i++)
            {
                newPass += chars[r.Next(chars.Length)];
            }
            string hashedPass = PasswordHashing.CreateHash(newPass);
            if (!msu.EditUser(userId, hashedPass, "password")) { return null; }

            return newPass;
        }

        /* Delete a user (only if he/she is not approved yet) - only admins */
        [HttpPost]
        public void DeleteUser(int userId)
        {
            // Check if user is admin
            if (UserModel.GetCurrentUserID() == -1 || !UserModel.GetCurrent().Admin) { return; }

            MySqlUser msu = new MySqlUser();
            CustomQuery query = new CustomQuery
            {
                Cmd = "SELECT needsApproval FROM pksudb.users WHERE userId = @uid",
                ArgNames = new[] { "@uid" },
                Args = new[] { (object)userId }
            };
            DataTable dt = msu.ExecuteQuery(query);
            if (dt != null && dt.Rows.Count == 1)
            {
                if ((bool)dt.Rows[0]["needsApproval"])
                {
                    msu.deleteUser(userId);
                }
            }
        }
    }
}
