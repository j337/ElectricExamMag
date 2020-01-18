using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.FlawManages.Models
{
    public class BugTypeList
    {
        public int? id { get; set; }
        public string configValueName { get; set; }
        public int? isStart { get; set; }
        public bool check { get; set; }
    }
}