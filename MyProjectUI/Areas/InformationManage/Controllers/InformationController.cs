using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyProjectUI.Models;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using MyProjectUI.Areas.InformationManage.Models;
using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using Microsoft.Reporting.WebForms;
using System.IO;
namespace MyProjectUI.Areas.InformationManage.Controllers
{
    public class InformationController : Controller
    {
        MyElectrCheck_DBDataContext md = new MyElectrCheck_DBDataContext();
        // GET: InformationManage/Information
        public ActionResult Index()
        {

            return View();
        }
        public FileResult Excel1()
        {
            MyElectrCheck_DBDataContext mdc = new MyElectrCheck_DBDataContext();
          
            List<DataManager> list = new List<DataManager>();
            if (Session["inspectlist"] != null)
            {
                //将session转为集合 
                list = (List<DataManager>)Session["inspectlist"];
            }

            NPOI.HSSF.UserModel.HSSFWorkbook Book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            NPOI.SS.UserModel.ISheet sheet1 = Book.CreateSheet("Sheet1");
            NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
            row1.CreateCell(0).SetCellValue("任务编号");
            row1.CreateCell(1).SetCellValue("任务名称");
            row1.CreateCell(2).SetCellValue("线路编号(起始杆号-终止杆号)");
            row1.CreateCell(3).SetCellValue("杆塔编号");
            row1.CreateCell(4).SetCellValue("缺陷级别");
            row1.CreateCell(5).SetCellValue("缺陷类型");
            row1.CreateCell(6).SetCellValue("发现时间");
            row1.CreateCell(7).SetCellValue("缺陷描述");
            for (int i = 0; i < list.Count; i++)
            {
                NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.CreateCell(0).SetCellValue(list[i].inspectionTaskCode);
                rowtemp.CreateCell(1).SetCellValue(list[i].inspectionTaskName);
                rowtemp.CreateCell(2).SetCellValue(list[i].lineCode+"("+list[i].startPoleCode+"-"+list[i].endPoleCode+")");
                rowtemp.CreateCell(3).SetCellValue(list[i].poleCode);
                rowtemp.CreateCell(4).SetCellValue(list[i].buglevelname);
                rowtemp.CreateCell(5).SetCellValue(list[i].bugtypename);
                rowtemp.CreateCell(6).SetCellValue(list[i].discoverTime.ToString());
                rowtemp.CreateCell(7).SetCellValue(list[i].bugDesc);

            }
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            Book.Write(ms);
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            DateTime dt = DateTime.Now;
            string dateTime = dt.ToString("yyMMddHHmmssff");
            string fileName = "查询结果" + dateTime + ".xls";
            return File(ms, "application/vnd.ms-excel", fileName);
        }
        public ActionResult IndexData()
        {
            var result = (from i in md.ps_inspectiontask_main
                          join d in md.ps_inspectiontask_detail
                          on i.id equals d.taskid
                          where i.taskStatus==4&&i.isCancel==0&&d.isSolve==0&&d.isBug==1
                          select new { id = i.id, inspectionTaskCode = i.inspectionTaskCode, inspectionTaskName = i.inspectionTaskName, lineCode = i.lineCode, startPoleCode = i.startPoleCode, endPoleCode = i.endPoleCode, poleCode = d.poleCode, bugLevel = d.bugLevel, bugType = d.bugType,buglevelname=d.bugLevelName,bugtypename=d.bugTypeName, discoverTime = d.discoverTime, bugDesc = d.bugDesc });
            List<DataManager> dmlist = new List<DataManager>();
            foreach (var v in result)
            {
                DataManager dm = new DataManager()
                {
                    id = v.id,
                    inspectionTaskCode = v.inspectionTaskCode,
                    inspectionTaskName = v.inspectionTaskName,
                    lineCode = v.lineCode,
                    startPoleCode = v.startPoleCode,
                    endPoleCode = v.endPoleCode,
                    poleCode = v.poleCode,
                    bugLevel = v.bugLevel,
                    buglevelname=v.buglevelname,
                    bugtypename=v.bugtypename,
                    bugType = v.bugType,
                    discoverTime = v.discoverTime,
                    bugDesc = v.bugDesc,
                    discoverTime2=""
                };
                dmlist.Add(dm);
            }

            //缺陷类型 
            var bugtypelist = (from b in md.ps_systemconfig
                               where b.configCode == "BUG_TYPE"
                               select b).ToList();

            //缺陷级别 
            var buglevellist = (from b in md.ps_systemconfig
                                where b.configCode == "BUG_LEVEL"
                                select b).ToList();

            int pageindex = int.Parse(Request.Params["pageindex"].ToString());
            int skip = (pageindex - 1) * 4;
            DataSource ds = new DataSource();

            ds.list = dmlist.Skip(skip).Take(4).ToList();

            ds.Pagecount = dmlist.Count();

            //将list保存到集合中 
            ds.buglevellist = buglevellist;

            ds.bugtypelist = bugtypelist;

            Session["inspectlist"] = ds.list;

            string str = JsonConvert.SerializeObject(ds);

            return Content(str);
        }

