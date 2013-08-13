using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace CalendarApplication.Models.Group
{
    public class ApplicantListModel
    {

        public List<ApplicantGroupModel> ApplicationGroupList { get; set; }

        public class ApplicantGroupModel
        {

            public int GroupID { get; set; }

            [Display(Name = "Group Name")]
            public string GroupName { get; set; }

            public List<ApplicantModel> ApplicantList { get; set; }

            public class ApplicantModel
            {
                public int UserID { get; set; }

                [Display(Name = "Name")]
                public string UserName { get; set; }

                [Display(Name = "Accept")]
                public bool Accept { get; set; }

                [Display(Name = "Make leader")]
                public bool MakeLeader { get; set; }

                [Display(Name = "Make creator")]
                public bool MakeCreator { get; set; }

                [Display(Name = "Reject (overrides accept)")]
                public bool Delete { get; set; }
            }
        }
    }
}