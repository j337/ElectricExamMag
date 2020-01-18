using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyProjectUI.Models;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using MyProjectUI.Areas.extinction.Models;
namespace MyProjectUI.Areas.extinction.Controllers
{
    public class SolveDetailController : Controller
    {
        MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();
        // GET: extinction/SolveDetail
        public ActionResult SolveDetailView()
        {
            return View();
        }
        //保存前端传递过来的id值 
        public ActionResult SaveIdBySession()
        {
            Session["solvetaskid"] = Request.Params["taskid"];
            return Json(" ");
        }

        //获得任务详细信息的action
        public ActionResult GetTaskDetailinfo()
        {
            //获得session中的值 
            int taskid = int.Parse(Session["solvetaskid"].ToString());

            int userid = int.Parse(Session["userid"].ToString());

            var user = (from u in medc.ps_user
                        where u.id == userid
                        select u).FirstOrDefault();


            //先获得消缺信息主表数据 
            var solvers = (from a in medc.ps_solvetask_main
                           where a.id == taskid
                           select a).FirstOrDefault();

            //再获得缺陷列表信息 
            var badlist = (from a in medc.ps_inspectiontask_detail
                           join b in medc.ps_solvetask_detail
                           on a.id equals b.inspectionTaskDelId 
                           where b.taskId == taskid
                           select a).ToList();

            //查询消缺员信息 
            var solverlist = (from a in medc.ps_solver_detail
                           where a.taskId == taskid
                           select a).ToList();

          

            DataSource ds = new DataSource();

            ds.taskdetlist = badlist;

            ds.taskmain = solvers;

            ds.solverlist = solverlist;

            ds.userinfo = user;

            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }
        //分配消缺员的action
        public ActionResult AddSolvetor()
        {
            int taskid = int.Parse(Request.Params["taskid"]);
            string strs = Request.Params["solvetors"];

            bool b = true;

            if (strs.IndexOf(',') > 0)
            {
                List<ps_solver_detail> des = new List<ps_solver_detail>();
                var user = (from u in medc.ps_user
                            where u.id == int.Parse(Session["userid"].ToString())
                            select u).FirstOrDefault();

                string[] strlist = strs.Split(',');

                for(var i = 0; i < strlist.Length; i++)
                {
                    var users = (from u in medc.ps_user
                                 where u.id == int.Parse(strlist[i])
                                 select u).FirstOrDefault();

                    ps_solver_detail solver = new ps_solver_detail()
                    {
                        taskId = taskid,
                        solverCode = users.userCode,
                        solverName = users.userName,
                        createdBy = user.userCode,
                        creationTime = DateTime.Now,
                        lastUpdateTime = DateTime.Now
                    };

                    des.Add(solver);
                }

                //添加数据到数据库 
                medc.ps_solver_detail.InsertAllOnSubmit(des);

                try
                {
                    medc.SubmitChanges();
                }
                catch (Exception)
                {
                    b = false;
                }

            }
            else
            {
              
                var user = (from u in medc.ps_user
                            where u.id == int.Parse(Session["userid"].ToString())
                            select u).FirstOrDefault();
     
                    var users = (from u in medc.ps_user
                                 where u.id == int.Parse(strs)
                                 select u).FirstOrDefault();

                    ps_solver_detail solver = new ps_solver_detail()
                    {
                        taskId = taskid,
                        solverCode = users.userCode,
                        solverName = users.userName,
                        createdBy = user.userCode,
                        creationTime = DateTime.Now,
                        lastUpdateTime = DateTime.Now
                    };

                   
                //添加数据到数据库 
                medc.ps_solver_detail.InsertOnSubmit(solver);

                try
                {
                    medc.SubmitChanges();
                }
                catch (Exception)
                {
                    b = false;
                }

            }

            if (b)
            {
                //成功之后修改主表中的数据 
                var solevtor = (from a in medc.ps_solvetask_main
                                where a.id == taskid
                                select a).FirstOrDefault();

                //修改数据 
                solevtor.taskStatus = 2;
                solevtor.taskStatusName = "已分配";
                solevtor.lastUpdateTime = DateTime.Now;

                try
                {
                    medc.SubmitChanges();
                }
                catch (Exception)
                {
                    b = false;
                }
            }
            return Json(b);
        }

