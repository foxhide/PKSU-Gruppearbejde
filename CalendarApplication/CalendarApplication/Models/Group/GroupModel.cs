using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;


using CalendarApplication.Models.User;

namespace CalendarApplication.Models.Group
{
    /// <summary>
    /// Class GroupModel contains data for a group
    /// </summary>
    public class GroupModel
    {
        public int ID { set; get; }

        [Display(Name = "Group Name")]
        public string Name { set; get; }
        public bool Selected { set; get; }
        [Display(Name = "Group Members")]
        public List<SelectListItem> groupMembers { set; get; }
        [Display(Name = "Group Leaders")]
        public List<SelectListItem> groupLeaders { set; get; }
        [Display(Name = "Event Creators")]
        public List<SelectListItem> canCreate { set; get; }

        //Override Equals
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj)) return true;

            if (!(obj is GroupModel)) return false;

            GroupModel otherGroup = (GroupModel)obj;
            return otherGroup.ID == ID;
        }

        //Override HashCode
        public override int GetHashCode()
        {
            return ID;
        }
    }
}