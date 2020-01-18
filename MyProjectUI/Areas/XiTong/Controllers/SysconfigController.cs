using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyProjectUI.Models;
using Newtonsoft.Json;
using MyProjectUI.Areas.XiTong.Models;

namespace MyProjectUI.Areas.XiTong.Controllers
{
    public class SysconfigController : Controller
    {
        MyElectrCheck_DBDataContext medb = new MyElectrCheck_DBDataContext();
        // GET: XiTong/Sysconfig
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult getdata()
        {
            int pageindex = int.Parse(Request.Params["pageindex"]);

            var result = from s in medb.ps_systemconfig
                         select s;
            int skip = (pageindex - 1) * 5;
            DataSource ds = new DataSource();
            ds.configlist = result.Skip(skip).Take(5).ToList();
            ds.configcount = result.Count();
            string str = JsonConvert.SerializeObject(ds);

            return Content(str);
        }
        public ActionResult wu()
        {
            int cid = int.Parse(Request.Params["id"]);
            Session["cid"] = cid;
            return Content("sah");
        }
        public ActionResult Add()
        {
            var count1 = medb.ps_systemconfig.Count();
            string configCode = Request.Params["configCode"];
            string configname = Request.Params["configname"];
            int valueid = int.Parse(Request.Params["valueid"]);
            string valuename = Request.Params["valuename"];
            int radio = int.Parse(Request.Params["radio"]);
            ps_systemconfig ps = new ps_systemconfig()
            {
                configCode = configCode,
                configName = configname,
                configValueId = valueid,
                configValueName = valuename,
                isStart = radio,
            };
            medb.ps_systemconfig.InsertOnSubmit(ps);
            medb.SubmitChanges();
            var count2 = medb.ps_systemconfig.Count();
            bool b = false;
            if (count1 < count2)
            {
                b = true;
            }
            return Json(b);
        }
        public ActionResult update()
        {
            string configcode = Request.Params["configcode"];
            string name = Request.Params["name"];
            int valueid = int.Parse(Request.Params["valueid"]);
            string valuename = Request.Params["valuename"];
            int radio = int.Parse(Request.Params["radio"]);
            int cid = int.Parse(Session["cid"].ToString());
            var result = (from c in medb.ps_systemconfig
                          where c.id == cid
                          select c).FirstOrDefault();
            result.configCode = configcode;
            result.configName = name;
            result.configValueId = valueid;
            result.configValueName = valuename;
            result.isStart = radio;
            bool b = true;
            try
            {
                medb.SubmitChanges();
            }
            catch (Exception)
            {
                b = false;
            }
            return Json(b);
        }
        public ActionResult del()
        {
            int id = int.Parse(Request.Params["conid"]);
            var count1 = medb.ps_systemconfig.Count();
            var result = (from c in medb.ps_systemconfig
                          where c.id == id
                          select c).FirstOrDefault();
            bool b = false;
            medb.ps_systemconfig.DeleteOnSubmit(result);
            medb.SubmitChanges();
            var count2 = medb.ps_systemconfig.Count();
            if (count1 > count2)
            {
                b = true;
            }
            return Json(b);
        }
        public ActionResult updisStart()
        {
            int isStart = int.Parse(Request.Params["isStart"]);
            int cid = int.Parse(Request.Params["cid"]);
            var result = (from s in medb.ps_systemconfig
                          where s.id == cid
                          select s).FirstOrDefault();
            result.isStart = isStart;
            bool b = true;
            try
            {
                medb.SubmitChanges();
            }
            catch (Exception)
            {
                b = false;
            }
            return Json(b);
        }
    }
}