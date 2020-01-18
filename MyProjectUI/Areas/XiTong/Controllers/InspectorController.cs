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
    public class InspectorController : Controller
    {
        MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();

        // GET: XiTong/Inspector
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetDataByName()
        {
            int pageindex = int.Parse(Request.Params["pageindex"]);

            int skip = (pageindex - 1) * 6;

            var user = (from u in medc.ps_user
                        where u.id == int.Parse(Session["userid"].ToString())
                        select u).FirstOrDefault();

            var role = (from r in medc.ps_role
                        where r.id == user.roleId
                        select r).FirstOrDefault();

            List<ps_inspectiontask_main> list = null;
            if (role.roleName == "系统管理员")
            {
               list = (from i in medc.ps_inspectiontask_main
                            select i).ToList();
            }
            else
            {
               list = (from i in medc.ps_inspectiontask_main
                            join b in medc.ps_inspector_detail
                            on i.id equals b.taskId
                            where i.taskStatus !=1 && b.inspectorCode == user.userCode&&i.taskStatus!=4&&i.isCancel==0
                            select i).ToList();
            }
           

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
        public ActionResult GetData()
        {
            //首先查询巡检员子表中状态不等于完成的所有巡检员的信息 
            var inspectors = (from a in medc.ps_inspectiontask_main
                              join b in medc.ps_inspector_detail
                              on a.id equals b.taskId
                              where a.taskStatus != 4
                              select b).ToList();

            //再将已查到的巡检员与ps_user表的角色名称为巡检员的用户相比较 
            var userlist = (from u in medc.ps_user
                            join r in medc.ps_role
                            on u.roleId equals r.id
                            where r.roleName == "巡检员" && u.userStatus == 1 && r.isStart == 1
                            select u).ToList();

            List<ps_user> list2 = new List<ps_user>();
            List<ps_user> list3 = new List<ps_user>();
            //最后筛选出user表中没有的巡检员信息 
            foreach (var u in userlist)
            {
                foreach (var v in inspectors)
                {
                    if (u.userCode == v.inspectorCode)
                    {
                        list2.Add(u);
                    }
                   
                   
                }
            }

           
            string sql = " select *from ps_user where 1=1 and roleid=(select id from ps_role where rolename='巡检员' and isstart=1) and userstatus=1";

            for(var i = 0; i <list2.Count(); i++)
            {
                sql += " and usercode!='" + list2[i].userCode + "'";
            }
            SqlConnection con = new SqlConnection("server=.;database=MyElectrCheck_DB;Integrated Security=true;");
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
            SqlCommand cmd = new SqlCommand(sql,con);

            SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

            while (dr.Read())
            {
                ps_user u = new ps_user()
                {
                    id = int.Parse(dr["id"].ToString()),
                    userCode = dr["userCode"].ToString(),
                    userName = dr["userName"].ToString()
                };
                list3.Add(u);
            }
            dr.Close();
            con.Close();

          
            DataSource ds = new DataSource();

            if (list3.Count() > 0)
            {
                ds.userlist2 = list3;
            }
           
            return Json(ds);
        }

        public ActionResult AddInspector()
        {
            int taskid = int.Parse(Request.Params["taskid"]);
            string strs = Request.Params["inspectors"];

            bool b = true;

            if (strs.IndexOf(',') > 0)
            {
                List<ps_inspector_detail> des = new List<ps_inspector_detail>();
                var user = (from u in medc.ps_user
                            where u.id == int.Parse(Session["userid"].ToString())
                            select u).FirstOrDefault();

                string[] strlist = strs.Split(',');
                for(var i = 0; i < strlist.Length; i++)
                {
                    var users = (from u in medc.ps_user
                                 where u.id == int.Parse(strlist[i])
                                 select u).FirstOrDefault();

                    ps_inspector_detail dd = new ps_inspector_detail()
                    {
                        taskId = taskid,
                        inspectorCode = users.userCode,
                        inspectorName = users.userName,
                        creationTime = DateTime.Now,
                        createdBy = user.userCode,
                        lastUpdateTime = DateTime.Now

                    };
                    des.Add(dd);

                }
                medc.ps_inspector_detail.InsertAllOnSubmit(des);

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
                    var ins = (from a in medc.ps_inspectiontask_main
                               where a.id == taskid
                               select a).FirstOrDefault();

                    ins.taskStatus = 2;
                    ins.taskStatusName = "已分配";

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
               
                var user = (from u in medc.ps_user
                            where u.id == int.Parse(Session["userid"].ToString())
                            select u).FirstOrDefault();

              
                    var users = (from u in medc.ps_user
                                 where u.id == int.Parse(strs)
                                 select u).FirstOrDefault();

                    ps_inspector_detail dd = new ps_inspector_detail()
                    {
                        taskId = taskid,
                        inspectorCode = users.userCode,
                        inspectorName = users.userName,
                        creationTime = DateTime.Now,
                        createdBy = user.userCode,
                        lastUpdateTime = DateTime.Now

                    };
                  
                    medc.ps_inspector_detail.InsertOnSubmit(dd);

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
                    var ins = (from a in medc.ps_inspectiontask_main
                               where a.id == taskid
                               select a).FirstOrDefault();

                    ins.taskStatus = 2;
                    ins.taskStatusName = "已分配";

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

         public  ActionResult ExcuteOrReceipt()
        {
            return View();
        }
        //修改任务状态 
        public ActionResult UpdTaskStatus()
        {
            int id = int.Parse(Request.Params["taskid"]);
            var taskmain = (from i in medc.ps_inspectiontask_main
                            where i.id == id
                            select i).FirstOrDefault();

            //修改状态 
            taskmain.taskStatus = 3;
            taskmain.taskStatusName = "执行中";
            taskmain.lastUpdateTime = DateTime.Now;
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

        public ActionResult ReceiptEnter()
        {
            var user = (from i in medc.ps_user
                        where i.id == int.Parse(Session["userid"].ToString())
                        select i).FirstOrDefault();

            ViewData["usercode"] = user.userCode;
            return View();
        }
        public ActionResult GetPoleIdById()
        {
            int id = int.Parse(Session["taskid"].ToString());

            var result = (from im in medc.ps_inspectiontask_main
                          join d in medc.ps_inspector_detail
                          on im.id equals d.taskId
                          where im.id == id
                          select new {taskid=im.id, taskcode = im.inspectionTaskCode, taskname = im.inspectionTaskName,linecode=im.lineCode, linename = im.lineName, startpole = im.startPoleCode, endpole = im.endPoleCode, createuser = im.issuedByName, createtime = im.issuedTime, taskstatus = im.taskStatusName, finishtime = im.finishTime, common = im.common, inspetorname = d.inspectorName, linid = im.lineId }).FirstOrDefault();

            InspectorDetail detas = new InspectorDetail()
            {
                 taskid=result.taskid,
                taskcode = result.taskcode,
                taskname = result.taskname,
                linecode=result.linecode,
                linename = result.linename,
                startpole = result.startpole,
                endpole = result.endpole,
                createtime = result.createtime,
                createuser = result.createuser,
                taskstatus = result.taskstatus,
                finishtime = result.finishtime,
                common = result.common,
                inspetorname = result.inspetorname,
                linid = result.linid
            };
          
            //查询所有缺陷类型信息 
            var bugtype = (from b in medc.ps_systemconfig
                           where b.configName=="缺陷类型"&&b.isStart==1
                           select b).ToList();
            //查询所有的缺陷级别信息 
            var buglevel = (from b in medc.ps_systemconfig
                            where b.configName == "缺陷级别"&&b.isStart==1
                            select b).ToList();

            var idetails = (from r in medc.ps_inspectiontask_detail
                            where r.taskid==id
                            select r).ToList();

            DataSource ds = new DataSource();

            ds.inspectordetails = detas;

            ds.bugtypelist = bugtype;

            ds.buglevellist = buglevel;

            ds.taskdetails = idetails;

            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }

        public ActionResult GetPoleDataById()
        {
            int id = int.Parse(Request.Params["poleid"]);

            //查找第一个杆塔编号的详细信息 
            var taskdet = (from a in medc.ps_inspector_detail
                           join b in medc.ps_inspectiontask_main
                           on a.taskId equals b.id
                           join c in medc.ps_inspectiontask_detail
                           on b.id equals c.taskid
                           where c.poleid == id
                           select new { linecode = c.lineCode, polecode = c.poleCode,bugtype=c.bugType,buglevel=c.bugLevel, isbug = c.isBug, intactRate = c.intactRate, bugTypeName = c.bugTypeName, bugLevelName = c.bugLevelName, bugDesc = c.bugDesc, inspectTime = c.inspectTime, inspectorName = c.discovererName, discovererName = c.discovererName, discoverTime = c.discoverTime, issuedByName = b.issuedByName, issuedTime = b.issuedTime }).FirstOrDefault();

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
                    bugtype=taskdet.bugtype,
                    buglevel=taskdet.buglevel,
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

        public ActionResult AddInspectInfo()
        {
            int taskid = int.Parse(Request.Params["taskid"]);
            int poleid = int.Parse(Request.Params["poleid"]);
            int intactrate = int.Parse(Request.Params["intactrate"]);
            int isbug = int.Parse(Request.Params["isbug"]);
            int isbuglevel = int.Parse(Request.Params["isbuglevel"]);
            DateTime discovertime = DateTime.Parse(Request.Params["discovertime"]);

            string buglevelname = Request.Params["buglevelname"];
            string bugtypename = Request.Params["bugtypename"];
            int buglevel = int.Parse(Request.Params["bugtype"]);
            int bugtype = int.Parse(Request.Params["bugtype"]);
            string desc = Request.Params["bugdesc"];
                 
            var user = (from u in medc.ps_user
                    where u.id == int.Parse(Session["userid"].ToString())
                    select u).FirstOrDefault();

            var taskdetails = (from t in medc.ps_inspectiontask_detail
                               where t.poleid == poleid &&t.taskid==taskid
                               select t).FirstOrDefault();
            //修改数据 
            taskdetails.isBug = isbug;
            taskdetails.intactRate = intactrate;
            taskdetails.bugLevel = buglevel;
            taskdetails.bugLevelName = buglevelname;
            taskdetails.bugType = bugtype;
            taskdetails.bugTypeName = bugtypename;
            taskdetails.bugDesc = desc;
            taskdetails.isBugLevel = isbuglevel;
            taskdetails.taskStatus = 4;
            taskdetails.discovererCode = user.userCode;
            taskdetails.discovererName = user.userName;
            taskdetails.discoverTime = discovertime;
            taskdetails.inspectTime = DateTime.Now;

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

        public ActionResult UpdInspectStatus()
        {
            int taskid = int.Parse(Request.Params["taskid"]);

            //查询出该编号对应的任务信息 
            var details = (from a in medc.ps_inspectiontask_main
                           where a.id == taskid
                           select a).FirstOrDefault();

            details.taskStatus = 4;
            details.taskStatusName = "已完成";
            details.lastUpdateTime = DateTime.Now;
            details.finishTime = DateTime.Now;

            List<ps_inspectiontask_detail> list = new List<ps_inspectiontask_detail>();

            var taskdetails = (from t in medc.ps_inspectiontask_detail
                               where t.taskid == taskid
                               select t).ToList();

            bool b = true;

            foreach (var v in taskdetails)
            {
                if (v.inspectTime == null)
                {
                    var dets = (from i in medc.ps_inspectiontask_detail
                                where i.id == v.id
                                select i).FirstOrDefault();

                    //修改数据 
                    dets.inspectTime = DateTime.Now;
                    dets.lastUpdateTime = DateTime.Now;
                    dets.taskStatus = 4;
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

        public ActionResult UpdInspectData()
        {
            return View();
        }

        public ActionResult GetUpdData()
        {
            int id = int.Parse(Request.Params["poleid"]);

            var detas = (from i in medc.ps_inspectiontask_detail
                         where i.id == id
                         select i).FirstOrDefault();

            DataSource ds = new DataSource();
            ds.taskdet = detas;

            string jsonstr = JsonConvert.SerializeObject(ds);

            return Json(ds);
        }
       
       

    }
}