using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace CalendarApplication.Models.Event
{
    public class FileModel
    {
        public int ID { get; set; }

        // uploader and event details when showing file list to admins
        public int UploaderID { get; set; }
        public string UploaderName { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; }
        public DateTime Uploaded { get; set; }

        public HttpPostedFileBase InputFile { get; set; }
        public string CurrentFileName { get; set; }
        
        public bool Active { get; set; }
        public bool Delete { get; set; }

        // Place in filelist (for javascript and html)
        public int Place { get; set; }

        //ID for html and javascript functions with filelists
        public string ViewID { get; set; }

        //Name for html and javascript with filelists
        public string ViewName { get; set; }
    }
}