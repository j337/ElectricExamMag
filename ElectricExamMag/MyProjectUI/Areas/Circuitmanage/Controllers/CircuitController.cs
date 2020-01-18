using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyProjectUI.Models;
using System.Data.Linq;
using Newtonsoft.Json;
using MyProjectUI.Areas.Circuitmanage.Models;
namespace MyProjectUI.Areas.Circuitmanage.Controllers
{
    public class CircuitController : Controller
    {
        // GET: Circuitmanage/Circuit
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GetData()
        {
            MyElectrCheck_DBDataContext mdc = new MyElectrCheck_DBDataContext();
            var result = from l in mdc.ps_line_info
                         select l;
            int pageindex = int.Parse(Request.Params["pageindex"]);

            int skip = (pageindex - 1) * 5;
            DataSource ds = new DataSource();
            ds.datalist = result.Skip(skip).Take(5).ToList();
            ds.linecount = result.Count();
            string str = JsonConvert.SerializeObject(ds);
            return Content(str); ;
        }
        public ActionResult SaveSession()
        {
            int lid = int.Parse(Request.Params["lid"]);
            Session["lid"] = lid;
            return Content("xs");
        }
        public ActionResult Sel()
        {
            string lineCode = "";
            int status = -1;
            if (Request.Params["lineCode"] != null)
            {
                lineCode = Request.Params["lineCode"];
            }
            if (Request.Params["status"] != null)
            {
                if (Request.Params["status"] != "no")
                {
                    status = int.Parse(Request.Params["status"]);
                }
            }

            MyElectrCheck_DBDataContext mdc = new MyElectrCheck_DBDataContext();

            int pageindex = int.Parse(Request.Params["pageindex"]);

            int skip = (pageindex - 1) * 5;
            List<ps_line_info> list = new List<ps_line_info>();
            var result = (from l in mdc.ps_line_info
                          select l).ToList();
            if (status != -1 && lineCode != "")
            {
                foreach (var v in result)
                {
                    if (v.lineCode.Contains(lineCode) && v.runningStatus == status)
                    {
                        list.Add(v);
                    }
                }
            }
            else if (status != -1)
            {
                foreach (var v in result)
                {
                    if (v.runningStatus == status)
                    {
                        list.Add(v);
                    }
                }
            }
            else if (lineCode != "")
            {
                foreach (var v in result)
                {
                    if (v.lineCode.Contains(lineCode))
                    {
                        list.Add(v);
                    }
                }
            }
            else
            {
                foreach (var v in result)
                {
                   
                        list.Add(v);
                   
                }
            }
            DataSource ds = new DataSource();
            ds.datalist = list.Skip(skip).Take(5).ToList();
            ds.linecount = list.Count();
            string str = JsonConvert.SerializeObject(ds);
            return Content(str); ;
        }
        public ActionResult upd1()
        {
            int lineid = int.Parse(Request.Params["lineid"]);
            int isStart = int.Parse(Request.Params["isStart"]);
            int uid = int.Parse(Session["userid"].ToString());
            bool b = true;
            MyElectrCheck_DBDataContext mdc = new MyElectrCheck_DBDataContext();
            ps_line_info result = (from l in mdc.ps_line_info
                                   where l.id == lineid
                                   select l).FirstOrDefault();
            var user = (from u in mdc.ps_user
                        where u.id == uid
                        select u).FirstOrDefault();
            //修改数据 
            result.isStart = isStart;
            result.lastUpdateTime = DateTime.Now;
            result.createdBy = user.userCode;
            try
            {
                //保存修改 
                mdc.SubmitChanges();
            }
            catch (Exception)
            {
                b = false;
            }

            return Json(b);
        }
        public ActionResult Del()
        {
            MyElectrCheck_DBDataContext mdc = new MyElectrCheck_DBDataContext();
            int id = int.Parse(Request.Params["id"]);
            bool b = false;
            var count1 = mdc.ps_line_info.Count();
            var result = (from l in mdc.ps_line_info
                          where l.id == id
                          select l).FirstOrDefault();
            mdc.ps_line_info.DeleteOnSubmit(result);
            mdc.SubmitChanges();
            var count2 = mdc.ps_line_info.Count();
            if (count1 > count2)
            {
                b = true;
            }
            return Json(b);
        }
    }


}