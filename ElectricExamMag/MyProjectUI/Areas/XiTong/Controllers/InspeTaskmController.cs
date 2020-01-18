using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyProjectUI.Models;
using MyProjectUI.Areas.XiTong.Models;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
namespace MyProjectUI.Areas.XiTong.Controllers
{
    public class InspeTaskmController : Controller
    {
        MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();
        // GET: XiTong/InspeTaskm
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetData()
        {
            int pageindex = int.Parse(Request.Params["pageindex"]);

            int skip = (pageindex - 1) * 6;

            var list = (from i in medc.ps_inspectiontask_main
                        where i.isCancel==0
                        select i).ToList();

            var configlist = (from c in medc.ps_systemconfig
                              where c.configCode == "TASK_STATUS"
                              select c).ToList();

            DataSource ds = new DataSource();

            ds.imlist = list.Skip(skip).Take(6).ToList();

            ds.syslist = configlist;

            ds.pagecount = list.Count;
            

            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }
        public ActionResult SelectData()
        {
            string taskcode = "";
            string linecode = "";
            string taskstatus = "";
            string createuser = "";
            string time1 = "";
            string time2 = "";
            int pageindex = int.Parse(Request.Params["pageindex"]);

            int skip = (pageindex - 1) * 6;

            string sql = "select *from [ps_inspectiontask_main] where isCancel=0 and 1=1 ";
            if (Request.Params["taskcode"] != "")
            {
                taskcode = Request.Params["taskcode"];
                sql += " and inspectionTaskCode like '%" + taskcode + "%' ";
            }
            if (Request.Params["linecode"] != "")
            {
                linecode = Request.Params["linecode"];
                sql += " and lineCode like '%" + linecode + "%' ";
            }

            if (Request.Params["taskstatus"] != "")
            {
                taskstatus = Request.Params["taskstatus"];
                sql += " and taskStatusName='" + taskstatus + "' ";
            }
            if (Request.Params["createby"] != "")
            {
                createuser = Request.Params["createby"];
                sql+= "and issuedByName like '%"+createuser+"%'";
            }

            if (Request.Params["time1"] != null)
            {
                time1 = Request.Params["time1"];
                time2 = Request.Params["time2"];
                sql += " and issuedTime>= '" + time1 + "' and issuedTime<='"+time2+"'";
            }

            SqlConnection con = new SqlConnection("server=.;database=MyElectrCheck_DB;Integrated Security=true;");
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            SqlCommand cmd = new SqlCommand(sql, con);

            SqlDataReader dr=cmd.ExecuteReader(CommandBehavior.CloseConnection);

            List<ps_inspectiontask_main> list = new List<ps_inspectiontask_main>();

            while (dr.Read())
            {
                ps_inspectiontask_main main = new ps_inspectiontask_main();
                main.id = int.Parse(dr["id"].ToString());
                main.inspectionTaskCode = dr["inspectionTaskCode"].ToString();
                main.inspectionTaskName = dr["inspectionTaskName"].ToString();
                main.lineCode = dr["lineCode"].ToString();
                main.startPoleCode = dr["startPoleCode"].ToString();
                main.endPoleCode = dr["endPoleCode"].ToString();
                main.taskStatus = int.Parse(dr["taskStatus"].ToString());
                main.taskStatusName = dr["taskStatusName"].ToString();
                if (dr["finishTime"].ToString() != "")
                {
                    main.finishTime = Convert.ToDateTime(dr["finishTime"].ToString());
                }
                main.isCancel = int.Parse(dr["isCancel"].ToString());
                list.Add(main);
            }

            dr.Close();
            con.Close();

            DataSource ds = new DataSource();
            ds.imlist = list.Skip(skip).Take(6).ToList();
            ds.pagecount = list.Count();

            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }

        //查看巡检任务的action
        public ActionResult InspectDetails()
        {
            return View();
        }

        //保存task的action
        public ActionResult SaveIdBySession()
        {
            Session["taskid"] = Request.Params["taskid"];
            return Json(" ");
        }

