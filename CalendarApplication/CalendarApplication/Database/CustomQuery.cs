using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CalendarApplication.Database
{
    public class CustomQuery
    {
        public string Cmd { set; get; }
        public string[] ArgNames { set; get; }
        public object[] Args { set; get; }
    }
}
