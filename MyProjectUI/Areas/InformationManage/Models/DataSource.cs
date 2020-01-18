using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyProjectUI.Areas.InformationManage.Models;
using MyProjectUI.Models;
namespace MyProjectUI.Areas.InformationManage.Models
{
    public class DataSource
    {
        public int Pagecount { get; set; }
        public List<DataManager> list { get; set; }
        public List<ps_systemconfig> bugtypelist { get; set; } //缺陷类型集合 
        public List<ps_systemconfig> buglevellist { get; set; } //缺陷级别集合 

    }
}