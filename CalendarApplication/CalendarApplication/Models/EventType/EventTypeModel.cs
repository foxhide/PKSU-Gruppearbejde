using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CalendarApplication.Models.EventType
{
    public class EventTypeModel
    {
        public int ID { set; get; }

        [Required]
        [Display(Name = "Event type name")]
        public string Name { set; get; }

        //Used for EventViewModel
        public bool Selected { set; get; }

        public List<FieldDataModel> TypeSpecific { set; get; }
    }
}