        public ActionResult GetDetailData()
        {
            int id = int.Parse(Session["taskid"].ToString());

          

            var result = (from im in medc.ps_inspectiontask_main
                          where im.id == id
                          select new { taskcode = im.inspectionTaskCode, taskname = im.inspectionTaskName, linename = im.lineName, startpole = im.startPoleCode, endpole = im.endPoleCode, createuser = im.issuedByName, createtime = im.issuedTime, taskstatus = im.taskStatusName, finishtime = im.finishTime, common = im.common,linid=im.lineId }).FirstOrDefault();

            //获得巡检员信息列表
            var inspectors = (from i in medc.ps_inspector_detail
                              where i.taskId == id
                              select i).ToList();
          
                InspectorDetail detas = new InspectorDetail()
                {
                    taskcode = result.taskcode,
                    taskname = result.taskname,
                    linename = result.linename,
                    startpole = result.startpole,
                    endpole = result.endpole,
                    createtime = result.createtime,
                    createuser = result.createuser,
                    taskstatus = result.taskstatus,
                    finishtime = result.finishtime,
                    common = result.common,
                    linid = result.linid
                };
          
            //查询该线路下的所有杆塔信息 
            var polelist = (from p in medc.ps_pole_info
                            where p.lineId == result.linid
                            select p).ToList();

            TaskBugDet bugdet = null;

            if (polelist.Count() > 0)
            {
                //查找第一个杆塔编号的详细信息 
                var taskdet = (from a in medc.ps_inspectiontask_detail 
                               join b in medc.ps_inspectiontask_main
                               on a.taskid equals b.id                           
                               where a.poleid == polelist.FirstOrDefault().id&&a.taskid==id
                               select new { linecode = a.lineCode, polecode = a.poleCode, isbug = a.isBug, intactRate = a.intactRate, bugTypeName = a.bugTypeName, bugLevelName = a.bugLevelName, bugDesc = a.bugDesc, inspectTime = a.inspectTime, discovererName = a.discovererName, discoverTime = a.discoverTime, issuedByName = b.issuedByName, issuedTime = b.issuedTime }).FirstOrDefault();

                if (taskdet != null)
                {
                    bugdet = new TaskBugDet()
                    {
                        linecode = taskdet.linecode,
                        polecode = taskdet.polecode,
                        isbug = taskdet.isbug,
                        intactRate = taskdet.intactRate,
                        bugLevelName = taskdet.bugLevelName,
                        bugTypeName = taskdet.bugTypeName,
                        bugDesc = taskdet.bugDesc,
                        inspectTime = taskdet.inspectTime,
                        discovererName = taskdet.discovererName,
                        discoverTime = taskdet.discoverTime,
                        issuedByName = taskdet.issuedByName,
                        issuedTime = taskdet.issuedTime
                    };
                }
            }

            DataSource ds = new DataSource();

            ds.inspectordetails =detas;

            ds.polelist = polelist;

            if (inspectors.Count() > 0)
            {
                ds.insdet = inspectors;
            }
            else
            {
                ds.insdet = null;
            }

            if (bugdet != null)
            {
                ds.bugdet = bugdet;
            }
            else
            {
                ds.bugdet = null;
            }

            string jsonstr = JsonConvert.SerializeObject(ds);
            
            return Content(jsonstr);
        }

        public ActionResult GetPoleData()
        {
            int id = int.Parse(Request.Params["poleid"]);
            int taskid = int.Parse(Session["taskid"].ToString());

            //查找第一个杆塔编号的详细信息 
            var taskdet = (from b in medc.ps_inspectiontask_main
                           join c in medc.ps_inspectiontask_detail
                           on b.id equals c.taskid
                           where c.poleid ==id&&c.taskid==taskid
                           select new { linecode = c.lineCode, polecode = c.poleCode, isbug = c.isBug, intactRate = c.intactRate, bugTypeName = c.bugTypeName, bugLevelName = c.bugLevelName, bugDesc = c.bugDesc, inspectTime = c.inspectTime, inspectorName = c.discovererName, discovererName = c.discovererName, discoverTime = c.discoverTime, issuedByName = b.issuedByName, issuedTime = b.issuedTime }).FirstOrDefault();

            TaskBugDet bugdet = null;

            DataSource ds = new DataSource();

            if (taskdet != null)
            {
                bugdet = new TaskBugDet()
                {
                    linecode = taskdet.linecode,
                    polecode = taskdet.polecode,
                    isbug = taskdet.isbug,
                    intactRate = taskdet.intactRate,
                    bugLevelName = taskdet.bugLevelName,
                    bugTypeName = taskdet.bugTypeName,
                    bugDesc = taskdet.bugDesc,
                    inspectorName = taskdet.inspectorName,
                    inspectTime = taskdet.inspectTime,
                    discovererName = taskdet.discovererName,
                    discoverTime = taskdet.discoverTime,
                    issuedByName = taskdet.issuedByName,
                    issuedTime = taskdet.issuedTime
                };

                ds.bugdet = bugdet;
            }
            else {
                ds.bugdet = null;
            }
            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }

