using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.FlawManages.Models
{
    public class inspectiontask
    {
        public string inspectionTaskCode { get; set; }
        public string issuedByName { get; set; }
        public string inspectionTaskName { get; set; }
        public string lineName { get; set; }
        public string startPoleCode { get; set; }
        public string endPoleCode { get; set; }
        public string createdBy { get; set; }
        public DateTime? creationTime { get; set; }
        public string common { get; set; }
    }
}