using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.InformationManage.Models
{
    public class DataSource
    {
        public int Pagecount { get; set; }
        public List<DataManager> list { get; set; }
    }
}