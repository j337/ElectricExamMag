using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.XiTong.Models
{
    public class InspectorSelect
    {
        public string taskcode { get; set; }
        public string linecode { get; set; }
        public string polecode { get; set; }
        public string bugtypename { get; set; }
        public string buglevelname { get; set; }
        public string discovername { get; set; }
        public DateTime? discovertime { get; set; }
        public string issuedname { get; set; }
        public DateTime? issuedtime { get; set; }
        public double? intactrate { get; set; }
        public string bugdesc { get; set; }
    }
}