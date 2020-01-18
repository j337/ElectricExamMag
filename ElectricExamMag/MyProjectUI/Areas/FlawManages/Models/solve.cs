using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.FlawManages.Models
{
    public class solve
    {
        public string solveTaskCode { get; set; }
        public string taskDesc { get; set; }
        public string issuedByName { get; set; }
        public string createdBy { get; set; }
        public DateTime? creationTime { get; set; }
        public string workDocTypeName { get; set; }
        public string poleCode { get; set; }
    }
}