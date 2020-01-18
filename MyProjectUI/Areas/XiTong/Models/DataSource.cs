using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyProjectUI.Models;
namespace MyProjectUI.Areas.XiTong.Models
{
    public class DataSource
    {
        public List<ps_role> rolelist { get; set; }
        public int rolecount { get; set; }
        public List<Userinfo> userlist { get; set; }
        public int usercount { get; set; }
        public List<ps_logs> loglist { get; set; }
        public int logcount { get; set; }
        public int logpagecount { get; set; }
        public MyMenus mm { get; set; }
        public List<ps_inspectiontask_main> imlist { get; set; }
        public int pagecount { get; set; }
        public List<ps_systemconfig> syslist { get; set; }
        public InspectorDetail inspectordetails { get; set; }
        public List<ps_pole_info> polelist { get; set; }
        public ps_inspectiontask_detail taskdet { get; set; }
        public TaskBugDet bugdet { get; set; }
        public List<ps_user> pslist { get; set; }
        public List<ps_line_info> lilist { get; set; }
        public List<ps_inspector_detail> insdet { get; set; }
        public ps_inspectiontask_main taskmain { get; set; }
        public List<ps_user> userlist2 { get; set; }
        public List<ps_systemconfig> bugtypelist { get; set; }
        public List<ps_systemconfig> buglevellist { get; set; }
        public List<ps_inspectiontask_detail> taskdetails { get; set; }
        public List<ps_solvetask_main> smlist { get; set; }
        public string usercode { get; set; }
        public string rolename { get; set; }
        public List<ps_systemconfig> configlist { get; set; }
        public int configcount { get; set; }
        public List<InspectorSelect> islist { get; set; }

        public ps_user userinfo { get; set; }

      
    }
}