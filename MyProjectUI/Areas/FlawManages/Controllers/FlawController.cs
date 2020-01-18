using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyProjectUI.Areas.FlawManages.Models;
using Newtonsoft.Json;
using MyProjectUI.Models;
using System.IO;
using System.Data;
using System.Data.SqlClient;
namespace MyProjectUI.Areas.FlawManages.Controllers
{
    public class FlawController : Controller
    {
        // GET: FlawManages/Flaw
        static MyElectrCheck_DBDataContext mecd = new MyElectrCheck_DBDataContext();
        public ActionResult TestView()
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult CheckLogin()
        {
            #region 登录验证
            string acc = Request.Params["acc"];

            string pwd = Request.Params["pwd"];

            var result = from a in mecd.ps_user where a.userName == acc && a.userPassword == pwd select a;

            string msg = "用户名或密码错误!";

            if (result.ToList().Count > 0)
            {
                msg = "true";
                //定义三个Session分别保存userid、roleid、userCode
                Session["userid"] = result.ToArray()[0].id;
                Session["userCode"] = result.ToArray()[0].userCode;
                Session["roleid"] = result.ToArray()[0].roleId;
            }
            #endregion

            return Content(msg);
        }
        public ActionResult SelectFlawWithTable()
        {
            int pageindex= int.Parse(Request.Params["pageindex"]);

            int userid = int.Parse(Session["userid"].ToString());

            var user = (from u in mecd.ps_user
                        where u.id == userid
                        select u).FirstOrDefault();
            var role = (from r in mecd.ps_role
                        where r.id == user.roleId
                        select r).FirstOrDefault();

            List<WaitList> list = new List<WaitList>();
            //分情况判断 
            if (role.roleName == "系统管理员")
            {

                //则可以查看所有任务 
                //缺陷管理 
                var solvemanage = (from a in mecd.ps_inspectiontask_main
                                   join b in mecd.ps_inspectiontask_detail
                                   on a.id equals b.taskid
                                   where b.isBugLevel==0&&b.taskStatus==4&&a.taskStatus==4&&b.isBug==1&&b.isSolve==0
                                   select new { id = b.id,tasktype="缺陷管理", taskname = a.lineName + "杆塔编号" + b.poleCode + "缺陷等级确认", finishtime = a.issuedTime, type = 1 }).ToList();
                if (solvemanage.Count() > 0)
                {
                    foreach (var v in solvemanage)
                    {
                        WaitList wl = new WaitList()
                        {
                            id = v.id,
                            WaitType = v.tasktype,
                            WaitName = v.taskname,
                            WaitDate = v.finishtime,
                            Type = v.type
                        };
                        list.Add(wl);
                    }
                }

                //巡检任务 
                var inspectortask = (from a in mecd.ps_inspectiontask_main                                   
                                     where a.taskStatus==2||a.taskStatus==3
                                     select new { id = a.id, tasktype = "巡检任务", taskname = a.inspectionTaskName, finishtime = a.issuedTime, type = 2 }).ToList();
                if (inspectortask.Count() > 0)
                {
                    foreach (var v in inspectortask)
                    {
                        WaitList wl = new WaitList()
                        {
                            id = v.id,
                            WaitType = v.tasktype,
                            WaitName = v.taskname,
                            WaitDate = v.finishtime,
                            Type = v.type
                        };
                        list.Add(wl);
                    }
                }

                //消缺任务 
                var solvetask = (from a in mecd.ps_solvetask_main
                                 where a.taskStatus==2||a.taskStatus==3
                                 select new { id = a.id, tasktype = "消缺任务", taskname = a.solveTaskName, finishtime = a.issuedTime, type = 3 }).ToList();

                if (solvetask.Count() > 0)
                {
                    foreach (var v in solvetask)
                    {
                        WaitList wl = new WaitList()
                        {
                            id = v.id,
                            WaitType = v.tasktype,
                            WaitName = v.taskname,
                            WaitDate = v.finishtime,
                            Type = v.type
                        };
                        list.Add(wl);
                    }
                }

                //消缺审查 
                var solveexamine = (from a in mecd.ps_solvetask_main
                                    where a.taskStatus == 4 
                                    select new { id = a.id, tasktype = "消缺审查", taskname = a.solveTaskName, finishtime = a.finishTime, type = 4 }).ToList();

                if (solveexamine.Count() > 0)
                {
                    foreach (var v in solveexamine)
                    {
                        WaitList wl = new WaitList()
                        {
                            id = v.id,
                            WaitType = v.tasktype,
                            WaitName = v.taskname,
                            WaitDate = v.finishtime,
                            Type = v.type
                        };
                        list.Add(wl);
                    }
                }

              
            }
            else if (role.roleName == "线路管理员")
            {
                
                //如果登录的角色为线路管理员那则只显示缺陷管理 任务 
                var solvemanage = (from a in mecd.ps_inspectiontask_main
                                   join b in mecd.ps_inspectiontask_detail
                                   on a.id equals b.taskid
                                   where a.issuedByCode==user.userCode&&a.taskStatus==4&&b.taskStatus==4&&b.isBugLevel==0&&b.isBug==1&&b.isSolve==0
                                   select new { id = b.id, tasktype = "缺陷管理", taskname = a.lineName + "杆塔编号" + b.poleCode + "缺陷等级确认", finishtime = a.issuedTime, type = 1 }).ToList();
                 

                if (solvemanage.Count() > 0)
                {
                    foreach (var v in solvemanage)
                    {
                        WaitList wl = new WaitList()
                        {
                            id = v.id,
                            WaitType = v.tasktype,
                            WaitName = v.taskname,
                            WaitDate = v.finishtime,
                            Type = v.type
                        };
                        list.Add(wl);
                    }
                }

                //消缺审查 
                var solveexamine = (from a in mecd.ps_solvetask_main
                                    where a.taskStatus == 4 && (a.taskManagerCode == user.userCode || a.issuedByCode == user.userCode)
                                    select new { id = a.id, tasktype = "消缺审查", taskname = a.solveTaskName, finishtime = a.finishTime, type = 4 }).ToList();        
               
                if (solveexamine.Count() > 0)
                {
                    foreach (var v in solveexamine)
                    {
                        WaitList wl = new WaitList()
                        {
                            id = v.id,
                            WaitType = v.tasktype,
                            WaitName = v.taskname,
                            WaitDate = v.finishtime,
                            Type = v.type
                        };
                        list.Add(wl);
                    }
                }

               

            } else if (role.roleName == "巡检员")
            {
                //巡检任务 
                var inspectortask = (from a in mecd.ps_inspectiontask_main
                                     join c in mecd.ps_inspector_detail
                                     on a.id equals c.taskId
                                     where (a.taskStatus == 2 || a.taskStatus == 3) &&c.inspectorCode==user.userCode
                                     select new { id = a.id, tasktype = "巡检任务", taskname = a.inspectionTaskName, finishtime = a.issuedTime, type = 2 }).ToList();
                if (inspectortask.Count() > 0)
                {
                    foreach (var v in inspectortask)
                    {
                        WaitList wl = new WaitList()
                        {
                            id = v.id,
                            WaitType = v.tasktype,
                            WaitName = v.taskname,
                            WaitDate = v.finishtime,
                            Type = v.type
                        };
                        list.Add(wl);
                    }
                }

            }else if (role.roleName == "消缺员")
            {
                //消缺任务 
                var solvetask = (from a in mecd.ps_solvetask_main
                                 join c in mecd.ps_solver_detail
                                 on a.id equals c.taskId
                                 where (a.taskStatus == 2 || a.taskStatus == 3)&&c.solverCode==user.userCode
                                 select new { id = a.id, tasktype = "消缺任务", taskname = a.solveTaskName, finishtime = a.issuedTime, type = 3 }).ToList();

                if (solvetask.Count() > 0)
                {
                    foreach (var v in solvetask)
                    {
                        WaitList wl = new WaitList()
                        {
                            id = v.id,
                            WaitType = v.tasktype,
                            WaitName = v.taskname,
                            WaitDate = v.finishtime,
                            Type = v.type
                        };
                        list.Add(wl);
                    }
                }
            }

            DataSource ds = new DataSource();

            if (list.Count() > 0)
            {
                int skip = (pageindex - 1) * 6;

                ds.list = list.Skip(skip).Take(6).ToList();

                ds.pagecount = list.Count();
            }

            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }
    
