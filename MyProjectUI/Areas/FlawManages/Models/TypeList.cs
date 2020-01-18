using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.FlawManages.Models
{
    public class TypeList
    {
        public int? id { get; set; }
        public string inspectionTaskCode { get; set; }
        public string lineCode { get; set; }
        public string poleCode{get;set;}
        public int? bugLevel { get; set; }
        public string bugTypeName { get; set; }
        public double? intactRate { get; set; }
        public string bugDesc { get; set; }
        public DateTime? discoverTime { get; set; }
        public string discovererName { get; set; }
        public int? SelectValueId { get; set; }
    }
}