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
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }

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

        /// <summary>
        /// Gets page for files a user has uploaded
        /// </summary>
        /// <returns></returns>
        public ActionResult FilesUploaded(int userId)
        {
            // Check if user is logged in and viewing own files or is admin
            if (UserModel.GetCurrentUserID() == -1) { return RedirectToAction("Login", "Account", null); }
            else if (!(UserModel.GetCurrentUserID() == userId || UserModel.GetCurrent().Admin)) { return RedirectToAction("Index", "Home", null); }

            FileListModel model = new FileListModel();
            MySqlConnect sql = new MySqlConnect();
            string que = "SELECT f.fileId,f.fileName,f.uploaded,u.userId,u.firstName,u.lastName,e.eventId,e.eventName"
                         + " FROM files AS f NATURAL JOIN users AS u NATURAL LEFT JOIN events AS e"
                         + " WHERE u.userId = @uid ORDER BY f.uploaded DESC";
            string[] argnam = { "@uid" };
            object[] args = { userId };
            CustomQuery query = new CustomQuery { Cmd = que, Args = args, ArgNames = argnam };
            DataTable dt = sql.ExecuteQuery(query);
            if (dt != null)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (!(dt.Rows[i]["fileId"] is DBNull) && !(dt.Rows[i]["userId"] is DBNull))
                    {
                        Models.Event.FileModel file = new Models.Event.FileModel();
                        file.CurrentFileName = dt.Rows[i]["fileName"] as string;
                        file.ID = (int)dt.Rows[i]["fileId"];
                        file.UploaderID = (int)dt.Rows[i]["userId"];
                        file.UploaderName = (dt.Rows[i]["firstName"] as string) + " " + (dt.Rows[i]["lastName"] as string);
                        file.Uploaded = dt.Rows[i]["uploaded"] as DateTime? ?? new DateTime(1, 1, 1);
                        file.EventID = dt.Rows[i]["eventId"] as int? ?? -1;
                        file.EventName = dt.Rows[i]["eventName"] as string;
                        model.Files.Add(file);
                    }
                }
            }
            else
            {
                TempData["errorMsg"] = sql.ErrorMessage;
            }
            return View(model);
        }
    }
}
