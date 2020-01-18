using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.InformationManage.Models
{
    public class DataManager
    {
        public int id { get; set; }
        public string inspectionTaskCode { get; set; }
        public string inspectionTaskName { get; set; }
        public string lineCode { get; set; }
        public string startPoleCode { get; set; }
        public string endPoleCode { get; set; }
        public string poleCode { get; set; }
        public int? bugLevel { get; set; }
        public string buglevelname { get; set; }
        public string bugtypename { get; set; }
        public int? bugType { get; set; }
        public DateTime? discoverTime { get; set; }
        public string discoverTime2 { get; set; }
        public string bugDesc { get; set; }
    }
}