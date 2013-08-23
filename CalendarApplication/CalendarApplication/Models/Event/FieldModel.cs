using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CalendarApplication.Models.EventType;

namespace CalendarApplication.Models.Event
{
    public class FieldModel : FieldDataModel
    {
        public int IntValue { get; set; }
        public float? FloatValue { get; set; }
        public string StringValue { get; set; }
        public bool BoolValue { get; set; }
        public HttpPostedFileBase File { get; set; }
        public DateTime DateValue { get; set; }

        public List<SelectListItem> List { get; set; }
        public List<StringListModel> StringList { get; set; }

        public object GetDBValue()
        {
            switch (this.Datatype)
            {
                case Fieldtype.Float: return this.FloatValue;
                case Fieldtype.User: if (this.IntValue < 1) { return null; } else { return this.IntValue; } //int or null, userId
                case Fieldtype.Group: if (this.IntValue < 1) { return null; } else { return this.IntValue; } //int or null, groupId
                case Fieldtype.Text: return this.StringValue;
                case Fieldtype.File: if (this.IntValue < 1) { return null; } else { return this.IntValue; } //int or null, fileId
                case Fieldtype.Datetime: return this.DateValue;
                case Fieldtype.Bool: return this.BoolValue; //bool
                // Lists are handled in seperate tables, just insert dummy value - null is returned if no elements, used for
                // checking if list has selected items in checking the type specifics
                case Fieldtype.FileList: if (this.CheckList()) { return false; } else { return null; }
                case Fieldtype.GroupList: if (this.CheckList()) { return false; } else { return null; }
                case Fieldtype.UserList: if (this.CheckList()) { return false; } else { return null; }
                case Fieldtype.TextList: if (this.CheckStringList()) { return false; } else { return null; }
            }
            return "";
        }

        /// <summary>
        /// Checks if at least one element in the list is selected
        /// </summary>
        /// <returns>True if (at least) one selected, otherwise false</returns>
        public bool CheckList()
        {
            if (this.List != null)
            {
                foreach (SelectListItem sli in this.List)
                {
                    if (sli.Selected) { return true; }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if at least one element in the string list is set as active and has a value
        /// </summary>
        /// <returns>True if (at least) one active non-empty string, otherwise false</returns>
        public bool CheckStringList()
        {
            if (this.StringList != null)
            {
                foreach (StringListModel slm in this.StringList)
                {
                    if (slm.Active && !String.IsNullOrWhiteSpace(slm.Text)) { return true; }
                }
            }
            return false;
        }
    }
}