        public ActionResult UpdateForForm()
        {
            #region 修改个人资料
            bool b = false;
            //获得params传过来的全部参数对象
            int age = int.Parse(Request.Params["age"]);
            string usern = Request.Params["usern"];
            string username = Request.Params["username"];
            string newpwd = Request.Params["newpwd"];
            string phone = Request.Params["phone"];
            string email = Request.Params["email"];
            string sex = Request.Params["sex"];
            DateTime? outdate;
            DateTime? indate;

            //先根据username查出ps_role表中职位名称对应的ROLE_ID
            var result2 = (from r in mecd.ps_role
                           where r.roleName == username
                           select r).FirstOrDefault();

            //创建连接字符串 
            SqlConnection con = new SqlConnection("server=.;database=MyElectrCheck_DB;Integrated Security=true");

            con.Open();

            //创建sql语句 
            string sql = "update ps_user set age="+age+",username='"+usern+"',roleid='"+result2.id+"',userpassword='"+newpwd+"',phonetel='"+phone+"',email='"+email+"',sex='"+sex+"' ";

            //获得Session中保存的ID转Int类型然后作为条件继续查
            int id = Convert.ToInt32(Session["userid"]);
           

            if (Request.Params["outdate"] != "")
            {
                outdate = Convert.ToDateTime(Request.Params["outdate"]);
               sql+= " ,dimissionDate='" + outdate+"'";
            }
            if (Request.Params["indate"] != "")
            {
                indate = Convert.ToDateTime(Request.Params["indate"]);
                sql += " ,entrydate='" + indate + "' ";
            }

            sql += " where id=" + id;

            //创建命令对象 
            SqlCommand cmd = new SqlCommand(sql, con);

            //执行命令对象 
            int count=cmd.ExecuteNonQuery();

            if (count>0)
            {
                b = true;
            }

            con.Close();
     
            #endregion

            return Json(b);
        }
        public ActionResult UpdateInfo()
        {
            return View();
        }
        public ActionResult Index2()
        {
            return View();
        }

