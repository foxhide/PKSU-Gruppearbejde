using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CalendarApplication.Models.EventType
{
    public enum Fieldtype
    {
        Float,
        Text,
        Datetime,
        User,
        Group,
        File,
        Bool,
        UserList,
        GroupList,
        FileList
    }

    public class FieldDataModel
    {
        public static List<string> DATA_TYPES
            = new List<string>
            {
                "Number",
                "Text",
                "Date",
                "User",
                "Group",
                "File",
                "Checkbox",
                "List of users",
                "List of groups",
                "List of files"
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
            this.RequiredCreate = false;
            this.RequiredApprove = false;
            this.Datatype = 0;
            this.ViewID = id;
        }

        public int ID { set; get; }

        [Required]
        [Display(Name = "Field name")]
        public string Name { set; get; }

        [Display(Name = "Description (max 100 characters)")]
        public string Description { set; get; }

        [Display(Name = "Required For Creation")]
        public bool RequiredCreate { set; get; }

        [Display(Name = "Required For Approval")]
        public bool RequiredApprove { set; get; }

        public Fieldtype Datatype { set; get; }
        public int VarcharLength { set; get; }

        public int ViewID { set; get; }

        public string GetDBType()
        {
            switch (this.Datatype)
            {
                case Fieldtype.Text: return "varchar("+this.VarcharLength+")";
                case Fieldtype.Datetime: return "datetime";
                case Fieldtype.Float: return "float";
                case Fieldtype.User: return "int"; //userId
                case Fieldtype.Group: return "int"; //groupId
                case Fieldtype.File: return "int";  //fileId
                case Fieldtype.Bool: return "tinyint(1)";    //Yes/No
                case Fieldtype.UserList:
                case Fieldtype.GroupList:
                case Fieldtype.FileList: return "tinyint(1)";  //Dummy type for lists
                default: return "int";
            }
        }

        public int GetTypeAsInt()
        {
            switch (this.Datatype)
            {
                case Fieldtype.Float: return 0;
                case Fieldtype.Text: return 1;
                case Fieldtype.Datetime: return 2;
                case Fieldtype.User: return 3;
                case Fieldtype.Group: return 4;
                case Fieldtype.File: return 5;
                case Fieldtype.Bool: return 6;
                case Fieldtype.UserList: return 7;
                case Fieldtype.GroupList: return 8;
                case Fieldtype.FileList: return 9;
                default: return -1;
            }
        }

    }
}