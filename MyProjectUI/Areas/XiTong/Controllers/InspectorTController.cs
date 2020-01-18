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
    public class InspectorTController : Controller
    {
        MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();

        // GET: XiTong/InspectorT
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetInspectData()
        {
            int id = int.Parse(Session["taskid"].ToString());

            //首先获得该任务下的线路便号
            var line = (from i in medc.ps_inspectiontask_main
                        where i.id == id
                        select i).FirstOrDefault();

            //获得该任务编号下的所有杆塔信息 
            var poles = (from i in medc.ps_inspectiontask_detail
                         where i.taskid == id && i.isBug==1
                         select i).ToList();

            //查询录入杆塔下的第一个杆塔信息 
            var inspdetail = (from i in medc.ps_inspectiontask_detail
                              where i.poleid == poles.FirstOrDefault().poleid
                              select i).FirstOrDefault();


            //查询所有缺陷类型信息 
            var bugtype = (from b in medc.ps_systemconfig
                           where b.configName == "缺陷类型" && b.isStart == 1
                           select b).ToList();
            //查询所有的缺陷级别信息 
            var buglevel = (from b in medc.ps_systemconfig
                            where b.configName == "缺陷级别" && b.isStart == 1
                            select b).ToList();

            var idetails = (from r in medc.ps_inspectiontask_detail
                            where r.taskid == id
                            select r).ToList();

            DataSource ds = new DataSource();

           
            ds.bugtypelist = bugtype;

            ds.buglevellist = buglevel;

            ds.taskdetails = poles;

            ds.taskdet = inspdetail;
            ds.taskmain = line;

            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }

        public ActionResult UpdDateInspect()
        {
            int taskid = int.Parse(Request.Params["taskid"]);
            int intactrate = -1;
            int isbug = int.Parse(Request.Params["isbug"]);
            string buglevelname = "";
            string bugtypename = "";
            int buglevel = -1;
            int bugtype = -1;
            string desc = "";


            if (Request.Params["buglevelname"] != "" && Request.Params["buglevel"] != "")
            {
                buglevelname = Request.Params["buglevelname"];
                buglevel = int.Parse(Request.Params["buglevel"]);
            }
            if (Request.Params["bugtypename"] != "" && Request.Params["bugtype"] != "")
            {
                bugtypename = Request.Params["bugtypename"];
                bugtype = int.Parse(Request.Params["bugtype"]);
            }
            if (Request.Params["bugdesc"] != "")
            {
                desc = Request.Params["bugdesc"];
            }
            if (Request.Params["intactrate"] != "")
            {
                intactrate= int.Parse(Request.Params["intactrate"]);
            }

            var dets = (from i in medc.ps_inspectiontask_detail
                        where i.id == taskid
                        select i).FirstOrDefault();



            dets.isBug = isbug;      
         

            if (intactrate != -1)
            {
                dets.intactRate = intactrate;
            }else
            {
                dets.intactRate = null;
            }
            if (buglevelname != "")
            {
                dets.bugLevelName = buglevelname;
                dets.bugLevel = buglevel;
            }
            else
            {
                dets.bugLevel = null;
                dets.bugLevelName = null;
            }
            if (bugtypename != "")
            {
                dets.bugTypeName = bugtypename;
                dets.bugType = bugtype;
            }
            else
            {
                dets.bugType = null;
                dets.bugTypeName = null;
            }
            if (desc != "")
            {
                dets.bugDesc = desc;
            }
            else
            {
                dets.bugDesc = "无";
            }

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

      public ActionResult GetData()
        {
            int pageindex = int.Parse(Request.Params["pageindex"]);

            var user = (from u in medc.ps_user
                        where u.id == int.Parse(Session["userid"].ToString())
                        select u).FirstOrDefault();

            var role = (from r in medc.ps_role
                        where r.id == user.roleId
                        select r).FirstOrDefault();

            List<InspectorSelect> islist = new List<InspectorSelect>();

            if (role.roleName == "系统管理员")
            {
                var flawlist = (from a in medc.ps_inspectiontask_main
                                join b in medc.ps_inspectiontask_detail
                                on a.id equals b.taskid
                                where b.isSolve == 0 && b.isBug == 1 && a.taskStatus == 4
                                select new { taskcode = a.inspectionTaskCode, linecode = a.lineCode, polecode = b.poleCode, buglevelname = b.bugLevelName, bugtypename = b.bugTypeName, discovername = b.discovererName, discovertime = b.discoverTime, issuedname = a.issuedByName, issuedtime = a.issuedTime, intactrate = b.intactRate, bugdesc = b.bugDesc }).ToList();


                if (flawlist.Count() > 0)
                {
                    foreach (var v in flawlist)
                    {
                        InspectorSelect inspeselect = new InspectorSelect()
                        {
                            taskcode = v.taskcode,
                            linecode = v.linecode,
                            polecode = v.polecode,
                            buglevelname = v.buglevelname,
                            bugtypename = v.bugtypename,
                            bugdesc = v.bugdesc,
                            discovername = v.discovername,
                            discovertime = v.discovertime,
                            issuedname = v.issuedname,
                            issuedtime = v.issuedtime,
                            intactrate = v.intactrate
                        };
                        islist.Add(inspeselect);
                    }
                }
              
            }
            else
            {
                var flawlist = (from a in medc.ps_inspectiontask_main
                                join b in medc.ps_inspectiontask_detail
                                on a.id equals b.taskid
                                join c in medc.ps_inspector_detail
                                on a.id equals c.taskId
                                where b.isSolve == 0 && b.isBug == 1 && a.taskStatus == 4&&c.inspectorCode==user.userCode
                                select new { taskcode = a.inspectionTaskCode, linecode = a.lineCode, polecode = b.poleCode, buglevelname = b.bugLevelName, bugtypename = b.bugTypeName, discovername = b.discovererName, discovertime = b.discoverTime, issuedname = a.issuedByName, issuedtime = a.issuedTime, intactrate = b.intactRate, bugdesc = b.bugDesc }).ToList();



                if (flawlist.Count() > 0)
                {
                    foreach (var v in flawlist)
                    {
                        InspectorSelect inspeselect = new InspectorSelect()
                        {
                            taskcode = v.taskcode,
                            linecode = v.linecode,
                            polecode = v.polecode,
                            buglevelname = v.buglevelname,
                            bugtypename = v.bugtypename,
                            bugdesc = v.bugdesc,
                            discovername = v.discovername,
                            discovertime = v.discovertime,
                            issuedname = v.issuedname,
                            issuedtime = v.issuedtime,
                            intactrate = v.intactrate
                        };
                        islist.Add(inspeselect);
                    }
                }
            }
           

            //缺陷类型 
            var bugtype = (from b in medc.ps_systemconfig
                           where b.configCode == "BUG_TYPE"
                           select b).ToList();
            //缺陷级别 
            var buglevel = (from b in medc.ps_systemconfig
                            where b.configCode == "BUG_LEVEL"
                            select b).ToList();

            int skip = (pageindex - 1) * 6;

            DataSource ds = new DataSource();

            if (islist.Count() > 0)
            {
                ds.islist = islist.Skip(skip).Take(6).ToList();

                Session["buglist"] = ds.islist;

                ds.pagecount = islist.Count();
            }

            ds.buglevellist = buglevel;

            ds.bugtypelist = bugtype;

            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }

        //导出excel表格 
        public ActionResult Excel()
        {
            List<InspectorSelect> list = new List<InspectorSelect>();
            if (Session["buglist"] != null)
            {
                //将session转为集合 
                list = (List<InspectorSelect>)Session["buglist"];
            }

            NPOI.HSSF.UserModel.HSSFWorkbook Book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            NPOI.SS.UserModel.ISheet sheet1 = Book.CreateSheet("Sheet1");
            NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
            row1.CreateCell(0).SetCellValue("任务编号");
            row1.CreateCell(1).SetCellValue("杆塔编号");
            row1.CreateCell(2).SetCellValue("缺陷级别");
            row1.CreateCell(3).SetCellValue("缺陷类型");
            row1.CreateCell(4).SetCellValue("发现人");
            row1.CreateCell(5).SetCellValue("发现时间");
            row1.CreateCell(6).SetCellValue("下发人");
            row1.CreateCell(7).SetCellValue("下发时间");
            row1.CreateCell(8).SetCellValue("发现人");
            row1.CreateCell(9).SetCellValue("完好率");
            row1.CreateCell(10).SetCellValue("缺陷描述");
            for (int i = 0; i < list.Count; i++)
            {
                NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.CreateCell(0).SetCellValue(list[i].taskcode);
                rowtemp.CreateCell(1).SetCellValue(list[i].linecode);
                rowtemp.CreateCell(2).SetCellValue(list[i].polecode);
                rowtemp.CreateCell(3).SetCellValue(list[i].buglevelname);
                rowtemp.CreateCell(4).SetCellValue(list[i].bugtypename);
                rowtemp.CreateCell(5).SetCellValue(list[i].discovername);
                rowtemp.CreateCell(6).SetCellValue(list[i].discovertime.ToString());
                rowtemp.CreateCell(7).SetCellValue(list[i].issuedname);
                rowtemp.CreateCell(8).SetCellValue(list[i].issuedname);
                rowtemp.CreateCell(9).SetCellValue(list[i].issuedtime.ToString());
                rowtemp.CreateCell(10).SetCellValue(list[i].bugdesc);

            }
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            Book.Write(ms);
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            DateTime dt = DateTime.Now;
            string dateTime = dt.ToString("yyMMddHHmmssff");
            string fileName = "查询结果" + dateTime + ".xls";
            return File(ms, "application/vnd.ms-excel", fileName);
        }
        public ActionResult SelectData()
        {
            string taskcode = "";
            string linecode = "";
            string bugtype = "";
            string buglevel = "";
            string time1 = "";
            string time2 = "";
            string time3 = "";
            string time4 = "";
            int pageindex = int.Parse(Request.Params["pageindex"]);

            int skip = (pageindex - 1) * 6;

            var user = (from u in medc.ps_user
                        where u.id == int.Parse(Session["userid"].ToString())
                        select u).FirstOrDefault();

            var role = (from r in medc.ps_role
                        where r.id == user.roleId
                        select r).FirstOrDefault();

            string sql = "";

            if (role.roleName == "系统管理员" )
            {
                sql = "select a.inspectionTaskCode,b.lineCode,b.polecode,b.bugLevelName,b.bugTypeName,b.discovererName,b.discoverTime,a.issuedByName,a.issuedTime,b.intactRate,b.bugDesc from ps_inspectiontask_main a,ps_inspectiontask_detail b  where a.id=b.taskid and b.isSolve=0 and b.isBug=1 and a.taskStatus=4 ";
            }
            else
            {
                sql = "select a.inspectionTaskCode,b.lineCode,b.polecode,b.bugLevelName,b.bugTypeName,b.discovererName,b.discoverTime,a.issuedByName,a.issuedTime,b.intactRate,b.bugDesc from ps_inspectiontask_main a inner join  ps_inspectiontask_detail b on a.id=b.taskid inner join ps_inspector_detail c on a.id=c.taskid  where b.isSolve=0 and b.isBug=1 and a.taskStatus=4 and c.inspectorCode='"+user.userCode+"' ";
            }
            if (Request.Params["taskcode"] != "")
            {
                taskcode = Request.Params["taskcode"];
                sql += " and a.inspectionTaskCode like '%" + taskcode + "%' ";
            }
            if (Request.Params["linecode"] != "")
            {
                linecode = Request.Params["linecode"];
                sql += " and b.linecode like '%" + linecode + "%'";
            }

            if (Request.Params["buglevel"] != "")
            {
                buglevel = Request.Params["buglevel"];
                sql += " and b.bugLevel=" + buglevel;
            }
            if (Request.Params["bugtype"] != "")
            {
                bugtype = Request.Params["bugtype"];
                sql += " and b.bugType=" + bugtype;
            }

            if (Request.Params["ptime1"] != "")
            {
                time1 = Request.Params["ptime1"];
                time2 = Request.Params["ptime2"];
                sql += " and b.discoverTime>= '" + time1 + "' and b.discoverTime<='" + time2 + "'";
            }
            if (Request.Params["ptime3"] != "")
            {
                time3 = Request.Params["ptime3"];
                time4 = Request.Params["ptime4"];
                sql += " and a.issuedTime>= '" + time3 + "' and a.issuedTime<='" + time4 + "'";
            }
         
            SqlConnection con = new SqlConnection("server=.;database=MyElectrCheck_DB;Integrated Security=true;");
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            SqlCommand cmd = new SqlCommand(sql, con);

            SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

            List<InspectorSelect> list = new List<InspectorSelect>();

            while (dr.Read())
            {
                InspectorSelect main = new InspectorSelect();
                main.taskcode = dr["inspectionTaskCode"].ToString();
                main.linecode = dr["lineCode"].ToString();
                main.polecode = dr["polecode"].ToString();
                main.bugtypename = dr["bugtypename"].ToString();
                main.buglevelname = dr["buglevelname"].ToString();
                main.discovername = dr["discoverername"].ToString();
                main.discovertime =DateTime.Parse(dr["discovertime"].ToString());
                main.issuedname = dr["issuedbyname"].ToString();
                main.issuedtime = DateTime.Parse(dr["issuedTime"].ToString());
                main.intactrate = int.Parse(dr["intactrate"].ToString());
                main.bugdesc = dr["bugdesc"].ToString();
               
                list.Add(main);
            }

            dr.Close();
            con.Close();

            DataSource ds = new DataSource();

            ds.islist = list.Skip(skip).Take(6).ToList();
            ds.pagecount = list.Count();
            Session["buglist"] = ds.islist;

            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }
    }
}