        //缺陷管理列表
        public ActionResult SelectSolveManageDetail()
        {
            int taskid = int.Parse(Request.Params["taskid"]);

            var inspects = (from i in mecd.ps_inspectiontask_main
                            join b in mecd.ps_inspectiontask_detail
                            on i.id equals b.taskid
                            where b.id == taskid
                            select new {taskcode=i.inspectionTaskCode, linename=i.lineName,polecode=b.poleCode,issuedname=i.issuedByName,issuedtime=i.issuedTime,bugdesc=b.bugDesc }).FirstOrDefault();

            var taskname ="杆塔编号" + inspects.polecode+"缺陷等级确认";


            BugManage bm = new BugManage()
            {
                taskcode = inspects.taskcode,
                taskname = taskname,
                excutename = inspects.issuedname,
                polecode = inspects.polecode,
                issuedname = inspects.issuedname,
                issuedtime = inspects.issuedtime,
                desc = inspects.bugdesc
            };

            string jsonstr = JsonConvert.SerializeObject(bm);

            return Content(jsonstr);
        }

        //巡检任务列表
        public ActionResult SelectInspectTakeDetail()
        {
            int taskid = int.Parse(Request.Params["taskid"]);

            var inspects = (from a in mecd.ps_inspectiontask_main
                            join b in mecd.ps_inspectiontask_detail
                            on a.id equals b.taskid
                            where a.id == taskid
                            select new { taskcode = a.inspectionTaskCode, taskname = a.inspectionTaskName, linename = b.lineName, startpolecode = a.startPoleCode, endpolecode = a.endPoleCode, issuedname = a.issuedByName, issuedtime = a.issuedTime, bugdesc = a.common }).FirstOrDefault();

            //查询巡检员信息 
            var inspectors = (from i in mecd.ps_inspector_detail
                              where i.taskId == taskid
                              select i).ToList();

            string inspector = "";
            if (inspectors.Count() > 0)
            {
                foreach(var v in inspectors)
                {
                    inspector += v.inspectorName + ",";
                }

                var index = inspector.LastIndexOf(',');
                inspector = inspector.Substring(0, index);
            }

            InspectDetail id = new InspectDetail()
            {
                taskcode = inspects.taskcode,
                taskname = inspects.taskname,
                excutename = inspector,
                linecode = inspects.linename,
                starpolecode = inspects.startpolecode,
                endpolecode = inspects.endpolecode,
                issuedname = inspects.issuedname,
                issuedtime = inspects.issuedtime,
                desc = inspects.bugdesc
            };

            string jsonstr = JsonConvert.SerializeObject(id);

            return Content(jsonstr);
        }

