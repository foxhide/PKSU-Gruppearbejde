using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CalendarApplication.Models.Account
{
    public class AccountEditModel : Register
    {
        [DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        public string OldPassword { set; get; }

        public int ID { set; get; }
        public bool Admin { set; get; }
    }
}