        //取消任务 
        public ActionResult CancleTask()
        {
            int id = int.Parse(Request.Params["taskid"]);
            int iscancle = int.Parse(Request.Params["iscancle"]);

            var solver = (from a in medc.ps_solvetask_main
                          where a.id == id&&a.taskStatus==1
                          select a).FirstOrDefault();

            solver.isCancel = iscancle;

            solver.lastUpdateTime = DateTime.Now;

            bool b = true;

            try
            {
                medc.SubmitChanges();
            }
            catch (Exception)
            {
                b = false;
            }

            return Json(b);
        }

        public ActionResult UpdSolveTaskView()
        {
            return View();
        }

        public ActionResult GetUpdTaskInfo()
        {
            //获得session中保存的taskid
            int taskid = int.Parse(Session["solvetaskid"].ToString());

            int pageindex = int.Parse(Request.Params["pageindex"]);
            //获得消缺员信息 
            var role = (from i in medc.ps_role
                        where i.roleName == "消缺员" && i.isStart == 1
                        select i).FirstOrDefault();

            var role2 = (from a in medc.ps_role
                         where a.roleName == "线路管理员" && a.isStart == 1
                         select a).FirstOrDefault();

            //先获得消缺信息主表数据 
            var solvers = (from a in medc.ps_solvetask_main
                           where a.id == taskid
                           select a).FirstOrDefault();

            //再获得缺陷列表信息 
            var badlist = (from a in medc.ps_inspectiontask_detail
                           join b in medc.ps_solvetask_detail
                           on a.id equals b.inspectionTaskDelId
                           where b.taskId == taskid
                           select a).ToList();

            //获得已选的消缺员 
            var solvetor = (from a in medc.ps_user
                            join b in medc.ps_solver_detail
                            on a.userCode equals b.solverCode
                            where a.roleId == role.id && a.userStatus == 1
                            select a).ToList();

            //再获得未选的消缺员 
            string sql = "select *from ps_user where 1=1 and userstatus=1 and roleid=(select id from ps_role where rolename='消缺员')";

            foreach (var v in solvetor)
            {
                sql += " and usercode!='" + v.userCode + "'";

            }

            SqlConnection con = new SqlConnection("server=.;database=MyElectrCheck_DB;Integrated Security=true;");

            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
            SqlCommand cmd = new SqlCommand(sql, con);

            SqlDataReader dr = cmd.ExecuteReader();

            List<ps_user> userlist = new List<ps_user>();

            while (dr.Read())
            {
                ps_user user = new ps_user()
                {
                    id = int.Parse(dr["id"].ToString()),
                    userCode = dr["userCode"].ToString(),
                    userName = dr["userName"].ToString()
                };

                userlist.Add(user);
            }

            dr.Close();
          
            //获得工作单据类型信息 
            var worklist = (from a in medc.ps_systemconfig
                            where a.configCode == "DOC_TYPE" && a.isStart == 1
                            select a).ToList();

            //获得任务负责人信息
            var tasklist = (from a in medc.ps_user
                            where a.roleId == role2.id && a.userStatus == 1
                            select a).ToList();

            //获得缺陷列表信息   首先获得没有进行消缺任务的缺陷列表信息 
            var solvelist = (from s in medc.ps_solvetask_detail
                             select s).ToList();

            string strs = "";

            sql = "select *from ps_inspectiontask_detail where issolve=1 and isbug=1 and taskstatus=4 and isBugLevel=1 ";

            if (solvelist.Count() > 0)
            {
                foreach (var v in solvelist)
                {
                    sql += " and id!=" + v.inspectionTaskDelId;
                    strs += v.inspectionTaskDelId + ",";
                }
                var index = strs.LastIndexOf(',');  //123456,
                strs = strs.Substring(0, index);
                Session["strs"] = strs;
            }

            SqlCommand cmd2 = new SqlCommand(sql, con);
            dr = cmd2.ExecuteReader();


            List<ps_inspectiontask_detail> idlist = new List<ps_inspectiontask_detail>();
            while (dr.Read())
            {
                ps_inspectiontask_detail id = new ps_inspectiontask_detail()
                {
                    id = int.Parse(dr["id"].ToString()),
                    lineCode = dr["lineCode"].ToString(),
                    poleCode = dr["poleCode"].ToString(),
                    bugLevelName = dr["bugLevelName"].ToString(),
                    bugTypeName = dr["bugTypeName"].ToString(),
                    bugDesc = dr["bugDesc"].ToString(),
                    discovererName = dr["discovererName"].ToString(),
                    discoverTime = Convert.ToDateTime(dr["discoverTime"].ToString())
                };
                idlist.Add(id);
            }

            dr.Close();
            con.Close();

            DataSource ds = new DataSource();

            ds.userlist1 = userlist;
            ds.userlist2 = solvetor;
            ds.userlist3 = tasklist;
            ds.syslist = worklist;

            ds.updtaskdetlist = badlist;

            ds.taskmain = solvers;

            int datacount = idlist.Count();
            int pagecount = (int)Math.Ceiling(datacount / 4.0);
            if (pageindex <= pagecount && pageindex > 0)
            {
                int skip = (pageindex - 1) * 4;
                ds.taskdetlist = idlist.Skip(skip).Take(4).ToList();
                ds.datacount = pagecount;
            }
            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }

