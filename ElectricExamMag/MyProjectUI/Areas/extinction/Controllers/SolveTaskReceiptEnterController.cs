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
    public class SolveTaskReceiptEnterController : Controller
    {
        MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();
        // GET: extinction/SolveTaskReceiptEnter
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetData()
        {
            int pageindex = int.Parse(Request.Params["pageindex"]);

            int skip = (pageindex - 1) * 6;

            var user = (from u in medc.ps_user
                        where u.id == int.Parse(Session["userid"].ToString())
                        select u).FirstOrDefault();

            var role = (from r in medc.ps_role
                        where r.id == user.roleId
                        select r).FirstOrDefault();

            List<ps_solvetask_main> solvedata = null;
            if (role.roleName == "系统管理员"||role.roleName=="线路管理员")
            {
                //获得消缺主表中的数据 
                solvedata = (from a in medc.ps_solvetask_main
                                 where a.taskStatus != 1 && a.taskStatus != 4 && a.taskStatus != 5
                                 select a).ToList();
            }
            else
            {
                //获得消缺主表中的数据 
                solvedata = (from a in medc.ps_solvetask_main
                             join b in medc.ps_solver_detail
                             on a.id equals b.taskId
                             where a.taskStatus != 1 && a.taskStatus != 4 && a.taskStatus != 5&&b.solverCode==user.userCode
                             select a).ToList();
            }

           

            //获得工作单据 
            var workdoc = (from a in medc.ps_systemconfig
                           where a.configCode == "DOC_TYPE" 
                           select a).ToList();

            DataSource ds = new DataSource();

            if (solvedata != null)
            {
                ds.taskmainlist = solvedata;
            }
                     
            ds.syslist = workdoc;

            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }

        public ActionResult SelectData()
        {
            int pageindex = int.Parse(Request.Params["pageindex"]);
            string taskcode = "";
            string worktypename = "";
            string createby ="";
            string time1="";
            string time2="";

            int skip = (pageindex - 1) * 6;

            string sql = "select *from ps_solvetask_main where 1=1 and taskstatus!=1 ";
            if (Request.Params["taskcode"] != "")
            {
                taskcode = Request.Params["taskcode"];
                sql += " and solveTaskCode like '%" + taskcode + "%' ";
            }
            if (Request.Params["worktypename"] != "")
            {
                worktypename = Request.Params["worktypename"];
                sql += " and workDocTypeName='" + worktypename + "' ";
            }

            if (Request.Params["createby"] != "")
            {
                createby = Request.Params["createby"];
                sql += "and issuedByName like '%" + createby + "%'";
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
                main.issuedTime = DateTime.Parse(dr["issuedTime"].ToString());
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
            ds.taskmainlist = list.Skip(skip).Take(6).ToList();
            ds.datacount = list.Count();

            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }

        //修改任务状态 
        public ActionResult TransTaskStatus()
        {
            int id = int.Parse(Request.Params["taskid"]);

            var solve = (from i in medc.ps_solvetask_main
                         where i.id == id
                         select i).FirstOrDefault();

            //修改数据 
            solve.taskStatus = 3;
            solve.taskStatusName = "执行中";
            solve.lastUpdateTime = DateTime.Now;

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

        //回执录入页面 
        public ActionResult ReceiptEnyerView()
        {
            return View();
        }

        //回执录入的action
        public ActionResult ReceiptEnterTask()
        {
            int id = int.Parse(Request.Params["taskid"]);
            string taskfinishdesc = Request.Params["taskfinishdesc"];
            string tasknotes = Request.Params["tasknotes"];
            string taskfinishreport = Request.Params["taskfinishreport"];

            var solver = (from s in medc.ps_solvetask_main
                          where s.id == id
                          select s).FirstOrDefault();

            solver.taskFinishDesc = taskfinishdesc;
            solver.taskNotes = tasknotes;
            solver.taskFinishReport = taskfinishreport;
            solver.lastUpdateTime = DateTime.Now;
            solver.isrejectexamine = 1;
            //清空负责人与下发人的意见信息 
            solver.issuedSuggestion = null;
            solver.managerSuggestion = null;

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

        //上传回执 
        public ActionResult UploadReceiptEnterTask()
        {
            int id = int.Parse(Request.Params["taskid"]);
            string taskfinishdesc = Request.Params["taskfinishdesc"];
            string tasknotes = Request.Params["tasknotes"];
            string taskfinishreport = Request.Params["taskfinishreport"];

            var solver = (from s in medc.ps_solvetask_main
                          where s.id == id
                          select s).FirstOrDefault();

            solver.taskFinishDesc = taskfinishdesc;
            solver.taskNotes = tasknotes;
            solver.taskFinishReport = taskfinishreport;
            solver.lastUpdateTime = DateTime.Now;
            solver.taskStatus = 4;
            solver.taskStatusName = "已完成";
            solver.finishTime = DateTime.Now;

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
        //回执修改的action
        public ActionResult UpdReceipt()
        {
            return View();
        }

        public ActionResult ShowReceiptData()
        {
            return View();
        }
        public ActionResult RejectUpdatePage()
        {
            return View();
        }
    }
   

}