using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyProjectUI.Models;
using Newtonsoft.Json;
using System.Data.Linq;
using MyProjectUI.Areas.Circuitmanage.Models;
namespace MyProjectUI.Areas.Pole.Controllers
{
    public class TowerController : Controller
    {
        MyElectrCheck_DBDataContext md = new MyElectrCheck_DBDataContext();
       
        //添加杆塔
        public ActionResult AddPoleTower()
        {
            return View();
        }
        //杆塔页面
        public ActionResult PoleTower()
        {
            return View();
        }
        public ActionResult GetData()
        {
            var result = from p in md.ps_pole_info
                         select p;
            int pageindex = int.Parse(Request.Params["pageindex"]);

            var linelist = (from l in md.ps_line_info
                            select l).ToList();

            int skip = (pageindex - 1) * 5;
            DataSource ds = new DataSource();
            ds.polelist = result.Skip(skip).Take(5).ToList();
            ds.polecount = result.Count();
            ds.datalist = linelist;
            string str = JsonConvert.SerializeObject(ds);
            GC.Collect();
            return Content(str);
          
        }

        public ActionResult sel()
        {
            int lineName = -1;
            int isStart = -1;
            if (Request.Params["lineName"] != null)
            {
                if (Request.Params["lineName"] != "all")
                {
                    lineName = int.Parse(Request.Params["lineName"]);
                }
                
            }
            if (Request.Params["isStart"] != null)
            {
                if (Request.Params["isStart"] != "11")
                {
                    isStart = int.Parse(Request.Params["isStart"]);
                }
            }
            List<ps_pole_info> list = new List<ps_pole_info>();

            var result = from p in md.ps_pole_info
                         select p;
            if (lineName != -1 && isStart != -1)
            {
                foreach(var p in result)
                {
                    if (p.lineId==lineName && p.isStart == isStart)
                    {
                        list.Add(p);
                    }
                }
            }else if (lineName != -1)
            {
                foreach(var p in result)
                {
                    if (p.lineId == lineName)
                    {
                        list.Add(p);
                    }
                }
            }else if (isStart!=-1)
            {
                foreach(var p in result)
                {
                    if (p.isStart == isStart)
                    {
                        list.Add(p);
                    }
                }
            }
            else
            {
                foreach(var p in result)
                {
                    list.Add(p);
                }
            }
            int pageindex = int.Parse(Request.Params["pageindex"]);

            int skip = (pageindex - 1) * 5;
            var linelist = (from l in md.ps_line_info
                            select l).ToList();
            DataSource ds = new DataSource();
            ds.polelist = list.Skip(skip).Take(5).ToList();
            ds.polecount = list.Count();
            ds.datalist = linelist;
            string str = JsonConvert.SerializeObject(ds);
            GC.Collect();
            return Content(str);
        }
        //根据它的id进行删除
        public ActionResult DelData()
        {
            int pid = int.Parse(Request.Params["pid"]);
            var count1 = md.ps_pole_info.Count();
            var pi = (from a in md.ps_pole_info
                               where a.id == pid
                               select a).FirstOrDefault();
            bool b = false;
            md.ps_pole_info.DeleteOnSubmit(pi);
            md.SubmitChanges();
            var count2 = md.ps_pole_info.Count();
            if (count1 > count2)
            {
                b = true;
            }
            GC.Collect();
            return Json(b);
        }
        public ActionResult CheckpoleCode()
        {
            string poleCode = Request.Params["poleCode"];
            bool b = true;
            var pole = (from p in md.ps_pole_info
                        where p.poleCode == poleCode
                        select p).FirstOrDefault();
            if (pole == null)
            {
                b = false;
            }
            return Json(b);
        }
        public ActionResult poledata()
        {
            int pid = int.Parse(Request.Params["pid"].ToString());
            var pole = (from p in md.ps_pole_info
                        where p.id == pid
                        select p).FirstOrDefault();
            Session["pid"] = pid;
            string jsonstr = JsonConvert.SerializeObject(pole);
            GC.Collect();
            return Content(jsonstr);
        }
        public ActionResult update()
        {
            string poleCode = Request.Params["poleCode"];
            int isStart = int.Parse(Request.Params["isStart"].ToString());
            int userid = int.Parse(Session["userid"].ToString());
            int pid = int.Parse(Session["pid"].ToString());
            var user = (from u in md.ps_user
                        where u.id == userid
                        select u).FirstOrDefault();
            var pole = (from p in md.ps_pole_info
                        where p.id == pid
                        select p).FirstOrDefault();
            pole.poleCode = poleCode;
            pole.isStart = isStart;
            pole.createBy = user.userCode;
            pole.lastUpdateTime = DateTime.Now;
            bool b = true;
            try
            {
                md.SubmitChanges();
            }
            catch (Exception)
            {
                b = false;
            }
            GC.Collect();
            return Json(b);
        }
        public ActionResult upd()
        {
            int poleid = int.Parse(Request.Params["poleid"]);
            int isStart = int.Parse(Request.Params["isStart"]);
            int uid = int.Parse(Session["userid"].ToString());
            bool b = true;
            ps_pole_info result = (from p in md.ps_pole_info
                                   where p.id == poleid
                                   select p).FirstOrDefault();
            var user = (from u in md.ps_user
                        where u.id == uid
                        select u).FirstOrDefault();
            //修改数据 
            result.isStart = isStart;
            result.lastUpdateTime = DateTime.Now;
            result.createBy = user.userCode;
            try
            {
                //保存修改 
                md.SubmitChanges();
            }
            catch (Exception)
            {
                b = false;
            }
            GC.Collect();
            return Json(b);
        }
        public ActionResult add()
        {
            bool b = false;
            var count1 = md.ps_pole_info.Count();
            int lid = int.Parse(Request.Params["lid"].ToString());
            string poleCode = Request.Params["poleCode"];
            int isStart = int.Parse(Request.Params["isStart"].ToString());
            int uid = int.Parse(Session["userid"].ToString());
            var line = (from l in md.ps_line_info
                        where l.id == lid
                        select l).FirstOrDefault();
            var user = (from u in md.ps_user
                        where u.id == uid
                        select u).FirstOrDefault();
            ps_pole_info pole = new ps_pole_info()
            {
                poleCode = poleCode,
                lineId = line.id,
                lineCode = line.lineCode,
                lineName = line.lineName,
                creationTime = DateTime.Now,
                createBy = user.userCode,
                lastUpdateTime = DateTime.Now,
                isStart = isStart
            };
            md.ps_pole_info.InsertOnSubmit(pole);
            md.SubmitChanges();
            var count2 = md.ps_pole_info.Count();
            if (count2 > count1)
            {
                b = true;
            }
            GC.Collect();
            return Json (b);
        }
    }
}