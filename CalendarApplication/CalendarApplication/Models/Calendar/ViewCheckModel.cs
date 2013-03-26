using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CalendarApplication.Models.User;

namespace CalendarApplication.Models.Calendar
{
    public class ViewCheckModel
    {
        public int Month { set; get; }
        public int Year { set; get; }
        public List<GroupModel> GroupsAvailable { set; get; }
        public List<GroupModel> GroupsSelected { set; get; }
        public int Eventtypes { set; get; }
    }
}