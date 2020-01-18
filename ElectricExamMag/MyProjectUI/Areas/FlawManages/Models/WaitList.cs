using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectUI.Areas.FlawManages.Models
{
    public class WaitList
    {
        public int? id { get; set; }
        public string WaitType { get; set; }
        public string WaitName { get; set; }
        public DateTime? WaitDate { get; set; }
        public int? Type { get; set; }
    }
}