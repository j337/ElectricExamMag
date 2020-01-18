using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.InformationManage.Models
{
    public class DataEliminate
    {
        public int id { get; set; }
        public string solveTaskCode { get; set; }
        public string solveTaskName { get; set; }
        public string lineCode { get; set; }
        public string startPoleCode { get; set; }
        public string endPoleCode { get; set; }
        public string poleCode { get; set; }
        public int? isBug { get; set; }
        public string BugName { get; set; }
        public string bugLevelName { get; set; }
        public string bugTypeName { get; set; }
        public DateTime? finishTime { get; set; }
        public DateTime? discoverTime { get; set; }
        public string finishTime2 { get; set; }
        public string discoverTime2 { get; set; }
        public string bugDesc { get; set; }
    }
}