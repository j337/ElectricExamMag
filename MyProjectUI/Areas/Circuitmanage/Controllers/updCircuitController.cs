using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyProjectUI.Models;
using Newtonsoft.Json;


namespace MyProjectUI.Areas.Circuitmanage.Controllers
{
    public class updCircuitController : Controller
    {
        MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();
        // GET: Circuitmanage/updCircuit
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult getdata()
        {
            int sid = int.Parse(Session["lid"].ToString());
           
            var result = (from s in medc.ps_line_info
                        where s.id == sid
                          select s).FirstOrDefault();
            string jsonstr = JsonConvert.SerializeObject(result);
            return Content(jsonstr);
        }
        public ActionResult getdata2()
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
            Session["poid1"] = result1.id;
            if (result1.id <= result2.id)
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
            Session["poid2"] = result2.id;
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
        public ActionResult update()
        {
            
              int lineLength = int.Parse(Request.Params["lineLength"]);

            string lineCode = Request.Params["lineCode"];
            string lineName = Request.Params["lineName"];
            int backLength = int.Parse(Request.Params["backLength"]);

            string voltageLevel = Request.Params["voltageLevel"];
            
               string common = Request.Params["common"];
            
            
               int towerBaseNum = int.Parse(Request.Params["towerBaseNum"]);
            
            DateTime productDate = Convert.ToDateTime(Request.Params["productDate"]);

            string startPoleCode = Request.Params["startPoleCode"];
            string endPoleCode = Request.Params["endPoleCode"];
            int isStart = int.Parse(Request.Params["isStart"]);
            int status = int.Parse(Request.Params["runningStatus"]);
            var count1 = medc.ps_line_info.Count();
            int uid = int.Parse(Session["userid"].ToString());
            int sid = int.Parse(Session["lid"].ToString());

            var pole1 = (from p in medc.ps_pole_info
                         where p.poleCode == startPoleCode
                         select p).FirstOrDefault();
            var pole2 = (from p in medc.ps_pole_info
                         where p.poleCode == endPoleCode
                         select p).FirstOrDefault();
            var result = (from l in medc.ps_line_info
                          where l.id == sid
                          select l).FirstOrDefault();
            var user = (from u in medc.ps_user
                        where u.id == uid
                        select u).FirstOrDefault();
            var statu = (from s in medc.ps_systemconfig
                          where s.configName == "运行状态" && s.configValueId == status
                          select s).FirstOrDefault();
            result.lineCode = lineCode;
            result.lineName = lineName;
            result.lineLength = lineLength;
            result.backLength = backLength;
            result.productDate = productDate;
            result.voltageLevel = voltageLevel;
            result.startPoleCode = startPoleCode;
            result.endPoleCode = endPoleCode;
            result.towerBaseNum = towerBaseNum;
            result.common = common;
            result.createdBy = user.userCode;
            result.isStart = isStart;
            result.startPole = pole1.id;
            result.endPole = pole2.id;
            result.runningStatus = status;
            result.runningStatusName = statu.configValueName;
            result.lastUpdateTime = DateTime.Now;
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
    }
}