        public ActionResult ReportInspectData()
        {
            string aspx = "/AspNets/RptView.aspx";

            using (var sw = new StringWriter())
            {
                System.Web.HttpContext.Current.Server.Execute(aspx, sw, true);
                return Content(sw.ToString());
            }

        }
        public ActionResult SelectData()
        {
            string taskcode = "";
            string linecode = "";
            string time1 = "";
            string time2 = "";
            string bugtype = "";
            string buglevel = "";

            int pageindex = int.Parse(Request.Params["pageindex"]);

            SqlConnection con = new SqlConnection("server=.;database=MyElectrCheck_DB;Integrated Security=true;");
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            string sql = "select a.id, a.inspectionTaskCode,a.inspectionTaskName,a.linecode,a.startpolecode,a.endpolecode,b.polecode,b.bugtype,b.buglevel,b.buglevelname,b.bugtypename,b.discovertime,b.bugdesc from ps_inspectiontask_main a inner join ps_inspectiontask_detail b on a.id=b.taskid where 1=1 and a.taskstatus=4 and a.iscancel=0 and b.issolve=0 and b.isbug=1 ";

            if (Request.Params["taskcode"] != "")
            {
                taskcode = Request.Params["taskcode"];
                sql += " and a.inspectionTaskCode like '%" + taskcode + "%'";
            }
            if (Request.Params["linecode"] != "")
            {
                linecode = Request.Params["linecode"];
                sql += "and a.lineCode like '%" + linecode + "%'";
            }
            if (Request.Params["bugtype"] != "")
            {
                bugtype = Request.Params["bugtype"];
                sql += "and b.bugtype =" + bugtype + "";
            }
            if (Request.Params["buglevel"] != "")
            {
                buglevel = Request.Params["buglevel"];
                sql += "and b.buglevel =" + buglevel + "";
            }
            if (Request.Params["time1"] !="")
            {
                time1 = Request.Params["time1"];
                sql += "and b.discoverTime >= '" + time1 + "' ";
            }
            if (Request.Params["time2"] !="")
            {
                time2 = Request.Params["time2"];
                sql += " and b.discoverTime <= '" + time2 + "' ";
            }

            SqlCommand cmd = new SqlCommand(sql, con);

            SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

            List<DataManager> list = new List<DataManager>();

            while (dr.Read())
            {
                //DataManager data = new DataManager()
                //{
                //    id = int.Parse(dr["id"].ToString()),
                //    inspectionTaskCode = dr["inspectionTaskCode"].ToString(),
                //    inspectionTaskName = dr["inspectionTaskName"].ToString(),
                //    lineCode = dr["lineCode"].ToString(),
                //    startPoleCode = dr["startPoleCode"].ToString(),
                //    endPoleCode = dr["endPoleCode"].ToString(),
                //    poleCode = dr["poleCode"].ToString(),
                //    bugLevel = int.Parse(dr["bugLevel"].ToString()),
                //    bugType = int.Parse(dr["bugType"].ToString()),
                //    buglevelname=dr["buglevelname"].ToString(),
                //    bugtypename=dr["bugtypename"].ToString(),
                //    discoverTime = DateTime.Parse(dr["discoverTime"].ToString()),
                //    bugDesc = dr["bugDesc"].ToString()
                //};
                DataManager dm = new DataManager();
                dm.id = int.Parse(dr["id"].ToString());
                dm.inspectionTaskCode = dr["inspectionTaskCode"].ToString();
                dm.inspectionTaskName = dr["inspectionTaskName"].ToString();
                dm.lineCode = dr["lineCode"].ToString();
                dm.startPoleCode = dr["startPoleCode"].ToString();
                dm.endPoleCode = dr["endPoleCode"].ToString();
                dm.poleCode = dr["poleCode"].ToString();
                if (dr["buglevel"].ToString() != "")
                {
                    dm.bugLevel = int.Parse(dr["bugLevel"].ToString());
                }
                else
                {
                    dm.bugLevel = null;
                }
                if (dr["bugtype"].ToString() != "")
                {
                    dm.bugType = int.Parse(dr["bugtype"].ToString());
                }
                else
                {
                    dm.bugType = null;
                }
                if (dr["buglevelname"].ToString() != "")
                {
                    dm.buglevelname = dr["buglevelname"].ToString();
                }
                else
                {
                    dm.buglevelname = null;
                }
                if (dr["bugtypename"].ToString() != "")
                {
                    dm.bugtypename = dr["bugtypename"].ToString();
                }
                else
                {
                    dm.bugtypename = null;
                }
                if (dr["bugdesc"].ToString() != "")
                {
                    dm.bugDesc = dr["bugdesc"].ToString();
                }
                else
                {
                    dm.bugDesc = null;
                }
                dm.discoverTime = DateTime.Parse(dr["discoverTime"].ToString());
                dm.discoverTime2 = "";
                list.Add(dm);
            }

            dr.Close();
            con.Close();

            int skip = (pageindex - 1) * 4;
            DataSource ds = new DataSource();
            ds.list = list.Skip(skip).Take(4).ToList();
            ds.Pagecount = list.Count();
            Session["inspectlist"] = ds.list;
            string str = JsonConvert.SerializeObject(ds);
            return Content(str);
        }
        //消缺记录统计页面
        public ActionResult EliminateCount()
        {
            return View();
        }

