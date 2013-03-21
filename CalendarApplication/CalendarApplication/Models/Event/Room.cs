﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CalendarApplication.Models.Event
{
    public class Room
    {
        public int ID { set; get; }
        public string Name { set; get; }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj)) return true;

            if (obj == null) return false;

            Room otherRoom = (Room)obj;
            return otherRoom.ID == ID;
        }

        public override int GetHashCode()
        {
            return ID;
        }
    }
}