        public ActionResult GetUpdSolvetors()
        {
            int pageindex = int.Parse(Request.Params["pageindex"]);
            int taskid = int.Parse(Request.Params["taskid"]);
      
                    //获得缺陷列表信息   首先获得没有进行消缺任务的缺陷列表信息 
                    var solvelist = (from s in medc.ps_solvetask_detail
                                     join b in medc.ps_solvetask_main
                                     on s.taskId equals b.id
                                     where b.taskStatus!=5
                                     select s).ToList();

            var solvelist2 = (from s in medc.ps_solvetask_detail
                              where s.taskId!=taskid
                             select s).ToList();
            if (solvelist.Count() > 0) {
                if (solvelist2.Count() >0)
                {
                    var str = "";
                    foreach (var v in solvelist)
                    {

                        if (v.taskId != taskid)
                        {
                            str += v.inspectionTaskDelId + ",";
                        }
                    }
                    var index = str.LastIndexOf(',');  //123456,
                    str = str.Substring(0, index);
                    Session["strs"] = str;
                }
                else
                {
                    Session["strs"] = null;
                }
                    }
           

            string strs = "";
            //获得session中保存的值 
            string sql = "select *from ps_inspectiontask_detail where issolve=0 and isbug=1 and taskstatus=4 and isBugLevel=1 ";

            if (Session["strs"] != null)
            {
                string sessstrs = Session["strs"].ToString();
                if (sessstrs.IndexOf(',') >= 0)
                {
                   
                    string[] strlist = sessstrs.Split(',');
                    for (var i = 0; i < strlist.Length; i++)
                    {
                        sql += " and id!=" + strlist[i];
                    }
                }
                else
                {
                    sql += " and id!=" + sessstrs;
                }
            }
            if (Request.Params["strs"] != "")
            {
                strs = Request.Params["strs"];
                if (strs.IndexOf(',') >= 0)
                {
                    string[] strlist2 = strs.Split(',');
                    for (var i = 0; i < strlist2.Length; i++)
                    {
                        sql += " and id!=" + strlist2[i];
                    }
                }
                else
                {
                    sql += " and id!=" + strs;
                }
            }

            SqlConnection con = new SqlConnection("server=.;database=MyElectrCheck_DB;Integrated Security=true;");
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            SqlCommand cmd = new SqlCommand(sql, con);

            SqlDataReader dr = cmd.ExecuteReader();


            List<ps_inspectiontask_detail> idlist = new List<ps_inspectiontask_detail>();
            while (dr.Read())
            {
                ps_inspectiontask_detail id = new ps_inspectiontask_detail()
                {
                    id = int.Parse(dr["id"].ToString()),
                    lineCode = dr["lineCode"].ToString(),
                    poleCode = dr["poleCode"].ToString(),
                    bugLevelName = dr["bugLevelName"].ToString(),
                    bugTypeName = dr["bugTypeName"].ToString(),
                    bugDesc = dr["bugDesc"].ToString(),
                    discovererName = dr["discovererName"].ToString(),
                    discoverTime = Convert.ToDateTime(dr["discoverTime"].ToString())
                };
                idlist.Add(id);
            }
            dr.Close();
            con.Close();

            DataSource ds = new DataSource();

            int datacount = idlist.Count();
            int pagecount = (int)Math.Ceiling(datacount / 4.0);
            if (pageindex <= pagecount && pageindex > 0)
            {
                int skip = (pageindex - 1) * 4;
                ds.taskdetlist = idlist.Skip(skip).Take(4).ToList();
                ds.datacount = pagecount;
            }
            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }

