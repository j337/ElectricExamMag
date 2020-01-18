using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.XiTong.Models
{
    public class Userinfo
    {
        public int id { get; set; }
        public string usercode { get; set; }
        public string username { get; set; }
        public string rolename { get; set; }
        public string userpwd { get; set; }
        public string email { get; set; }
        public DateTime? lastlogintime { get; set; }
        public DateTime? overdate { get; set; }
        public DateTime? outdate { get; set; }
        public int? userstatus { get; set; }

    }
}