        public FileResult Excel2()
        {
            MyElectrCheck_DBDataContext mdc = new MyElectrCheck_DBDataContext();

            List<DataEliminate> list = new List<DataEliminate>();
            if (Session["solvelist"] != null)
            {
                //将session转为集合 
                list = (List<DataEliminate>)Session["solvelist"];
            }

            NPOI.HSSF.UserModel.HSSFWorkbook Book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            NPOI.SS.UserModel.ISheet sheet1 = Book.CreateSheet("Sheet1");
            NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
            row1.CreateCell(0).SetCellValue("任务编号");
            row1.CreateCell(1).SetCellValue("任务名称");
            row1.CreateCell(2).SetCellValue("线路编号");
            row1.CreateCell(3).SetCellValue("线路编号");
            row1.CreateCell(4).SetCellValue("有无故障");
            row1.CreateCell(5).SetCellValue("缺陷级别");
            row1.CreateCell(6).SetCellValue("缺陷类型");
            row1.CreateCell(7).SetCellValue("消缺时间");
            row1.CreateCell(8).SetCellValue("发现时间");
            row1.CreateCell(9).SetCellValue("缺陷描述");
            for (int i = 0; i < list.Count; i++)
            {
                NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.CreateCell(0).SetCellValue(list[i].solveTaskCode);
                rowtemp.CreateCell(1).SetCellValue(list[i].solveTaskName);
                rowtemp.CreateCell(2).SetCellValue(list[i].lineCode);
                if (list[i].isBug == 1)
                {
                    rowtemp.CreateCell(3).SetCellValue("有");
                }
                else
                {
                    rowtemp.CreateCell(3).SetCellValue("无");
                }
                rowtemp.CreateCell(4).SetCellValue(list[i].bugLevelName);
                rowtemp.CreateCell(5).SetCellValue(list[i].bugLevelName);
                rowtemp.CreateCell(6).SetCellValue(list[i].bugTypeName);
                rowtemp.CreateCell(7).SetCellValue(list[i].finishTime.ToString());
                rowtemp.CreateCell(8).SetCellValue(list[i].discoverTime.ToString());
                rowtemp.CreateCell(9).SetCellValue(list[i].bugDesc);

            }
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            Book.Write(ms);
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            DateTime dt = DateTime.Now;
            string dateTime = dt.ToString("yyMMddHHmmssff");
            string fileName = "查询结果" + dateTime + ".xls";
            return File(ms, "application/vnd.ms-excel", fileName);
        }

