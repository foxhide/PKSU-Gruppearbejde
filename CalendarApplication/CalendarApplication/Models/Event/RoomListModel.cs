using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CalendarApplication.Models.Event
{
    public class RoomListModel
    {
        public List<Room> RoomList { get; set; }
    }
}