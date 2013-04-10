using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CalendarApplication.Models.EventType
{
    public class FieldDataModel
    {
        public static List<string> DATA_TYPES
            = new List<string>
            {
                "Integer",
                "Text",
                "Date",
                "User",
                "Group",
                "File",
                "Yes/No"
            };

        public FieldDataModel()
        {
            ID = -1;
        }

        public FieldDataModel(int id)
        {
            this.ID = -1;
            this.Name = "";
            this.Description = "";
            this.Required = false;
            this.Datatype = 0;
            this.ViewID = id;
        }

        public int ID { set; get; }

        [Display(Name = "Field name")]
        public string Name { set; get; }

        [Display(Name = "Description (max 100 characters)")]
        public string Description { set; get; }

        [Display(Name = "Required")]
        public bool Required { set; get; }

        public int Datatype { set; get; }
        public int VarcharLength { set; get; }

        public int ViewID { set; get; }

        public string GetDBType()
        {
            switch (this.Datatype)
            {
                case 1: return "varchar("+this.VarcharLength+")";
                case 2: return "datetime";
                case 0:
                case 3:
                case 4: return "int";
                case 6: return "varchar(100)";  //File
                case 7: return "tinyint(1)";    //Yes/No
                default: return "int";
            }
        }

    }
}