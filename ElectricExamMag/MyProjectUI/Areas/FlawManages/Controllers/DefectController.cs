using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using MyProjectUI.Areas.FlawManages.Models;
using MyProjectUI.Models;
namespace MyProjectUI.Areas.FlawManages.Controllers
{
    public class DefectController : Controller
    {
        static MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();
        // GET: FlawManages/Defect
        public ActionResult Index()
        {
            return View();
        }
        //处理工作平台传的ID而返回存储结果页面的action
        public ActionResult ResultAction()
        {
            #region 处理工作平台ID
            Session["DefectID"] = int.Parse(Request.Params["id"]);

            bool b = true;

            if (Session["DefectID"].ToString() == "0")
            {
                b = false;
            }
            #endregion

            return Json(b);
        }
        public ActionResult RedIndex()
        {
            return View();
        }
        public ActionResult ResultData()
        {
            #region Session中ID数据
            //获取Session["DefectID"]中的ID
            var result1 = (from a in medc.ps_inspectiontask_detail
                           join b in medc.ps_inspectiontask_main on a.taskid equals b.id
                           where a.id == Convert.ToInt32(Session["taskmainid"])
                           select new { a.id, b.inspectionTaskCode, a.lineCode, a.poleCode, a.bugTypeName, a.intactRate, a.bugDesc, a.discoverTime, a.discovererName }).FirstOrDefault();
            #endregion

            #region 查出缺陷等级选项
             var result2 = (from a in medc.ps_systemconfig
                              where a.configCode == "BUG_LEVEL"
                              select a).ToList();
            #endregion

            var result3 = new { result1, result2 };

            string jsonstr = JsonConvert.SerializeObject(result3);

            return Content(jsonstr);
        }
        public ActionResult AddBugLevel()
        {
            #region 添加缺陷处理等级
            int id = int.Parse(Request.Params["id"]);

            int? buglevel = int.Parse(Request.Params["buglevel"]);

            string buglevelname = Request.Params["buglevelname"];

            ps_inspectiontask_detail result1 = (from a in medc.ps_inspectiontask_detail
                                                where a.id == id
                                                select a).FirstOrDefault();

            bool result2 = true;

            try
            {
                result1.bugLevelName = buglevelname;
                result1.bugLevel = buglevel;
                result1.isBugLevel = 1;
                result1.lastUpdateTime = DateTime.Now;

                medc.SubmitChanges();
            }
            catch (Exception)
            {
                result2 = false;
            }
            #endregion
            return Json(result2);
        }
        public ActionResult TypeList1()
        {
            #region 缺陷类型列表
            var result = (from a in medc.ps_systemconfig
                          where a.configCode == "BUG_TYPE"
                          select new BugTypeList() { id = a.id, configValueName = a.configValueName, isStart = a.isStart, check = false }).ToList();

            for (int i = 0; i < result.Count; i++)
            {
                if (result[i].isStart == 1)
                {
                    result[i].check = true;
                }
            }

            List<BugTypeList> result2 = new List<BugTypeList>();

            for (int i = 0; i < 4; i++)
            {
                result2.Add(result[i]);
            }

            var List = new { a = result2, b = result.Count };

            string jsonstr = JsonConvert.SerializeObject(List);
            #endregion
            return Content(jsonstr);
        }
        public ActionResult TypeList2()
        {
            #region 缺陷类型列表(自定义分页功能)
            int pagenum = int.Parse(Request.Params["pagenum"]);

            var result = (from a in medc.ps_systemconfig
                          where a.configCode == "BUG_TYPE"
                          select new BugTypeList() { id = a.id, configValueName = a.configValueName, isStart = a.isStart, check = false }).ToList();

            for (int i = 0; i < result.Count; i++)
            {
                if (result[i].isStart == 1)
                {
                    result[i].check = true;
                }
            }

            int count = 0;

            List<BugTypeList> result2 = new List<BugTypeList>();

            if (pagenum == 1)
            {
                result2.Clear();
                for (int i = 0; i < 4; i++)
                {
                    result2.Add(result[i]);
                }
            }
            else
            {
                result2.Clear();
                for (int i = 4 * pagenum - 4; i < result.Count; i++)
                {
                    if (count < 4)
                    {
                        result2.Add(result[i]);
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            string jsonstr = JsonConvert.SerializeObject(result2);
            #endregion
            return Content(jsonstr);
        }
        public ActionResult onOroffBugType()
        {
            #region 启用/停用类型
            int id = int.Parse(Request.Params["id"]);

            bool state = true;

            string type = "";

            ps_systemconfig result = (from a in medc.ps_systemconfig
                                      where a.id == id
                                      select a).FirstOrDefault();

            try
            {
                if (result.isStart == 1)
                {
                    type = "停用";
                    result.isStart = 0;
                }
                else
                {
                    type = "启用";
                    result.isStart = 1;
                }
                medc.SubmitChanges();
            }
            catch (Exception)
            {
                state = false;
            }

            use u = new use() { type = type, succ = state };
            #endregion
            return Json(u);
        }
        public ActionResult addBugType()
        {
            #region 找出最大的configValueName
            int radio = int.Parse(Request.Params["radio"]);

            string BugName = Request.Params["BugName"];

            bool state = true;

            //查找出最大的configValueId
            SqlConnection con = new SqlConnection("Server=.;database=MyElectrCheck_DB;uid=sa;pwd=123");

            SqlCommand cmd = new SqlCommand("select max(configValueId) from ps_systemconfig", con);

            con.Open();

            int maxid = Convert.ToInt32(cmd.ExecuteScalar());

            con.Close();
            #endregion

            #region 基于最大configValueId添加一条新缺陷类型
            //创建一个新的ps_systemconfig对象添加并保存至数据库
            ps_systemconfig ps = new ps_systemconfig() { configCode = "BUG_TYPE", configName = "缺陷类别", configValueId = maxid + 1, configValueName = BugName, isStart = radio };

            try
            {
                medc.ps_systemconfig.InsertOnSubmit(ps);
                medc.SubmitChanges();
            }
            catch (Exception)
            {
                state = false;
            }
            #endregion
            return Json(state);
        }
        public ActionResult deleteBugType()
        {
            #region 根据前台id删除缺陷类型
            int did = int.Parse(Request.Params["id"]);

            bool state = true;

            var result1 = (from a in medc.ps_systemconfig
                           where a.id == did
                           select a).FirstOrDefault();

            try
            {
                medc.ps_systemconfig.DeleteOnSubmit(result1);

                medc.SubmitChanges();
            }
            catch (Exception)
            {
                state = false;
            }
            #endregion
            return Json(state);
        }
        public ActionResult updateBugType()
        {
            #region 获取修改前的数据
            int id = int.Parse(Request.Params["id"]);

            var result = from a in medc.ps_systemconfig
                         where a.id == id
                         select new updateBugType() { isStart = a.isStart.ToString(), configValueName = a.configValueName };

            string jsonstr = JsonConvert.SerializeObject(result);
            #endregion
            return Content(jsonstr);
        }
        public ActionResult updateType()
        {
            #region 修改缺陷类型
            bool state = true;

            int id = int.Parse(Request.Params["id"]);

            string Name = Request.Params["Name"];

            int isStart = int.Parse(Request.Params["isStart"]);

            var result = (from a in medc.ps_systemconfig
                          where a.id == id
                          select a).FirstOrDefault();

            try
            {
                result.configValueName = Name;
                result.isStart = isStart;
                medc.SubmitChanges();
            }
            catch (Exception)
            {
                state = false;
            }
            #endregion
            return Json(state);
        }
        public ActionResult DefectTableDataList()
        {
            //int pagenum = int.Parse(Request.Params["pagenum"]);
            #region 查出缺陷列表中所有状态为:(已完成)的数据
            //获取Session["DefectID"]中的ID
            var result1 = (from a in medc.ps_inspectiontask_detail
                           join b in medc.ps_inspectiontask_main on a.taskid equals b.id
                           where a.taskStatus == 4 && a.isBugLevel == 0&&a.isBug==1&&a.isSolve==0
                           select new TypeList() { id = a.id, bugLevel = a.bugLevel, inspectionTaskCode = b.inspectionTaskCode, lineCode = a.lineCode, poleCode = a.poleCode, bugTypeName = a.bugTypeName, intactRate = a.intactRate, bugDesc = a.bugDesc, discoverTime = a.discoverTime, discovererName = a.discovererName, SelectValueId = 0 }).ToList();
            #endregion

            #region 查出缺陷级别
            role[] result2 = (from a in medc.ps_systemconfig
                              where a.configCode == "BUG_LEVEL"
                              select new role() { label = a.configValueName, value = a.configValueId.ToString() }).ToArray();
            #endregion

            #region 查出缺陷类型
            role[] result4 = (from a in medc.ps_systemconfig
                              where a.configCode == "BUG_TYPE"
                              select new role() { label = a.configValueName, value = a.configValueId.ToString() }).ToArray();
            #endregion

            int pagecount = result1.Count();

            Session["DataList"] = result1;

            result1 = result1.Skip(0).Take(6).ToList();

            var result3 = new { inspectlist = result1, buglevellist = result2, pagecount = pagecount, Bugresult = result4 };

            string jsonstr = JsonConvert.SerializeObject(result3);

            return Content(jsonstr);
        }
        public ActionResult SelectByDataList()
        {

            string taskcode ="";
            string linecode = "";
            string polecode = "";
            string discovername = "";
            string bugtype = "";
            string buglevel = "";
            string ptime1 = "";
            string ptime2 = "";

            int pageindex = int.Parse(Request.Params["pageindex"]);
            int skip = (pageindex - 1) * 6;

            string sql = "select a.id,b.inspectionTaskCode,a.lineCode,a.poleCode,a.bugTypeName,a.intactRate,a.bugDesc,a.discoverTime,a.discovererName from ps_inspectiontask_detail as a inner join ps_inspectiontask_main as b on a.taskid = b.id where a.taskStatus = 4 and a.isbug=1 and a.isbuglevel=0 and a.issolve=0 ";

            if (Request.Params["taskcode"] != "")
            {
                taskcode = Request.Params["taskcode"];
                sql += " and b.inspectionTaskCode = " + "'" +taskcode + "'";
            }
            if (Request.Params["linecode"] != "")
            {
                linecode = Request.Params["linecode"];
                sql += " and b.linecode = " + "'" + linecode + "'";
            }

            if (Request.Params["polecode"] != "")
            {
                polecode = Request.Params["polecode"];
                sql += " and a.polecode = " + "'" + polecode + "'";
            }
            if (Request.Params["discovername"] != "")
            {
                discovername = Request.Params["discovername"];
                sql += " and a.discovererName = " + "'" + discovername + "'";
            }
            if (Request.Params["bugtype"] != "")
            {
                bugtype = Request.Params["bugtype"];
                sql += " and a.bugtype = " + taskcode;
            }
            if (Request.Params["buglevel"] != "")
            {
                buglevel = Request.Params["buglevel"];
                sql += " and a.buglevel = " + buglevel;
            }

            if (Request.Params["time1"] != "")
            {
                ptime1 = Request.Params["time1"];
                ptime2 = Request.Params["time2"];
                sql+=" and a.discovertime>='"+ptime1+"' and a.discovertime<='"+ptime2+"'";
            }

            SqlConnection con = new SqlConnection("server=.;database=MyElectrCheck_DB;Integrated Security=true;");

            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            #region 使用SqlCommand命令对象进行查询
          
            SqlCommand cmd = new SqlCommand(sql, con);

            SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

            List<TypeList> list = new List<TypeList>();

            while (dr.Read())
            {
                TypeList t1 = new TypeList();

                t1.id = Convert.ToInt32(dr["id"]);
                t1.inspectionTaskCode = dr["inspectionTaskCode"].ToString();
                t1.lineCode = dr["lineCode"].ToString();
                t1.poleCode = dr["poleCode"].ToString();
                t1.bugTypeName = dr["bugTypeName"].ToString();
                t1.intactRate = Convert.ToDouble(dr["intactRate"]);
                t1.bugDesc = dr["bugDesc"].ToString();
                t1.discoverTime = Convert.ToDateTime(dr["discoverTime"]);
                t1.discovererName = dr["discovererName"].ToString();
                t1.SelectValueId = 0;
                list.Add(t1);
            }
            dr.Close();
            con.Close();

            #endregion

           
            string jsonstr ="";

            if (list.Count() > 0)
            {
                (Session["DataList"] as List<TypeList>).Clear();

                Session["DataList"] = list;

                var result = new { DataList = list.Skip(skip).Take(6).ToList(), SelectState = true, count = list.Count };

                jsonstr = JsonConvert.SerializeObject(result);
            }

            return Content(jsonstr);
        }
        public ActionResult SelectBySelectDataPageNum()
        {
            int pagenum = int.Parse(Request.Params["pagenum"]);

            //获得Session中保存的string集合
            List<TypeList> result1 = Session["DataList"] as List<TypeList>;

            #region 根据查询后的数据进行分页显示数据
            int count = 0;

            List<TypeList> result4 = new List<TypeList>();

            if (pagenum == 1)
            {
                result4.Clear();
                for (int i = 0; i < 6; i++)
                {
                    result4.Add(result1[i]);
                }
            }
            else
            {
                result4.Clear();
                for (int i = 6 * pagenum - 6; i < result1.Count; i++)
                {
                    if (count < 6)
                    {
                        result4.Add(result1[i]);
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            #endregion
            string jsonstr = JsonConvert.SerializeObject(result4);

            return Content(jsonstr);
        }
        public ActionResult AddSelectBugLevel()
        {
            string[] str1 = (Request.Params["str"]).Split(',');//第一个:ID,第二个:缺陷等级

            bool State = true;

            if (State)
            {
                for (int i = 0; i < str1.Length; i++)
                {
                    string[] str2 = str1[i].Split('-');

                    int id = int.Parse(str2[0]);

                    var result = (from a in medc.ps_inspectiontask_detail
                                  where a.id == id
                                  select a).FirstOrDefault();

                    try
                    {
                        result.bugLevel = int.Parse(str2[1]);
                        result.isBugLevel = 1;
                        medc.SubmitChanges();
                    }
                    catch (Exception)
                    {
                        State = false;
                    }
                }
            }

            return Json(true);
        }
        public ActionResult BugLevelAff()
        {
            return View();
        }
    }
}