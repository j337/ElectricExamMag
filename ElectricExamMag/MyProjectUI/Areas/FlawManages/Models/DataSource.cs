using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.FlawManages.Models
{
    public class DataSource
    {
        public List<WaitList> list { get; set; }
        public int pagecount { get; set; }
    }
}