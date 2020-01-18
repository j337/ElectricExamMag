using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.InformationManage.Models
{
    public class DataCount
    {
        public List<DataEliminate> plist { get; set; }
        public int PageCount { get; set; }
    }
}