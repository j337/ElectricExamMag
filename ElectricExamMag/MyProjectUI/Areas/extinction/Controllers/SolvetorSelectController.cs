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
    public class SolvetorSelectController : Controller
    {
        MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();

        // GET: extinction/SolvetorSelect
        public ActionResult Index()
        {
            return View();
        }

        //导出excel表格 
        public ActionResult Excel()
        {
            List<SolveRecore> list = new List<SolveRecore>();
            if (Session["solvelist"] != null)
            {
                //将session转为集合 
                list = (List<SolveRecore>)Session["solvelist"];
            }

            NPOI.HSSF.UserModel.HSSFWorkbook Book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            NPOI.SS.UserModel.ISheet sheet1 = Book.CreateSheet("Sheet1");
            NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
            row1.CreateCell(0).SetCellValue("任务编号");
            row1.CreateCell(1).SetCellValue("任务状态");
            row1.CreateCell(2).SetCellValue("工作单据");
            row1.CreateCell(3).SetCellValue("线路编号");
            row1.CreateCell(4).SetCellValue("杆塔编号");
            row1.CreateCell(5).SetCellValue("缺陷级别");
            row1.CreateCell(6).SetCellValue("缺陷类型");
            row1.CreateCell(7).SetCellValue("发现人");
            row1.CreateCell(8).SetCellValue("发现时间");
            row1.CreateCell(9).SetCellValue("下发人");
            row1.CreateCell(10).SetCellValue("下发时间");
            row1.CreateCell(11).SetCellValue("完好率");
            row1.CreateCell(12).SetCellValue("缺陷描述");
            for (int i = 0; i < list.Count; i++)
            {
                NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.CreateCell(0).SetCellValue(list[i].taskcode);
                rowtemp.CreateCell(1).SetCellValue(list[i].taskstatusname);
                rowtemp.CreateCell(2).SetCellValue(list[i].worktypename);
                rowtemp.CreateCell(3).SetCellValue(list[i].linecode);
                rowtemp.CreateCell(4).SetCellValue(list[i].polecode);
                rowtemp.CreateCell(5).SetCellValue(list[i].buglevelname);
                rowtemp.CreateCell(6).SetCellValue(list[i].bugtypename);
                rowtemp.CreateCell(7).SetCellValue(list[i].discovername);
                rowtemp.CreateCell(8).SetCellValue(list[i].discovertime.ToString());
                rowtemp.CreateCell(9).SetCellValue(list[i].issuedname);
                rowtemp.CreateCell(10).SetCellValue(list[i].issuedtime.ToString());
                rowtemp.CreateCell(11).SetCellValue(list[i].intactrate+"%");
                rowtemp.CreateCell(12).SetCellValue(list[i].bugdesc);

            }
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            Book.Write(ms);
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            DateTime dt = DateTime.Now;
            string dateTime = dt.ToString("yyMMddHHmmssff");
            string fileName = "查询结果" + dateTime + ".xls";
            return File(ms, "application/vnd.ms-excel", fileName);
        }

        public ActionResult GetData()
        {
            int pageindex = int.Parse(Request.Params["pageindex"]);

            //任务状态 
            var taskstaus = (from i in medc.ps_systemconfig
                             where i.configCode == "SOLVETASK_STATUS"
                             select i).ToList();
            //缺陷类型 
            var bugtype = (from b in medc.ps_systemconfig
                            where b.configCode == "BUG_TYPE"
                           select b).ToList();
            //缺陷级别 
            var buglevel = (from b in medc.ps_systemconfig
                            where b.configCode == "BUG_LEVEL"
                            select b).ToList();
            //工作单据 
            var workdoc = (from w in medc.ps_systemconfig
                           where w.configCode == "DOC_TYPE"
                           select w).ToList();

            var user = (from u in medc.ps_user
                        where u.id == int.Parse(Session["userid"].ToString())
                        select u).FirstOrDefault();

            var role = (from r in medc.ps_role
                        where r.id == user.roleId
                        select r).FirstOrDefault();

            List<SolveRecore> list =new List<SolveRecore>();

            if (role.roleName == "系统管理员" || role.roleName == "线路管理员")
            {
                var result = (from a in medc.ps_solvetask_main
                              join b in medc.ps_solvetask_detail
                              on a.id equals b.taskId
                              join c in medc.ps_inspectiontask_detail
                              on b.inspectionTaskDelId equals c.id
                              where a.taskStatus == 3 || a.taskStatus == 6
                              select new { taskcode = a.solveTaskCode, taskstatusname = a.taskStatusName, worktypename = a.workDocTypeName, linecode = c.lineCode, polecode = c.poleCode, buglevelname = c.bugLevelName, bugtypename = c.bugTypeName, discovername = c.discovererName, discovertime = c.discoverTime, issuedname = a.issuedByCode, issuedtime = a.issuedTime, intactrate = c.intactRate, bugdesc = c.bugDesc }).ToList();

                if (result.Count() > 0)
                {
                    foreach (var v in result)
                    {
                        SolveRecore recore = new SolveRecore()
                        {
                            taskcode = v.taskcode,
                            taskstatusname = v.taskstatusname,
                            worktypename = v.worktypename,
                            linecode = v.linecode,
                            polecode = v.polecode,
                            buglevelname = v.buglevelname,
                            bugtypename = v.bugtypename,
                            discovername = v.discovername,
                            discovertime = v.discovertime,
                            issuedname = v.issuedname,
                            issuedtime = v.issuedtime,
                            intactrate = v.intactrate
                           
                        };
                        if (v.bugdesc != null)
                        {
                            recore.bugdesc = v.bugdesc;
                            
                        }
                        list.Add(recore);
                    }
                }
            }
            else
            {
                var result = (from a in medc.ps_solvetask_main
                              join b in medc.ps_solvetask_detail
                              on a.id equals b.taskId
                              join c in medc.ps_inspectiontask_detail
                              on b.inspectionTaskDelId equals c.id
                              join d in medc.ps_solver_detail
                              on a.id equals d.taskId
                              where (a.taskStatus == 3 || a.taskStatus == 6)&&d.solverCode==user.userCode
                              select new { taskcode = a.solveTaskCode, taskstatusname = a.taskStatusName, worktypename = a.workDocTypeName, linecode = c.lineCode, polecode = c.poleCode, buglevelname = c.bugLevelName, bugtypename = c.bugTypeName, discovername = c.discovererName, discovertime = c.discoverTime, issuedname = a.issuedByCode, issuedtime = a.issuedTime, intactrate = c.intactRate, bugdesc = c.bugDesc }).ToList();

                if (result.Count() > 0)
                {
                    foreach (var v in result)
                    {
                        SolveRecore recore = new SolveRecore()
                        {
                            taskcode = v.taskcode,
                            taskstatusname = v.taskstatusname,
                            worktypename = v.worktypename,
                            linecode = v.linecode,
                            polecode = v.polecode,
                            buglevelname = v.buglevelname,
                            bugtypename = v.bugtypename,
                            discovername = v.discovername,
                            discovertime = v.discovertime,
                            issuedname = v.issuedname,
                            issuedtime = v.issuedtime,
                            intactrate = v.intactrate                            
                        };
                        if (v.bugdesc != null)
                        {
                            recore.bugdesc = v.bugdesc;
                        }
                        list.Add(recore);
                    }
                }
            }

          
            int skip = (pageindex - 1) * 6;

            DataSource ds = new DataSource();

            ds.taskstatuslist = taskstaus;

            ds.buglevellist = buglevel;

            ds.bugtypelist = bugtype;

            ds.worktypelist = workdoc;

            if (list.Count() > 0)
            {
                ds.srlist = list.Skip(skip).Take(6).ToList();
                ds.datacount = list.Count();
                Session["solvelist"] = ds.srlist;

            }
            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }
        public ActionResult SelectData()
        {
            string taskcode = "";
            string worktype = "";
            string bugtype = "";
            string buglevel = "";
            string taskstatus = "";
            string time1 = "";
            string time2 = "";
            int pageindex = int.Parse(Request.Params["pageindex"]);

            int skip = (pageindex - 1) * 6;

            string sql = "";

            var user = (from u in medc.ps_user
                        where u.id == int.Parse(Session["userid"].ToString())
                        select u).FirstOrDefault();

            var role = (from r in medc.ps_role
                        where r.id == user.roleId
                        select r).FirstOrDefault();

            if (role.roleName == "系统管理员" || role.roleName == "线路管理员")
            {
                sql = "select a.solveTaskCode,a.taskStatusName,a.workDocTypeName,c.lineCode,c.poleCode,c.bugLevelName,c.bugTypeName,c.discovererName,c.discoverTime,a.issuedByName,a.issuedTime,c.intactRate,c.bugDesc from ps_solvetask_main a inner join ps_solvetask_detail b on a.id=b.taskid inner join ps_inspectiontask_detail c on b.inspectionTaskDelId=c.id  where 1=1 and (a.taskstatus=3 or a.taskstatus=6)";
            }
            else
            {
                sql = "select a.solveTaskCode,a.taskStatusName,a.workDocTypeName,c.lineCode,c.poleCode,c.bugLevelName,c.bugTypeName,c.discovererName,c.discoverTime,a.issuedByName,a.issuedTime,c.intactRate,c.bugDesc from ps_solvetask_main a inner join ps_solvetask_detail b on a.id=b.taskid inner join ps_inspectiontask_detail c on b.inspectionTaskDelId=c.id inner join ps_solver_detail d on a.id=d.taskid where 1=1 and d.solverCode='"+user.userCode+ "' and (a.taskstatus=3 or a.taskstatus=6) ";
            
            }
            if (Request.Params["taskcode"] != "")
            {
                taskcode = Request.Params["taskcode"];
                sql += " and a.solveTaskCode like '%" + taskcode + "%' ";
            }
            if (Request.Params["taskstatus"] != "")
            {
                taskstatus = Request.Params["taskstatus"];
                sql += " and a.taskstatus='" + taskstatus + "'";
            }

            if (Request.Params["buglevel"] != "")
            {
                buglevel = Request.Params["buglevel"];
                sql += " and c.bugLevel=" + buglevel;
            }
            if (Request.Params["bugtype"] != "")
            {
                bugtype = Request.Params["bugtype"];
                sql += " and c.bugType" + bugtype;
            }
            if (Request.Params["worktype"] != "")
            {
                worktype = Request.Params["worktype"];
                sql += " and a.workDocType =" + worktype;
            }
            if (Request.Params["time1"] != null)
            {
                time1 = Request.Params["time1"];
                time2 = Request.Params["time2"];
                sql += " and a.issuedTime>= '" + time1 + "' and a.issuedTime<='" + time2 + "'";
            }
         

            SqlConnection con = new SqlConnection("server=.;database=MyElectrCheck_DB;Integrated Security=true;");
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            SqlCommand cmd = new SqlCommand(sql, con);

            SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

            List<SolveRecore> list = new List<SolveRecore>();

            while (dr.Read())
            {
                SolveRecore main = new SolveRecore();
                main.taskcode = dr["solveTaskCode"].ToString();
                main.taskstatusname = dr["taskStatusName"].ToString();
                main.worktypename = dr["workDocTypeName"].ToString();
                main.issuedname = dr["issuedByName"].ToString();
                main.issuedtime = DateTime.Parse(dr["issuedTime"].ToString());
                main.linecode = dr["lineCode"].ToString();
                main.polecode = dr["poleCode"].ToString();
                main.discovername = dr["discovererName"].ToString();
                main.discovertime = DateTime.Parse( dr["discoverTime"].ToString());
                main.intactrate = int.Parse(dr["intactRate"].ToString());
                main.bugdesc = dr["bugDesc"].ToString();
                main.buglevelname = dr["bugLevelName"].ToString();
                main.bugtypename = dr["bugTypeName"].ToString();
               
                list.Add(main);
            }
            dr.Close();
            con.Close();

            DataSource ds = new DataSource();

            if (list != null)
            {
                ds.srlist = list.Skip(skip).Take(6).ToList();
                ds.datacount = list.Count();
                Session["solvelist"] = ds.srlist;
            }

            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);

        }
    }
}