        //修改消缺任务的action
        public ActionResult UpdSolveTask()
        {
            int taskid = int.Parse(Request.Params["taskid"]);
            string taskcode = Request.Params["taskcode"];
            string taskname = Request.Params["taskname"];
            //工作单据编号 
            int worktypeid = int.Parse(Request.Params["worktype"]);
            //任务负责人code
            string taskmanagercode = Request.Params["taskmanagercode"];
          
            //任务描述
            string taskdesc = Request.Params["taskdesc"];
            //备注
            string remark = Request.Params["remark"];

            string solverid = Request.Params["solvetorsid"];
            string inspectlist = Request.Params["inspectlist"];

            //首先查询出要修改的一条数据 
            var solvetask = (from a in medc.ps_solvetask_main
                             where a.id == taskid
                             select a).FirstOrDefault();

            //查询出ps_systemconfig表中对应的worktypeid
            var sys = (from a in medc.ps_systemconfig
                       where a.configCode == "DOC_TYPE" && a.configValueId == worktypeid
                       select a).FirstOrDefault();

            //再查询对应的负责人信息 
            var user = (from u in medc.ps_user
                        where u.userCode == taskmanagercode
                        select u).FirstOrDefault();

            var users = (from u in medc.ps_user
                         where u.id == int.Parse(Session["userid"].ToString())
                         select u).FirstOrDefault();

            //再修改数据 
            solvetask.solveTaskCode = taskcode;
            solvetask.solveTaskName = taskname;
            solvetask.workDocType = sys.configValueId;
            solvetask.workDocTypeName = sys.configValueName;
            solvetask.taskManagerCode = user.userCode;
            solvetask.taskManagerName = user.userName;
            solvetask.taskDesc = taskdesc;
            solvetask.common = remark;
            solvetask.lastUpdateTime = DateTime.Now;
            solvetask.taskStatus = 2;
            solvetask.taskStatusName = "已分配";
            solvetask.lastUpdateTime = DateTime.Now;

            bool b = true;

            try
            {
                medc.SubmitChanges();
            }
            catch (Exception)
            {
                b = false;
            }

            if (b)
            {
                //主表数据修改成功之后再修改子表中的数据 
                //首先删除消缺子表中的数据
                var sol = (from a in medc.ps_solvetask_detail
                           where a.taskId == taskid
                           select a).ToList();

                medc.ps_solvetask_detail.DeleteAllOnSubmit(sol);

                try
                {
                    medc.SubmitChanges();
                }
                catch (Exception)
                {
                    b = false;
                }

                if (b)
                {
                    List<ps_solvetask_detail> solvelist = new List<ps_solvetask_detail>();
                    //删除成功之后 再向消缺任务子表中添加新的缺陷信息 
                    if (inspectlist.IndexOf(",") > 0)
                    {

                        string[] solveidlist = inspectlist.Split(',');
                        for(var i = 0; i < solveidlist.Length; i++)
                        {
                            ps_solvetask_detail solve = new ps_solvetask_detail()
                            {
                                taskId = taskid,
                                inspectionTaskDelId = int.Parse(solveidlist[i]),
                                createdBy = users.userCode,
                                creationTime = DateTime.Now,
                                lastUpdateTime = DateTime.Now
                            };
                            solvelist.Add(solve);

                        }
                    }
                    else
                    {
                        ps_solvetask_detail solve = new ps_solvetask_detail()
                        {
                            taskId = taskid,
                            inspectionTaskDelId = int.Parse(inspectlist),
                            createdBy = users.userCode,
                            creationTime = DateTime.Now,
                            lastUpdateTime = DateTime.Now
                        };
                        solvelist.Add(solve);

                    }

                    medc.ps_solvetask_detail.InsertAllOnSubmit(solvelist);

                    try
                    {
                        medc.SubmitChanges();
                    }
                    catch (Exception)
                    {

                        b = false;
                    }

                    if (b)
                    {
                        List<ps_solver_detail> solvetorlist = new List<ps_solver_detail>();
                        //删除成功之后 再向消缺任务子表中添加新的缺陷信息 
                        if (solverid.IndexOf(",") > 0)
                        {

                            string[]  solvtoridlist = solverid.Split(',');
                            for (var i = 0; i < solvtoridlist.Length; i++)
                            {
                                var usersolver = (from a in medc.ps_user
                                                  where a.id == int.Parse(solvtoridlist[i])
                                                  select a).FirstOrDefault();

                                ps_solver_detail solve = new ps_solver_detail()
                                {
                                    taskId = taskid,
                                     solverCode= usersolver.userCode,
                                     solverName=usersolver.userName,
                                    createdBy = users.userCode,
                                    creationTime = DateTime.Now,
                                    lastUpdateTime = DateTime.Now,
                                    
                                };
                                solvetorlist.Add(solve);

                            }
                        }
                        else
                        {
                            var usersolver = (from a in medc.ps_user
                                              where a.id == int.Parse(solverid)
                                              select a).FirstOrDefault();
                            ps_solver_detail solve = new ps_solver_detail()
                            {
                                taskId = taskid,
                                solverCode = usersolver.userCode,
                                solverName = usersolver.userName,
                                createdBy = users.userCode,
                                creationTime = DateTime.Now,
                                lastUpdateTime = DateTime.Now,
                            };
                            solvetorlist.Add(solve);

                        }

                        medc.ps_solver_detail.InsertAllOnSubmit(solvetorlist);

                        try
                        {
                            medc.SubmitChanges();
                        }
                        catch (Exception)
                        {

                            b = false;
                        }
                    }
                } 

            }

            return Json(b);
        }

