using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CalendarApplication.Models.EventType
{
    public class FieldDataModel
    {
        public static List<string> Datatypes
            = new List<string>
            {
                "Integer",
                "Text",
                "User",
                "Group",
                "File"
            };

        public int ID { set; get; }

        [Display(Name = "Field name")]
        public string Name { set; get; }

        [Display(Name = "Description (max 100 characters)")]
        public string Description { set; get; }

        [Display(Name = "Required")]
        public bool Required { set; get; }

        public int Datatype { set; get; }
    }
}