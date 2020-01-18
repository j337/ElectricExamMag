using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.XiTong.Models
{
    public class TaskBugDet
    {
        public int taskid { get; set; }
        public string linecode { get; set; }
        public string polecode { get; set; }
        public int? isbug { get; set; }
        public double? intactRate { get; set; }
        public int? bugtype { get; set; }
        public int? buglevel { get; set; }
        public string bugTypeName { get; set; }
        public string bugLevelName { get; set; }
        public string bugDesc { get; set; }
        public DateTime? inspectTime { get; set; }
        public string inspectorName { get; set; }
        public string discovererName { get; set; }
        public DateTime? discoverTime { get; set; }
        public string issuedByName { get; set;}
        public DateTime? issuedTime { get; set; }
        
    }
}