        public ActionResult TaskExmaine()
        {
            return View();
        }
        public ActionResult UpdExmaineSuggestion()
        {
            int taskid = int.Parse(Request.Params["taskid"]);
            string ispass ="";
            string managersuggestion = "";
            string isuedsuggstion = "";

            var solver = (from s in medc.ps_solvetask_main
                          where s.id == taskid
                          select s).FirstOrDefault();

            bool b = true;
         
            if (Request.Params["managersuggestion"] != "")
            {
                managersuggestion = Request.Params["managersuggestion"];
                solver.managerSuggestion = managersuggestion;
               
            }
            if (Request.Params["isuedsuggstion"] != "")
            {
                isuedsuggstion = Request.Params["isuedsuggstion"];
                solver.issuedSuggestion = isuedsuggstion;
              
            }
            if (Request.Params["ispass"] != "")
            {
                ispass = Request.Params["ispass"];

                if (ispass == "1")
                {
                    solver.taskStatus = 5;
                    solver.taskStatusName = "审查通过";
                    solver.lastUpdateTime = DateTime.Now;

                    try
                    {
                        medc.SubmitChanges();
                    }
                    catch (Exception)
                    {
                        b = false;
                    }

                    if (b)
                    {
                        var inspdetails = (from i in medc.ps_inspectiontask_detail
                                           where i.id == taskid
                                           select i).ToList();

                        //修改数据 
                        foreach(var v in inspdetails)
                        {
                            v.isSolve = 1;
                        }

                        //保存数据 
                        try
                        {
                            medc.SubmitChanges();
                        }
                        catch (Exception)
                        {
                            b = false;
                        }
                    }

                }
                else
                {
                    solver.taskStatus = 6;
                    solver.taskStatusName = "驳回";
                    solver.lastUpdateTime = DateTime.Now;

                    try
                    {
                        medc.SubmitChanges();
                    }
                    catch (Exception)
                    {
                        b = false;
                    }
                }
            }

          

            return Json(b);
        }
    }
}