        public ActionResult ReportSolveData()
        {
            //List<DataEliminate> list = new List<DataEliminate>();
            //if (Session["solvelist"] != null)
            //{
            //    //将session转为集合 
            //    list = (List<DataEliminate>)Session["solvelist"];

            //   foreach(var v in list)
            //    {
            //        if (v.isBug == 1)
            //        {
            //            v.BugName = "有";
            //        }
            //        else
            //        {
            //            v.BugName = "无";
            //        }
            //    }
            //}


            //LocalReport localreport = new LocalReport();
            //localreport.ReportPath = Server.MapPath("~/Models/SolveReport.rdlc");

            //ReportDataSource reportdatasource = new ReportDataSource("SolveRpt", list);

            //localreport.DataSources.Add(reportdatasource);

            //string reportType = "PDF";
            //string mimeType;
            //string encoding;
            //string fileNameExtension;

            //string deviceInfo = "<DeviceInfo><OutputFormat></OutputFormat><PageWidth>8.5in</PageWidth><PageHeight>11in</PageHeight><MarginTop>0.5in</MarginTop><MarginLeft>1in</MarginLeft><MarginRight>1in</MarginRight><MarginBottom>0.5in</MarginBottom></DeviceInfo>";

            //Warning[] warnlist;
            //string[] streams;
            //byte[] renderedbytes;

            //renderedbytes = localreport.Render(
            //     reportType,
            //     deviceInfo,
            //     out mimeType,
            //     out encoding,
            //     out fileNameExtension,
            //     out streams,
            //     out warnlist
            //    );

            //return File(renderedbytes, "utf-8");

            string aspx = "/AspNets/RptView2.aspx";

            using (var sw = new StringWriter())
            {
                System.Web.HttpContext.Current.Server.Execute(aspx, sw, true);
                return Content(sw.ToString());
            }
        }

