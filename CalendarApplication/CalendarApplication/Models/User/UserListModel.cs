using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CalendarApplication.Models.User
{
    public class UserListModel
    {
        public List<UserModel> UserList { get; set; } 
    }
}