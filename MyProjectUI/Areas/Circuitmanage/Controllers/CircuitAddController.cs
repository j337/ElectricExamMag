using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyProjectUI.Models;
using Newtonsoft.Json;
namespace MyProjectUI.Areas.Circuitmanage.Controllers
{
    public class CircuitAddController : Controller
    {
        MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();
        // GET: Circuitmanage/CircuitAdd
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult getdata()
        {
            var result = from p in medc.ps_pole_info
                         select p;
            string str = JsonConvert.SerializeObject(result);
            return Content(str);
        }
        public ActionResult checkpole()
        {
            string pcode1 = Request.Params["pcode1"];
            string pcode2 = Request.Params["pcode2"];
            bool b = false;
            var result1 = (from p in medc.ps_pole_info
                           where p.poleCode == pcode1
                           select p).FirstOrDefault();
            var result2 = (from p in medc.ps_pole_info
                           where p.poleCode == pcode2
                           select p).FirstOrDefault();
            Session["pid1"] = result1.id;
            if (result1.id<=result2.id)
            {
                b = false;
            }
            else
            {
                b = true;
            }
            return Json(b);
        }
        public ActionResult checkpole2()
        {
            string pcode1 = Request.Params["pcode1"];
            string pcode2 = Request.Params["pcode2"];
            bool b = true;
            var result1 = (from p in medc.ps_pole_info
                           where p.poleCode == pcode1
                           select p).FirstOrDefault();
            var result2 = (from p in medc.ps_pole_info
                           where p.poleCode == pcode2
                           select p).FirstOrDefault();
            Session["pid2"] = result2.id;
            if (result1.id >= result2.id)
            {
                b = false;
            }
            else
            {
                b = true;
            }
            return Json(b);
        }
        public ActionResult poleid()
        {
            string pcode1 = Request.Params["pcode1"];
            var result = (from p in medc.ps_pole_info
                          where p.poleCode == pcode1
                          select p).FirstOrDefault();
            Session["pid"] = result.id;
            return Content("dan");
        }
        public ActionResult poleid2()
        {
            string pcode2 = Request.Params["pcode2"];
            var result = (from p in medc.ps_pole_info
                          where p.poleCode == pcode2
                          select p).FirstOrDefault();
            Session["pid2"] = result.id;
            return Content("dan");
        }

        public ActionResult ChecklineCode()
        {
            string linecode = Request.Params["linecode"];

            var line = (from l in medc.ps_line_info
                        where l.lineCode == linecode
                        select l).FirstOrDefault();

            bool b = true;
            if (line == null)
            {
                b = false;
            }

            return Json(b);
        }
        public ActionResult Addline()
        {
            bool b = false;
           
            string lineCode = Request.Params["lineCode"];
            string lineName = Request.Params["lineName"];
            
                int lineLength = int.Parse(Request.Params["lineLength"]);
            
               int backLength = int.Parse(Request.Params["backLength"]);
            
               DateTime productDate = Convert.ToDateTime(Request.Params["productDate"]);
            
              string voltageLevel = Request.Params["voltageLevel"];
            
               string common = Request.Params["common"];
           
               int towerBaseNum = int.Parse(Request.Params["towerBaseNum"]);
            
            string startPoleCode = Request.Params["startPoleCode"];
            string endPoleCode = Request.Params["endPoleCode"];
            int isStart = int.Parse(Request.Params["isStart"]);
            int status = int.Parse(Request.Params["status"]);
            var count1 = medc.ps_line_info.Count();
            int uid = int.Parse(Session["userid"].ToString());
            int pid = int.Parse(Session["pid"].ToString());
            int pid2 = int.Parse(Session["pid2"].ToString());
            var user = (from u in medc.ps_user
                        where u.id == uid
                        select u).FirstOrDefault();

            var sys = (from s in medc.ps_systemconfig
                       where s.configName == "运行状态" && s.configValueId == status
                       select s).FirstOrDefault();
            ps_line_info lines = new ps_line_info();
            
            ps_line_info line = new ps_line_info()
            {
                lineCode = lineCode,
                lineName = lineName,
                lineLength = lineLength,
                backLength = backLength,
                productDate = productDate,
                voltageLevel = voltageLevel,
                startPoleCode = startPoleCode,
                endPoleCode = endPoleCode,
                towerBaseNum = towerBaseNum,
                common = common,
                startPole = pid,
                createdBy =user.userCode,
                isStart = isStart,
                runningStatus = status,
                endPole=pid2,
                runningStatusName=sys.configValueName,
                creationTime =DateTime.Now,
                lastUpdateTime=DateTime.Now
            };
            medc.ps_line_info.InsertOnSubmit(line);
            medc.SubmitChanges();
            var count2 = medc.ps_line_info.Count();
            if (count2 > count1)
            {
                b = true;
            }
            return Json(b);
        }
    }
}