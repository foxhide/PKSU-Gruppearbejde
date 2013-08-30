using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CalendarApplication.Models.Event;

namespace CalendarApplication.Models.User
{
    public class FileListModel
    {
        public List<FileModel> Files { get; set; }

        public FileListModel()
        {
            this.Files = new List<FileModel>();
        }
    }
}