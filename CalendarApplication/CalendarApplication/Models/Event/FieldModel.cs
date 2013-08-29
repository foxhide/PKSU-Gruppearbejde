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
        public FileModel File { get; set; }
        public List<FileModel> FileList { get; set; }
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
                // need to check whether file exists or has an input when called from eventcontroller for required creation / approval
                // will not insert a fileId that is 0 when this is called from mysqlevent when inserting fileid into table
                case Fieldtype.File: if ((this.File.ID > 0 && this.File.Active) || this.File.InputFile != null) { return this.File.ID; } else { return null; } //int or null, fileId
                case Fieldtype.Datetime: return this.DateValue;
                case Fieldtype.Bool: return this.BoolValue; //bool
                // Lists are handled in seperate tables, just insert dummy value - null is returned if no elements, used for
                // checking if list has selected items in checking the type specifics
                case Fieldtype.FileList: if (this.CheckFileList()) { return false; } else { return null; }
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


        /// <summary>
        /// Checks if at least one file in the file list is active and has an input file or older file
        /// </summary>
        /// <returns>True if at least one active file, otherwise false</returns>
        public bool CheckFileList()
        {
            if (this.FileList != null)
            {
                foreach (FileModel fm in this.FileList)
                {
                    if (fm.Active && (fm.ID > 0 || fm.InputFile != null)) { return true; }
                }
            }
            return false;
        }
    }
}