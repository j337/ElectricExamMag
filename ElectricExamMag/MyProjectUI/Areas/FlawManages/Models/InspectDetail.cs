using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.FlawManages.Models
{
    public class InspectDetail
    {
        public string taskcode { get; set; }
        public string taskname { get; set; }
        public string excutename { get; set; }
        public string linecode { get; set; }
        public string starpolecode { get; set; }
        public string endpolecode { get; set; }
        public string issuedname { get; set; }
        public DateTime? issuedtime { get; set; }
        public string desc { get; set; }
    }
}