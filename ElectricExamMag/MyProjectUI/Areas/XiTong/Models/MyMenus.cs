using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.XiTong.Models
{
    public class MyMenus
    {
        public int id { get; set; }
        public string funcode{ get; set; }
        public string funname { get; set; }
        public string funurl { get; set; }
        public int? parentid { get; set; }
        public string parentname { get; set; }
        public int? isStart { get; set; }
        public List<MyMenus> MMlist { get; set; }

        //无参构造方法 
        public MyMenus()
        {
            this.MMlist = new List<MyMenus>();
        }

    }
}