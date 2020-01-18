using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.FlawManages.Models
{
    public class BugPage
    {
        public int? pagecount { get; set; }
        public int? pagesize { get; set; }
        public BugTypeList[] btl { get; set; }
    }
}