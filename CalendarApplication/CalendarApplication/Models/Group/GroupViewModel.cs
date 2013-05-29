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
        public List<UserModel> Members { get; set; }
        public List<UserModel> Leaders { get; set; }
        public List<UserModel> Creators { get; set; }
        public List<EventTypeModel> EventTypes { get; set; }

        public bool IsLeader(int id)
        {
            foreach(UserModel usr in Leaders)
            {
                if (usr.ID == id)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsMember(int id)
        {
            foreach (UserModel usr in Members)
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