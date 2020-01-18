using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.XiTong.Models
{
    public class InspectorDetail
    {
        public int taskid { get; set; }
        public string taskcode { get; set; }
        public string taskname { get; set; }
        public string linecode { get; set; }
        public string linename { get; set; }
        public string startpole { get; set; }
        public string endpole { get; set; }
        public string createuser { get; set; }
        public DateTime? createtime { get; set; }
        public string taskstatus { get; set; }
        public DateTime? finishtime { get; set; }                   
        public string common { get; set; }
        public string inspetorname { get; set; }
        public int? linid { get; set; }
    }
}