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
        public bool Open { get; set; }
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
                if (usr.ID == id)
                {
                    return usr.Leader;
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

        public bool IsApplicant(int id)
        {
            if (id != -1)
            {
                string cmd = "SELECT * FROM groupApplicants WHERE userId = @uid AND groupId = @gid";
                string[] argnam = { "@uid", "@gid" };
                object[] args = { id, this.ID };
                Database.CustomQuery query = new Database.CustomQuery{ Args = args, ArgNames = argnam, Cmd = cmd };
                Database.MySqlConnect msc = new Database.MySqlConnect();
                System.Data.DataTable dt = msc.ExecuteQuery(query);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        return !(dt.Rows[0]["userId"] is DBNull);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }
    }
}