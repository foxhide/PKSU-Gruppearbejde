using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;


namespace CalendarApplication.Models.Group
{

    public class GroupListModel
    {
        public List<GroupModel> GroupList { set; get; }
    }
}