        public ActionResult AddInspectTask()
        {
            int userid = int.Parse(Session["userid"].ToString());
            var user = (from u in medc.ps_user
                        where u.id == userid
                        select u).FirstOrDefault();

            ViewData["usercode"] = user.userCode;
            return View();
        }

        public ActionResult GetlineCodes()
        {
            //首先查询巡检员子表中状态不等于完成的所有巡检员的信息 
            var inspectors = (from a in medc.ps_inspector_detail
                              join b in medc.ps_inspectiontask_main
                              on a.taskId equals b.id
                              where b.taskStatus!=4
                              select a).ToList();

            List<ps_user> list = new List<ps_user>();

            string sql = " select *from ps_user where 1=1 and roleid=(select id from ps_role where rolename='巡检员' and isstart=1) and userstatus=1";

            for (var i = 0; i < inspectors.Count(); i++)
            {
                sql += " and usercode!='" + inspectors[i].inspectorCode + "'";
            }
        
            SqlConnection con = new SqlConnection("server=.;database=MyElectrCheck_DB;Integrated Security=true;");
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
            SqlCommand cmd = new SqlCommand(sql, con);

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                ps_user u = new ps_user()
                {
                    id = int.Parse(dr["id"].ToString()),
                    userCode = dr["userCode"].ToString(),
                    userName = dr["userName"].ToString()
                };
                list.Add(u);
            }

            dr.Close();
       
            //首先查询出巡检任务为完成的巡检任务信息 
            var ins = (from i in medc.ps_inspectiontask_main
                       where i.taskStatus != 4
                       select i).ToList();

            sql = "select *from ps_line_info where 1=1 and isStart=1 ";
            if (ins.Count() > 0)
            {
                foreach(var v in ins)
                {
                    sql += " and id!=" + v.lineId+" ";
                }
            }

            List<ps_line_info> linelist = new List<ps_line_info>();

            SqlCommand cmd2 = new SqlCommand(sql, con);

            dr = cmd2.ExecuteReader();

            while (dr.Read())
            {
                ps_line_info lines = new ps_line_info()
                {
                    id = int.Parse(dr["id"].ToString()),
                    lineCode=dr["lineCode"].ToString(),
                    lineName = dr["lineName"].ToString(),
                    startPole = int.Parse(dr["startPole"].ToString()),
                    endPole = int.Parse(dr["endPole"].ToString()),
                    startPoleCode = dr["startPoleCode"].ToString(),
                    endPoleCode = dr["endPoleCode"].ToString()
                };
                linelist.Add(lines);
            }

            dr.Close();
            con.Close();