        public ActionResult EliminateCountData()
        {
            var results = (from r in md.ps_solvetask_main
                           join i in md.ps_solvetask_detail
                           on r.id equals i.taskId
                           join p in md.ps_inspectiontask_detail
                           on i.inspectionTaskDelId equals p.id
                           join d in md.ps_inspectiontask_main
                           on p.taskid equals d.id
                           where r.taskStatus==5&&r.isCancel==0
                           select new { id = r.id, solveTaskCode = r.solveTaskCode,finishtime=r.finishTime, solveTaskName = r.solveTaskName, lineCode = p.lineCode, startPoleCode = d.startPoleCode, endPoleCode = d.endPoleCode, poleCode = p.poleCode, isBug = p.isBug, bugLevel = p.bugLevelName, bugType = p.bugTypeName, issuedTime = r.issuedTime, discoverTime = p.discoverTime, bugDesc = p.bugDesc });
            List<DataEliminate> delist = new List<DataEliminate>();
            if (results.Count() > 0)
            {
                foreach (var f in results)
                {
                    DataEliminate de = new DataEliminate()
                    {
                        id = f.id,
                        solveTaskCode = f.solveTaskCode,
                        solveTaskName = f.solveTaskName,
                        lineCode = f.lineCode,
                        startPoleCode = f.startPoleCode,
                        endPoleCode = f.endPoleCode,
                        poleCode = f.poleCode,
                        isBug = f.isBug,
                        bugLevelName = f.bugLevel,
                        bugTypeName = f.bugType,
                        finishTime = f.finishtime,
                        discoverTime = f.discoverTime,
                        bugDesc = f.bugDesc
                    };
                    delist.Add(de);
                }
            }
            int pageindex = int.Parse(Request.Params["pageindex"].ToString());
            int skip = (pageindex - 1) * 4;
            DataCount dc = new DataCount();
            if (delist.Count() > 0)
            {
                dc.plist = delist.Skip(skip).Take(4).ToList();
                dc.PageCount = delist.Count();
                Session["solvelist"] = dc.plist;
            }

            string strs = JsonConvert.SerializeObject(dc);
            return Content(strs);
        }
        public ActionResult SelectBtn()
        {
            string sovercode = "";
            string sovername = "";
            string isbug = "";
            string time1 = "";
            string time2 = "";

            int pageindex = int.Parse(Request.Params["pageindex"]);
            SqlConnection con = new SqlConnection("server=.;database=MyElectrCheck_DB;Integrated Security=true;");
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            string sql = "select a.id,a.solveTaskCode,a.solveTaskName,a.finishTime,c.lineCode,d.startPoleCode,d.endPoleCode,c.poleCode,c.isBug,c.bugLevelName,c.bugTypeName,a.issuedTime,c.discoverTime,c.bugDesc from ps_solvetask_main a inner join [ps_solvetask_detail] b on a.id=b.taskid inner join ps_inspectiontask_detail c on b.inspectionTaskDelId=c.id inner join ps_inspectiontask_main d on c.taskid=d.id where 1=1 and a.taskstatus=5 and a.iscancel=0 ";
            if (Request.Params["sovercode"] != "")
            {
                sovercode = Request.Params["sovercode"];
                sql += " and a.solveTaskCode like '%" + sovercode + "%'";
            }
            if (Request.Params["sovername"] != "")
            {
                sovername = Request.Params["sovername"];
                sql += "and c.lineCode like '%" + sovername + "%'";
            }
            if (Request.Params["isbug"] != "")
            {
                isbug = Request.Params["isbug"];
                sql += "and c.isBug =" + isbug + "";
            }
            if (Request.Params["time1"] != "")
            {
                time1 = Request.Params["time1"];
                sql += "and a.finishTime >= '" + time1 + "' ";
            }
            if (Request.Params["time2"] != "")
            {
                time2 = Request.Params["time2"];
                sql += "and a.finishTime <= '" + time2 + "' ";
            }
            SqlCommand cmd = new SqlCommand(sql, con);

            SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

            List<DataEliminate> list = new List<DataEliminate>();

            while (dr.Read())
            {
                DataEliminate del = new DataEliminate()
                {
                    id = int.Parse(dr["id"].ToString()),
                    solveTaskCode = dr["solveTaskCode"].ToString(),
                    solveTaskName = dr["solveTaskName"].ToString(),
                    lineCode = dr["lineCode"].ToString(),
                    startPoleCode = dr["startPoleCode"].ToString(),
                    endPoleCode = dr["endPoleCode"].ToString(),
                    poleCode = dr["poleCode"].ToString(),
                    isBug = int.Parse(dr["isBug"].ToString()),
                    bugLevelName = dr["bugLevelName"].ToString(),
                    bugTypeName = dr["bugTypeName"].ToString(),
                    finishTime = DateTime.Parse(dr["finishTime"].ToString()),
                    discoverTime = DateTime.Parse(dr["discoverTime"].ToString()),
                    bugDesc = dr["bugDesc"].ToString()
                };
                list.Add(del);
            }

            dr.Close();
            con.Close();

            int skip = (pageindex - 1) * 4;
            DataCount ds = new DataCount();
            if (list.Count() > 0)
            {
                ds.plist = list.Skip(skip).Take(4).ToList();
                ds.PageCount = list.Count();
                Session["solvelist"] = ds.plist;
            }
            string strs = JsonConvert.SerializeObject(ds); ;
            return Content(strs);
        }
    }
}