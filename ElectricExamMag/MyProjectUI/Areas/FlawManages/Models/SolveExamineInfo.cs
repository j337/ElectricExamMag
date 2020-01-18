﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.FlawManages.Models
{
    public class SolveExamineInfo
    {
        public string taskcode { get; set; }
        public string taskname { get; set; }
        public string taskstatus { get; set; }
        public string excutename { get; set; }
        public string polecode { get; set; }
        public DateTime? finishtime { get; set; }
        public string issuedname { get; set; }
        public string taskmanagername { get; set; }
    }
}