using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyProjectUI.Models;
namespace MyProjectUI.Areas.Circuitmanage.Models
{
    public class DataSource
    {
        public List<ps_line_info> datalist { get; set; }
        public int linecount { get; set; }
        public List<ps_pole_info> polelist { get; set; }
        public int polecount { get; set; }
    }
}