        //消缺任务列表
        public ActionResult SelectSolveTakeDetail()
        {
            int taskid = int.Parse(Request.Params["taskid"]);

            var solvetask = (from a in mecd.ps_solvetask_main
                            join b in mecd.ps_solvetask_detail
                            on a.id equals b.taskId
                            where a.id==taskid
                            select new { taskcode = a.solveTaskCode, taskname = a.solveTaskName, issuedname = a.issuedByName, issuedtime = a.issuedTime, bugdesc = a.taskDesc }).FirstOrDefault();

            //查询消缺员信息 
            var inspectors = (from i in mecd.ps_solver_detail
                              where i.taskId == taskid
                              select i).ToList();

            string solvecodes = "";
            if (inspectors.Count() > 0)
            {
                foreach (var v in inspectors)
                {
                    solvecodes += v.solverName + ",";
                }

                var index = solvecodes.LastIndexOf(',');
                solvecodes = solvecodes.Substring(0, index);
            }

            //再查询待消缺编号 
            var ipoles = (from i in mecd.ps_solvetask_detail
                          join b in mecd.ps_inspectiontask_detail
                          on i.inspectionTaskDelId equals b.id
                          where i.taskId == taskid
                          select b).ToList();

            string polecodes = "";

            if (ipoles.Count() > 0)
            {
                foreach (var v in ipoles)
                {
                    polecodes += v.poleCode + ",";
                }

                var index = polecodes.LastIndexOf(',');
                polecodes = polecodes.Substring(0, index);
            }

            SolveDetail sd = new SolveDetail()
            {
                taskcode = solvetask.taskcode,
                taskname = solvetask.taskname,
                excutename = solvecodes,
                polecode = polecodes,               
                issuedname = solvetask.issuedname,
                issuedtime = solvetask.issuedtime,
                desc = solvetask.bugdesc
            };

            string jsonstr = JsonConvert.SerializeObject(sd);

            return Content(jsonstr);
        }
        public ActionResult SaveIdBySession()
        {
            Session["taskmainid"] = Request.Params["taskid"];

            return Json("true");
        }

        public ActionResult JurisdictionManage()
        {
            #region 下拉列表与角色登录权限的管理
            int id = Convert.ToInt32(Session["userid"]);

            var result = (from a in mecd.ps_user join b in mecd.ps_role on a.roleId equals b.id where a.id == id select new { b.roleName, a.entryDate, a.userName, a.phoneTel, a.email, a.dimissionDate, a.sex, a.age, a.userPassword }).FirstOrDefault();

            ps_role[] result2 = (from a in mecd.ps_role
                                 select a).ToArray();

            role[] role = new Models.role[result2.Length];

            for (int i = 0; i < result2.Length; i++)
            {
                Models.role r = new Models.role();
                r.label = result2[i].roleName;
                r.value = result2[i].roleName;
                if (result2[i].roleName == "系统管理员")
                {
                    if (result.roleName == "系统管理员")
                    {
                        r.selected = "selected";
                    }
                }
                else
                {
                    if (result.roleName == result2[i].roleName)
                    {
                        r.selected = "selected";
                    }
                }
                role[i] = r;
            }

            var result3 = new { result, role };

            string jsonstr = JsonConvert.SerializeObject(result3);

            #endregion

            return Content(jsonstr);
        }

        //消缺审查
        public ActionResult SelectSolveExamineDetail()
        {
            int taskid = int.Parse(Request.Params["taskid"]);

            //消缺主表信息 
            var solver = (from s in mecd.ps_solvetask_main
                          where s.id == taskid
                          select s).FirstOrDefault();

            //消缺杆号 
            var poles = (from p in mecd.ps_inspectiontask_detail
                         join b in mecd.ps_solvetask_detail
                         on p.id equals b.inspectionTaskDelId
                         where b.taskId == taskid
                         select p).ToList();

            string polestr = "";

            if (poles.Count() > 0)
            {
                foreach(var v in poles)
                {
                    polestr += v.poleCode + ",";
                }
                var index = polestr.LastIndexOf(',');
                polestr = polestr.Substring(0, index);
            }


            //执行者
            var solvers = (from a in mecd.ps_solver_detail
                           where a.taskId == taskid
                           select a).ToList();

            string strsolve = "";

            foreach(var v in solvers)
            {
                strsolve += v.solverName + ",";
            }

            var index2 = strsolve.LastIndexOf(',');

            strsolve = strsolve.Substring(0, index2);

            SolveExamineInfo sei = new SolveExamineInfo()
            {
                taskcode = solver.solveTaskCode,
                taskname = solver.solveTaskName,
                taskstatus=solver.taskStatusName,
                excutename = strsolve,
                polecode = polestr,
                finishtime = solver.finishTime,
                issuedname = solver.issuedByName,
                taskmanagername = solver.taskManagerName
            };

            string jsonstr = JsonConvert.SerializeObject(sei);

            return Content(jsonstr);
        }



    }
}