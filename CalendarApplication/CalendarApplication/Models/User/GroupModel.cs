using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CalendarApplication.Models.User
{
    /// <summary>
    /// Class GroupModel contains data for a group
    /// </summary>
    public class GroupModel
    {
        public int ID { set; get; }

        public string Name { set; get; }
        public bool Selected { set; get; }

        //Override Equals
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj)) return true;

            if (obj == null) return false;

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