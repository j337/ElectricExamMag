using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyProjectUI.Models;
namespace MyProjectUI.Areas.extinction.Models
{
    public class DataSource
    {
        public List<SolveTask> stlist { get; set; }
        public List<ps_solvetask_main> taskmainlist { get; set; }
        public List<ps_systemconfig> syslist { get; set; }
        public int datacount { get; set; }
        public List<ps_user> userlist1 { get; set; }  //未选消缺员
        public List<ps_user> userlist2 { get; set; }  //已选消缺员
        public List<ps_user> userlist3 { get; set; }   //任务负责人
        public List<ps_inspectiontask_detail> taskdetlist { get; set; }
        public ps_solvetask_main taskmain { get; set; }
        public List<ps_solver_detail> solverlist { get; set; }
        public List<ps_inspectiontask_detail> updtaskdetlist { get; set; }
        public List<ps_user> userlist4 { get; set; }
        public List<ps_systemconfig> worktypelist { get; set; }
        public List<SolveRecore> srlist { get; set; }
        public List<ps_systemconfig> buglevellist { get; set; }
        public List<ps_systemconfig> bugtypelist { get; set; }
        public List<ps_systemconfig> taskstatuslist { get; set; }
        public ps_user userinfo { get; set; }

        public ps_role roleinfo { get; set; }
      
    }
}