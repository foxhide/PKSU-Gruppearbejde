using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CalendarApplication.Models.User;

namespace CalendarApplication.Models.Group
{
    /// <summary>
    /// Class GroupModel contains data for a group
    /// </summary>
    public class GroupModel
    {
        public int ID { set; get; }

        [Display(Name = "Group name:")]
        public string Name { set; get; }
        public bool Selected { set; get; }
        public List<UserModel> groupMembers { set; get; }
        public List<UserModel> groupLeaders { set; get; }

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