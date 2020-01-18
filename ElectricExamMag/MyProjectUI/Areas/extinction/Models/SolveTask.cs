using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.extinction.Models
{
    public class SolveTask
    {
        public int id { get; set; }
        public string taskcode { get; set; }
        public string taskname { get; set; }
        public int workdoctype { get; set; }
        public string workdoctypename { get; set; }
        public string issuedbycode { get; set; }
        public string issuedbyname { get; set; }
        public DateTime issuedtime { get; set; }
        public int taskstatus { get; set; }
        public DateTime finishtime { get; set; }
        public int iscancle { get; set; }

    }
}