using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CalendarApplication.Models.User;
using CalendarApplication.Models.EventType;

namespace CalendarApplication.Models.Group
{
    public class GroupViewModel
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public List<GroupUserModel> Members { get; set; }
        public List<EventTypeModel> EventTypes { get; set; }

        public class GroupUserModel 
        {
            public string Name { get; set; }
            public int ID { get; set; }
            public bool Creator { get; set; }
            public bool Leader { get; set; }
        }

        public bool IsLeader(int id)
        {
            foreach(GroupUserModel usr in Members)
            {
                if (usr.Leader)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsMember(int id)
        {
            foreach (GroupUserModel usr in Members)
            {
                if (usr.ID == id)
                {
                    return true;
                }
            }
            return false;
        }
    }
}