            DataSource ds = new DataSource();
            ds.pslist = list;
            if (linelist.Count() > 0)
            {
                ds.lilist = linelist;
            }
            return Json(ds);
        }
        public ActionResult CheckTaskCode()
        {
            string taskcode = Request.Params["taskcode"];
            var inspect = (from i in medc.ps_inspectiontask_main
                           where i.inspectionTaskCode == taskcode
                           select i).FirstOrDefault();

            bool b = true;
            if (inspect != null)
            {
                b = false;
            }
            return Json(b);
        }
        public ActionResult AddTaskinfo()
        {
            string taskcode = Request.Params["taskcode"];
            string taskname = Request.Params["taskname"];
            int lineid = int.Parse(Request.Params["lineid"]);
            string linecode = Request.Params["linecode"];
            string linename = Request.Params["linename"];
            int startpole = int.Parse(Request.Params["startpole"]);
            int endpole = int.Parse(Request.Params["endpole"]);
            string startpolecode = Request.Params["startpolecode"];
            string endpolecode = Request.Params["endpolecode"];
            string desc = Request.Params["desc"];
            DateTime issuedtime = Convert.ToDateTime(Request.Params["issuedtime"]);
            string inspecname = "";
            string inspectorcode = "";
            var user = (from u in medc.ps_user
                        where u.id == int.Parse(Session["userid"].ToString()) && u.userStatus == 1
                        select u).FirstOrDefault();

            bool b = true;

            if (Request.Params["inspecname"] != ""&& Request.Params["inspectorcode"] != "")
            {
                inspectorcode = Request.Params["inspectorcode"];    
                inspecname = Request.Params["inspecname"];          
                ps_inspectiontask_main taskmain = new ps_inspectiontask_main();
                taskmain.inspectionTaskCode = taskcode;
                taskmain.inspectionTaskName = taskname;
                taskmain.lineId = lineid;
                taskmain.lineCode = linecode;
                taskmain.lineName = linename;
                taskmain.startPole = startpole;
                taskmain.startPoleCode = startpolecode;
                taskmain.endPole = endpole;
                taskmain.endPoleCode = endpolecode;
                taskmain.issuedByCode = user.userCode;
                taskmain.issuedByName = user.userName;
                taskmain.issuedTime =issuedtime;
                taskmain.taskStatus = 2;
                taskmain.taskStatusName = "已分配";
                taskmain.finishTime = null;
                taskmain.common = desc;
                taskmain.createdBy = user.userCode;
                taskmain.creationTime = DateTime.Now;
                taskmain.lastUpdateTime = DateTime.Now;
                taskmain.isCancel = 0;

                medc.ps_inspectiontask_main.InsertOnSubmit(taskmain);
                //保存添加 
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
                    var tm = (from m in medc.ps_inspectiontask_main
                              orderby m.id descending
                              select m).FirstOrDefault();

                    List<ps_inspector_detail> ilist = new List<ps_inspector_detail>();
                    if (inspectorcode.IndexOf(',') > 0 )
                    {
                        string[] strslist = inspectorcode.Split(',');

                        for (var i = 0; i < strslist.Length; i++)
                        {
                            var u = (from p in medc.ps_user
                                     where p.userCode == strslist[i]
                                     select p).FirstOrDefault();   
                            ps_inspector_detail det = new ps_inspector_detail()
                            {
                                taskId = tm.id,
                                inspectorCode = strslist[i],
                                inspectorName = u.userName,
                                creationTime = DateTime.Now,
                                createdBy = user.userCode,
                                lastUpdateTime = DateTime.Now
                            };
                            ilist.Add(det);
                        }
                    }
                    else
                    {
                        ps_inspector_detail det = new ps_inspector_detail()
                        {
                            taskId = tm.id,
                            inspectorCode = inspectorcode,
                            inspectorName =inspecname,
                            creationTime = DateTime.Now,
                            createdBy = user.userCode,
                            lastUpdateTime = DateTime.Now
                        };
                        ilist.Add(det);
                    }
                   

                    medc.ps_inspector_detail.InsertAllOnSubmit(ilist);

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
                //实例化对象将数据添加到该实例中 
                ps_inspectiontask_main taskmain = new ps_inspectiontask_main()
                {
                    inspectionTaskCode = taskcode,
                    inspectionTaskName = taskname,
                    lineId = lineid,
                    lineCode = linecode,
                    lineName = linename,
                    startPole = startpole,
                    startPoleCode = startpolecode,
                    endPole = endpole,
                    endPoleCode = endpolecode,
                    issuedByCode = user.userCode,
                    issuedByName = user.userName,
                    issuedTime = issuedtime,
                    taskStatus = 1,
                    taskStatusName = "未分配",
                    finishTime = null,
                    common = desc,
                    creationTime = DateTime.Now,
                    createdBy = user.userCode,
                    lastUpdateTime = DateTime.Now,
                    isCancel = 0
                };

                medc.ps_inspectiontask_main.InsertOnSubmit(taskmain);
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
                //首先查询出刚刚添加的任务的id
                var taskmains = (from i in medc.ps_inspectiontask_main
                                 orderby i.id descending
                                 select i).FirstOrDefault();

                //根据线路编号查询出所有的杆塔信息 
                var poles = (from p in medc.ps_pole_info
                             where p.lineId == lineid
                             select p).ToList();

                List<ps_inspectiontask_detail> taskdetails = new List<ps_inspectiontask_detail>();
                for (var i = 0; i < poles.Count(); i++)
                {
                    ps_inspectiontask_detail dets = new ps_inspectiontask_detail()
                    {
                        taskid = taskmains.id,
                        poleid = poles[i].id,
                        poleCode = poles[i].poleCode,
                        lineCode = poles[i].lineCode,
                        lineName = poles[i].lineName,
                        isBug = 0,
                        intactRate = 100,
                        bugLevel = null,
                        bugLevelName = null,
                        bugType = null,
                        bugTypeName = null,
                        bugDesc = null,
                        discovererCode = user.userCode,
                        discovererName = user.userName,
                        discoverTime = DateTime.Now,
                        isSolve = 0,
                        inspectTime = null,
                        createdBy = user.userCode,
                        creationTime = DateTime.Now,
                        lastUpdateTime = DateTime.Now,
                        taskStatus = 1,
                        isBugLevel = 0
                    };
                    taskdetails.Add(dets);
                }

                //插入数据 
                medc.ps_inspectiontask_detail.InsertAllOnSubmit(taskdetails);

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
       
        public ActionResult UpdTaskPage()
        {
            return View();
        }

     

        public ActionResult GetUpdData()
        {
            //获得session中保存的值 
            int id = int.Parse(Session["taskid"].ToString());

            var result = (from a in medc.ps_inspectiontask_main
                          where a.id == id
                          select a).FirstOrDefault();

            //首先查询巡检员子表中状态不等于完成的所有巡检员的信息 
            var inspectorlist = (from a in medc.ps_inspectiontask_main
                              join b in medc.ps_inspector_detail
                              on a.id equals b.taskId
                              where b.taskId==id
                              select b).ToList();

            var userlist = (from u in medc.ps_user
                            join r in medc.ps_role
                            on u.roleId equals r.id
                            where r.roleName == "巡检员"&&u.userStatus==1&&r.isStart==1
                            select u).ToList();

      

            var linecode = (from l in medc.ps_line_info
                            where l.isStart == 1
                            select l).ToList();

            DataSource ds = new DataSource();
            ds.taskmain = result;
            ds.pslist = userlist;
            ds.lilist = linecode;
            ds.insdet = inspectorlist;

            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }

        public ActionResult UpdTaskData()
        {
            int id = int.Parse(Request.Params["taskid"]);
            string taskcode = Request.Params["taskcode"];
            string taskname = Request.Params["taskname"];
            int lineid = int.Parse(Request.Params["lineid"]);
            string linecode = Request.Params["linecode"];
            string linename = Request.Params["linename"];
            int startpole = int.Parse(Request.Params["startpole"]);
            int endpole = int.Parse(Request.Params["endpole"]);
            string startpolecode = Request.Params["startpolecode"];
            string endpolecode = Request.Params["endpolecode"];
            string desc = Request.Params["desc"];
            string issuedcode = Request.Params["issuedcode"];  //下发人code
            string issuedname = Request.Params["issuedname"];  //下发人name
            DateTime issuedtime = Convert.ToDateTime(Request.Params["issuedtime"]);  //下发时间
            string inspectorcode = Request.Params["inspectorcode"];
            string createdBy = Request.Params["createdBy"];
            DateTime createdtime = Convert.ToDateTime(Request.Params["createdtime"]);

            bool b = true;

            var users = (from u in medc.ps_user
                         where u.id == int.Parse(Session["userid"].ToString())
                         select u).FirstOrDefault();

                var inspectormain = (from i in medc.ps_inspectiontask_main
                                     where i.id == id
                                     select i).FirstOrDefault();

                //修改数据 
                inspectormain.inspectionTaskCode = taskcode;
                inspectormain.inspectionTaskName = taskname;
                inspectormain.lineId = lineid;
                inspectormain.lineCode = linecode;
                inspectormain.lineName = linename;
                inspectormain.startPole = startpole;
                inspectormain.startPoleCode = startpolecode;
                inspectormain.endPole = endpole;
                inspectormain.endPoleCode = endpolecode;
                inspectormain.taskStatus = 2;
                inspectormain.taskStatusName = "已分配";
                inspectormain.common = desc;
                inspectormain.lastUpdateTime = DateTime.Now;

                try
                {
                    //保存修改 
                    medc.SubmitChanges();
                }
                catch (Exception)
                {
                    b = false;
                }
                if (b)  //修改巡检员信息表的数据 
                {
                    if (inspectorcode.IndexOf(',') > 0)
                    {
                        List<ps_inspector_detail> list2 = new List<ps_inspector_detail>();
                        string[] strlist = inspectorcode.Split(',');
                        for (var i = 0; i < strlist.Length; i++)
                        {
                            var user = (from c in medc.ps_user
                                        where c.userCode == strlist[i]
                                        select c).FirstOrDefault();
                            ps_inspector_detail detail = new ps_inspector_detail()
                            {
                                taskId = id,
                                inspectorCode = user.userCode,
                                inspectorName = user.userName,
                                creationTime = DateTime.Now,
                                createdBy = users.userCode,
                                lastUpdateTime = DateTime.Now

                            };
                            list2.Add(detail);

                        }

                        //保存数据
                        medc.ps_inspector_detail.InsertAllOnSubmit(list2);

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

        public ActionResult CancleTask()
        {
            int id = int.Parse(Request.Params["taskid"]);
            var inspectortask = (from i in medc.ps_inspectiontask_main
                                 where i.id == id
                                 select i).FirstOrDefault();


            bool b = true;

            if (inspectortask.taskStatus == 1)
            {
                inspectortask.isCancel = 1;
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
                b = false;
            }

            return Json(b);
        }
    }
}