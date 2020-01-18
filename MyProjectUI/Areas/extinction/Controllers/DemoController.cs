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
    public class DemoController : Controller
    {
        MyElectrCheck_DBDataContext medb = new MyElectrCheck_DBDataContext();
        // GET: extinction/Demo
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult TestView()
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Login1()
        {
            var userName = Request.Params["userName"];
            var userPass = Request.Params["userPass"];
          
            var result = (from u in medb.ps_user
                          where userName == u.userName && userPass == u.userPassword
                          select u).FirstOrDefault();
            bool b = false;
            if (result!=null)
            {
                b = true;

                Session["userid"] = result.id;
            }
            else
            {
                b = false;
            }
            return Json(b);
        }


        //消缺任务管理主页面
        public ActionResult MissingHomePage()
        {
            return View(); 
        }
    
        public ActionResult GetSolvetData()
        {
            int pageindex = int.Parse(Request.Params["pageindex"]);

            int skip = (pageindex - 1) * 6;

            var result = (from a in medb.ps_solvetask_main
                          where a.isCancel==0
                          select a).ToList();
            //任务状态 
            var taskstatus = (from a in medb.ps_systemconfig
                              where a.configCode == "SOLVETASK_STATUS"
                              select a).ToList();

            //获得消缺员信息 
            var role = (from i in medb.ps_role
                        where i.roleName == "消缺员" && i.isStart == 1
                        select i).FirstOrDefault();

            //获得已选的消缺员 
            var solvetor = (from a in medb.ps_user
                            join b in medb.ps_solver_detail
                            on a.userCode equals b.solverCode
                            join c in medb.ps_solvetask_main
                           on b.taskId equals c.id
                            where a.roleId == role.id && a.userStatus == 1&&c.taskStatus!=5
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
            //关闭连接 
            dr.Close();
            con.Close();

            //获得工作单据信息 
            var worktype = (from s in medb.ps_systemconfig
                            where s.configCode == "DOC_TYPE"
                            select s).ToList();

            DataSource ds = new DataSource();

            ds.taskmainlist = result.Skip(skip).Take(6).ToList();

            ds.syslist = taskstatus;

            ds.userlist1 = userlist;

            ds.worktypelist = worktype;

            ds.datacount = result.Count();

            string jsonstr = JsonConvert.SerializeObject(ds);

             return Content(jsonstr);
        }

        /// <summary>
        /// 多条件查询 
        /// </summary>
        /// <returns></returns>
         public ActionResult SelectData()
        {
            string taskcode = "";
            string worktype = "";
            string createuser = "";
            string time1 = "";
            string time2 = "";
            string taskstatus = "";
            int pageindex = int.Parse(Request.Params["pageindex"]);
             
            int skip = (pageindex - 1) * 6;

            string sql = "select *from ps_solvetask_main  where 1=1 and isCancel=0 ";
            if (Request.Params["taskcode"] != "")
            {
                taskcode = Request.Params["taskcode"];
                sql += " and solveTaskCode like '%" + taskcode + "%' ";
            }
          
            if (Request.Params["worktypename"] != "")
            {
                worktype= Request.Params["worktypename"];
                sql += " and workDocTypeName='" + worktype + "' ";
            }
            if (Request.Params["createby"] != "")
            {
                createuser = Request.Params["createby"];
                sql += "and issuedByName like '%" + createuser + "%'";
            }
            if (Request.Params["taskstatus"] != "")
            {
                taskstatus = Request.Params["taskstatus"];
                sql += "and taskStatusName='" + taskstatus + "'";
            }
            if (Request.Params["time1"] != null)
            {
                time1 = Request.Params["time1"];
                time2 = Request.Params["time2"];
                sql += " and issuedTime>= '" + time1 + "' and issuedTime<='" + time2 + "'";
            }

            SqlConnection con = new SqlConnection("server=.;database=MyElectrCheck_DB;Integrated Security=true;");
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            SqlCommand cmd = new SqlCommand(sql, con);

            SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

            List<ps_solvetask_main> list = new List<ps_solvetask_main>();

            while (dr.Read())
            {
                ps_solvetask_main main = new ps_solvetask_main();
                main.id = int.Parse(dr["id"].ToString());
                main.solveTaskCode = dr["solveTaskCode"].ToString();
                main.solveTaskName = dr["solveTaskName"].ToString();
                main.workDocTypeName = dr["workDocTypeName"].ToString();
                main.issuedByName = dr["issuedByName"].ToString();
                main.issuedTime =DateTime.Parse(dr["issuedTime"].ToString());
                main.taskStatus = int.Parse(dr["taskStatus"].ToString());
                main.taskStatusName = dr["taskStatusName"].ToString();
                if (dr["finishTime"].ToString() != "")
                {
                    main.finishTime = Convert.ToDateTime(dr["finishTime"].ToString());
                }
                main.isCancel = int.Parse(dr["isCancel"].ToString());
                list.Add(main);
            }

            //关闭连接 
            dr.Close();
            con.Close();

            DataSource ds = new DataSource();
            ds.taskmainlist = list.Skip(skip).Take(6).ToList();
            ds.datacount = list.Count();

            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }
        
        //添加消缺任务页面
        public ActionResult AddSolveTask()
        {
            var user = (from u in medb.ps_user
                        where u.id == int.Parse(Session["userid"].ToString())
                        select u).FirstOrDefault();

            ViewData["userName"] = user.userName;

            return View();
        }

        //获得消缺人员与缺陷列表信息 
        public ActionResult GetSolvelistData()
        {
            int pageindex = int.Parse(Request.Params["pageindex"]);
            //获得消缺员信息 
            var role = (from i in medb.ps_role
                        where i.roleName == "消缺员"&&i.isStart==1
                        select i).FirstOrDefault();

            var role2 = (from a in medb.ps_role
                         where a.roleName == "线路管理员"&&a.isStart==1
                         select a).FirstOrDefault();


            //获得已选的消缺员 
            var solvetor = (from a in medb.ps_user
                            join b in medb.ps_solver_detail
                            on a.userCode equals b.solverCode
                            join c in medb.ps_solvetask_main
                            on b.taskId equals c.id
                            where a.roleId == role.id&&a.userStatus==1&&c.taskStatus!=5
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

           SqlDataReader dr=cmd.ExecuteReader();

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

            //关闭连接 
            dr.Close();
            con.Close();

            //获得工作单据类型信息 
            var worklist = (from a in medb.ps_systemconfig
                            where a.configCode == "DOC_TYPE" &&a.isStart==1
                            select a).ToList();

            //获得任务负责人信息
            var tasklist = (from a in medb.ps_user
                            where a.roleId == role2.id &&a.userStatus==1
                            select a).ToList();

         

            // SqlCommand cmd2 = new SqlCommand(sql, con);
            //dr = cmd2.ExecuteReader();


            // List<ps_inspectiontask_detail> idlist = new List<ps_inspectiontask_detail>();
            // while (dr.Read())
            // {
            //     ps_inspectiontask_detail id = new ps_inspectiontask_detail()
            //     {
            //         id = int.Parse(dr["id"].ToString()),
            //         lineCode = dr["lineCode"].ToString(),
            //         poleCode = dr["poleCode"].ToString(),
            //         bugLevelName = dr["bugLevelName"].ToString(),
            //         bugTypeName = dr["bugTypeName"].ToString(),
            //         bugDesc = dr["bugDesc"].ToString(),
            //         discovererName = dr["discovererName"].ToString(),
            //         discoverTime = Convert.ToDateTime(dr["discoverTime"].ToString())
            //     };
            //     idlist.Add(id);
            // }

            //关闭连接 
            // dr.Close();


            DataSource ds = new DataSource();

            ds.userlist1 = userlist;
            ds.userlist2 = solvetor;
            ds.userlist3 = tasklist;
            ds.syslist = worklist;
           // int datacount = idlist.Count();
            //int pagecount = (int)Math.Ceiling(datacount / 4.0);
            //if (pageindex <= pagecount && pageindex > 0)
            //{
            //    int skip = (pageindex - 1) * 4;
            //    ds.taskdetlist = idlist.Skip(skip).Take(4).ToList();
            //    ds.datacount = pagecount;
            //}
            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }

        //查询缺陷列表数据 
        public ActionResult GetSolvetors()
        {
            int pageindex = int.Parse(Request.Params["pageindex"]);
            string strs ="";
            //获得session中保存的值 
            string sql = "select *from ps_inspectiontask_detail where issolve=0 and isbug=1 and taskstatus=4 and isBugLevel=1 ";

            //获得缺陷列表信息   首先获得没有进行消缺任务的缺陷列表信息 
            var solvelist = (from s in medb.ps_solvetask_detail
                             join b in medb.ps_solvetask_main
                             on s.taskId equals b.id
                             where b.taskStatus != 5&&b.isCancel!=1
                             select s).ToList();
         
            if (solvelist.Count() > 0)
            {
                foreach (var v in solvelist)
                {
                    sql += " and id!=" + v.inspectionTaskDelId;
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
            int pagecount =(int) Math.Ceiling(datacount / 4.0);
            if (pageindex <= pagecount&&pageindex>0)
            {
                int skip = (pageindex - 1) * 4;
                ds.taskdetlist = idlist.Skip(skip).Take(4).ToList();
                ds.datacount = pagecount;
            }      
            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }
       
        public ActionResult TestView2()
        {
            return View();
        }

        //检查taskcode
        public ActionResult CheckTaskCode()
        {
            string taskcode = Request.Params["taskcode"];

            var result = (from a in medb.ps_solvetask_main
                          where a.solveTaskCode == taskcode
                          select a).FirstOrDefault();

            bool b = true;

            if (result != null)
            {
                b = false;
            }
            return Json(b);
        }

        //添加消缺任务 
        public ActionResult AddSolveData()
        {
            string taskcode = Request.Params["taskcode"];
            string taskname = Request.Params["taskname"];
            int worktype =int.Parse( Request.Params["worktype"]);
            int taskmanagercode =int.Parse( Request.Params["taskmanagercode"]);
            DateTime issuedtime = Convert.ToDateTime(Request.Params["issuedtime"]);
            string taskdesc = Request.Params["taskdesc"];
            string remark = Request.Params["remark"];
            string inspectlist = Request.Params["inspectlist"];
            string solvetorsid = "";

            //查询工作单据信息 
            var workdoc = (from a in medb.ps_systemconfig
                           where a.configCode == "DOC_TYPE" && a.configValueId == worktype
                           select a).FirstOrDefault();
            //查询任务负责人信息 
            var taskmanager = (from a in medb.ps_user
                               where a.id == taskmanagercode && a.userStatus == 1
                               select a).FirstOrDefault();
            var issued = (from a in medb.ps_user
                          where a.id == int.Parse(Session["userid"].ToString())
                          select a).FirstOrDefault();

            bool b = true;

            if (Request.Params["solvetorsid"] != "")
            {
                solvetorsid = Request.Params["solvetorsid"];

                //首先向消缺主表中插入数据 
                ps_solvetask_main main = new ps_solvetask_main()
                {
                    solveTaskCode = taskcode,
                    solveTaskName = taskname,
                    workDocType = workdoc.configValueId,
                    workDocTypeName = workdoc.configValueName,
                    taskDesc = taskdesc,
                    taskManagerCode = taskmanager.userCode,
                    taskManagerName = taskmanager.userName,
                    taskStatus = 2,
                    taskStatusName = "已分配",
                    finishTime = null,
                    common = remark,
                    managerSuggestion = null,
                    taskFinishDesc = null,
                    taskFinishReport = null,
                    taskNotes = null,
                    issuedByCode = issued.userCode,
                    issuedByName = issued.userName,
                    issuedTime = issuedtime,
                    issuedSuggestion = null,
                    createdBy = issued.userName,
                    creationTime = DateTime.Now,
                    lastUpdateTime = DateTime.Now,
                    isCancel = 0,
                    isrejectexamine=0
                };
                //添加数据 
                medb.ps_solvetask_main.InsertOnSubmit(main);

                try
                {
                    medb.SubmitChanges();
                }
                catch (Exception)
                {
                    b = false;
                }

                if (b)
                {
                    //首先查询出消缺主表中最大的一条编号  即刚刚插入的数据编号 
                    var tasks = (from a in medb.ps_solvetask_main
                                 orderby a.id descending
                                 select a).FirstOrDefault();

                    List<ps_solvetask_detail> tasklist = new List<ps_solvetask_detail>();
                    if (inspectlist.IndexOf(',') >= 0)
                    {
                        //再向消缺子表中插入数据 
                        string[] solvetoridlist = inspectlist.Split(',');
                        for (var i = 0; i < solvetoridlist.Length; i++)
                        {
                            ps_solvetask_detail taskdetail = new ps_solvetask_detail()
                            {
                                taskId = tasks.id,
                                inspectionTaskDelId = int.Parse(solvetoridlist[i]),
                                createdBy = issued.userName,
                                creationTime = DateTime.Now,
                                lastUpdateTime = DateTime.Now
                            };
                            tasklist.Add(taskdetail);

                        }
                    }
                    else
                    {
                        ps_solvetask_detail taskdetail = new ps_solvetask_detail()
                        {
                            taskId = tasks.id,
                            inspectionTaskDelId = int.Parse(inspectlist),
                            createdBy = issued.userName,
                            creationTime = DateTime.Now,
                            lastUpdateTime = DateTime.Now
                        };
                        tasklist.Add(taskdetail);
                    }

                    //添加数据 
                    medb.ps_solvetask_detail.InsertAllOnSubmit(tasklist);
                    try
                    {
                        medb.SubmitChanges();
                    }
                    catch (Exception)
                    {
                        b = false;
                    }

                    if (b)
                    {
                        List<ps_solver_detail> solvers = new List<ps_solver_detail>();
                        //最后向消缺员信息表中插入消缺员信息 
                        if (solvetorsid.IndexOf(',') >= 0)
                        {
                            //则进行分割 
                            string[] solvelist = solvetorsid.Split(',');

                            for (var i = 0; i < solvelist.Length; i++)
                            {
                                //根据消缺员id查询出对应的消缺员 
                                var users = (from u in medb.ps_user
                                             where u.id == int.Parse(solvelist[i])
                                             select u).FirstOrDefault();
                                ps_solver_detail solver = new ps_solver_detail()
                                {
                                    taskId = tasks.id,
                                    solverCode = users.userCode,
                                    solverName = users.userName,
                                    createdBy = issued.userCode,
                                    lastUpdateTime = DateTime.Now
                                };
                                solvers.Add(solver);

                            }

                        }
                        else
                        {
                            //根据消缺员id查询出对应的消缺员 
                            var users = (from u in medb.ps_user
                                         where u.id == int.Parse(solvetorsid)
                                         select u).FirstOrDefault();
                            ps_solver_detail solver = new ps_solver_detail()
                            {
                                taskId = tasks.id,
                                solverCode = users.userCode,
                                solverName = users.userName,
                                createdBy = issued.userCode,
                                lastUpdateTime = DateTime.Now
                            };

                            solvers.Add(solver);

                        }

                        medb.ps_solver_detail.InsertAllOnSubmit(solvers);
                        try
                        {
                            medb.SubmitChanges();
                        }
                        catch (Exception)
                        {

                            b = false;
                        }

                    }
                }
            }
            else
            {
                //首先向消缺主表中插入数据 
                ps_solvetask_main main = new ps_solvetask_main()
                {
                    solveTaskCode = taskcode,
                    solveTaskName = taskname,
                    workDocType = workdoc.configValueId,
                    workDocTypeName = workdoc.configValueName,
                    taskDesc = taskdesc,
                    taskManagerCode = taskmanager.userCode,
                    taskManagerName = taskmanager.userName,
                    taskStatus = 1,
                    taskStatusName = "未分配",
                    finishTime = null,
                    common = remark,
                    managerSuggestion = null,
                    taskFinishDesc = null,
                    taskFinishReport = null,
                    taskNotes = null,
                    issuedByCode = issued.userCode,
                    issuedByName = issued.userName,
                    issuedTime = issuedtime,
                    issuedSuggestion = null,
                    createdBy = issued.userName,
                    creationTime = DateTime.Now,
                    lastUpdateTime = DateTime.Now,
                    isCancel = 0,
                    isrejectexamine=0
                };
                //添加数据 
                medb.ps_solvetask_main.InsertOnSubmit(main);

                try
                {
                    medb.SubmitChanges();
                }
                catch (Exception)
                {
                    b = false;
                }

                if (b)
                {
                    //首先查询出消缺主表中最大的一条编号  即刚刚插入的数据编号 
                    var tasks = (from a in medb.ps_solvetask_main
                                 orderby a.id descending
                                 select a).FirstOrDefault();

                    List<ps_solvetask_detail> tasklist = new List<ps_solvetask_detail>();
                    if (inspectlist.IndexOf(',') >= 0)
                    {
                        //再向消缺子表中插入数据 
                        string[] solvetoridlist = inspectlist.Split(',');
                        for (var i = 0; i < solvetoridlist.Length; i++)
                        {
                            ps_solvetask_detail taskdetail = new ps_solvetask_detail()
                            {
                                taskId = tasks.id,
                                inspectionTaskDelId = int.Parse(solvetoridlist[i]),
                                createdBy = issued.userName,
                                creationTime = DateTime.Now,
                                lastUpdateTime = DateTime.Now
                            };
                            tasklist.Add(taskdetail);

                        }
                    }
                    else
                    {
                        ps_solvetask_detail taskdetail = new ps_solvetask_detail()
                        {
                            taskId = tasks.id,
                            inspectionTaskDelId = int.Parse(inspectlist),
                            createdBy = issued.userName,
                            creationTime = DateTime.Now,
                            lastUpdateTime = DateTime.Now
                        };
                        tasklist.Add(taskdetail);
                    }

                    //添加数据 
                    medb.ps_solvetask_detail.InsertAllOnSubmit(tasklist);
                    try
                    {
                        medb.SubmitChanges();
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