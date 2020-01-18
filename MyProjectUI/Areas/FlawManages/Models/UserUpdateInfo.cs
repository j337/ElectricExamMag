using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.FlawManages.Models
{
    public class UserUpdateInfo
    {
        public int? age { get; set; }
        public string usern { get; set; }
        public string username { get; set; }
        public string newpwd { get; set; }
        public DateTime? indate { get; set; }
        public DateTime? outdate { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string sex { get; set; }
    }
}