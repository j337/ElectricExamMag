using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyProjectUI.Models;
using MyProjectUI.Areas.XiTong.Models;
using Newtonsoft.Json;
namespace MyProjectUI.Areas.XiTong.Controllers
{
    public class UserController : Controller
    {
        //创建全局Linq to SQL类 
        MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();
        // GET: XiTong/User
        public ActionResult Index()
        {
            return View();
        }

        //获得数据 
        public ActionResult GetData()
        {
            int pageindex = int.Parse(Request.Params["pageindex"]);
            int skip = (pageindex - 1) * 6;

            var rolelist = (from u in medc.ps_user
                            join r in medc.ps_role
                            on u.roleId equals r.id
                            select new {id=u.id,usercode=u.userCode,username=u.userName, userpwd = u.userPassword, rolename = r.roleName,email=u.email,lastlogintime=u.lastLoginTime, overdate = u.entryDate, outdate = u.dimissionDate, status = u.userStatus }).Skip(skip).Take(6).ToList();

            List<Userinfo> userinfo = new List<Userinfo>();

            foreach(var v in rolelist)
            {
                Userinfo  user= new Userinfo()
                {
                    id = v.id,
                    usercode = v.usercode,
                    username = v.username,
                    userpwd=v.userpwd,
                    rolename = v.rolename,
                    email = v.email,
                    lastlogintime = v.lastlogintime,
                    overdate=v.overdate,
                    outdate=v.outdate,
                    userstatus = v.status
                };
                userinfo.Add(user);
            }

            //查询所有角色集合 
            var role = (from r in medc.ps_role
                        select r).ToList();

            DataSource ds = new DataSource();

            ds.rolelist = role;

            ds.userlist = userinfo;

            ds.usercount = medc.ps_user.Count();

            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }

        //定义一个查询的action
        public ActionResult SelectData()
        {
            string username = "";
            int status = -1;
            if (Request.Params["username"] != null)
            {
                username = Request.Params["username"];
            }
            if (Request.Params["status"] !=null)
            {
                if (Request.Params["status"].ToString() != "no")
                {
                    status = int.Parse(Request.Params["status"]);
                }

            }
            int pageindex = int.Parse(Request.Params["pageindex"]);

            int skip = (pageindex - 1) * 6;

            List<Userinfo> list = new List<Userinfo>();

            var userlist = (from u in medc.ps_user
                            join r in medc.ps_role
                            on u.roleId equals r.id
                            select new { id = u.id, usercode = u.userCode, username = u.userName,userpwd=u.userPassword, rolename = r.roleName, email = u.email, lastlogintime = u.lastLoginTime,overdate=u.entryDate,outdate=u.dimissionDate, status = u.userStatus }).ToList();

            List<Userinfo> result = new List<Userinfo>();

            foreach(var v in userlist)
            {
                Userinfo user = new Userinfo()
                {
                    id = v.id,
                    usercode = v.usercode,
                    username = v.username,
                    userpwd=v.userpwd,
                    rolename = v.rolename,
                    email = v.email,
                    lastlogintime = v.lastlogintime,
                    overdate=v.overdate,
                    outdate=v.outdate,
                    userstatus = v.status
                };
                result.Add(user);
            }

            if (status != -1 && username != "")
            {
                foreach (var v in result)
                {
                    if (v.username.Contains(username) && v.userstatus == status)
                    {
                        list.Add(v);

                    }
                }

            }
            else if (status != -1)
            {
                foreach (var v in result)
                {
                    if (v.userstatus == status)
                    {
                        list.Add(v);
                    }
                }
            }
            else if (username != "")
            {
                foreach (var v in result)
                {
                    if (v.username.Contains(username))
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

            ds.userlist = list.Skip(skip).Take(6).ToList();

            ds.usercount = list.Count();

            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }

        public ActionResult CheckInfo()
        {
            string usercode="";
            string username = "";

            bool b = false;

            if (Request.Params["userid"] != null)
            {
                usercode = Request.Params["userid"];

                var user = (from u in medc.ps_user
                            where u.userCode == usercode
                            select u).FirstOrDefault();
                if (user == null)
                {
                    b = true;
                }
            }
            if (Request.Params["username"] != null)
            {
                username = Request.Params["username"];
                var user = (from u in medc.ps_user
                            where u.userName == username
                            select u).FirstOrDefault();
                if (user == null)
                {
                    b = true;
                }
            }


            return Json(b);
        }
        //添加用户信息 
        public ActionResult AddUser()
        {
            string userid = Request.Params["userid"];
            string username = Request.Params["username"];
            string userpwd = Request.Params["userpwd"];
            string roleid = Request.Params["roleid"];
            DateTime? overdate=null;
            if (Request.Params["overdate"] != "")
            {
                overdate = Convert.ToDateTime(Request.Params["overdate"]);
            }
            DateTime? outdate=null;
            if (Request.Params["overdate"] != "")
            {
                overdate = Convert.ToDateTime(Request.Params["overdate"]);
            }

            int status = int.Parse(Request.Params["status"]);

            //获取session中的值 
            //获得session中保存的值 
            int id = int.Parse(Session["userid"].ToString());
            var pu = (from u in medc.ps_user
                        where u.id == id
                        select u).FirstOrDefault();

            ps_user user = new ps_user()
            {
                userCode = userid,
                userName = username,
                userPassword = userpwd,
                sex = null,
                age = null,
                entryDate = overdate,
                dimissionDate = outdate,
                phoneTel = null,
                email = null,
                creationTime = DateTime.Now,
                lastLoginTime = DateTime.Now,
                createdBy = pu.userCode,
                lastUpdateTime = DateTime.Now,
                userStatus = status,
                roleId = roleid

            };
            //添加到数据库 
            medc.ps_user.InsertOnSubmit(user);

            bool b = true;

            try
            {
                medc.SubmitChanges();

                ps_logs logs = new ps_logs()
                {
                    userCode = pu.userCode,
                    userName = pu.userName,
                    operateInfo = "添加用户信息",
                    operateDateTime = DateTime.Now
                };
                medc.ps_logs.InsertOnSubmit(logs);
                medc.SubmitChanges();
            }
            catch (Exception)
            {
                b = false;
            }
            return Json(b);
        }

        public ActionResult UpdCheckforId()
        {
            int id = int.Parse(Request.Params["uid"]);
            string userid = Request.Params["userid"];
            bool b = false;

            var user = (from r in medc.ps_user
                        where r.id == id
                        select r).FirstOrDefault();

            if (user.userCode == userid)
            {
                b = true;
            }
            else
            {
                var userinfo = (from r in medc.ps_user
                                where r.userCode == userid
                                select r).FirstOrDefault();
                if (userinfo == null)
                {
                    b = true;
                }
            }
            return Json(b);
        }

        public ActionResult UpdCheckforName()
        {
            int id = int.Parse(Request.Params["uid"]);
            string username = Request.Params["username"];
            bool b = false;

            var user = (from r in medc.ps_user
                        where r.id == id
                        select r).FirstOrDefault();

            if (user.userName == username)
            {
                b = true;
            }
            else
            {
                var userinfo = (from r in medc.ps_user
                                where r.userName == username
                                select r).FirstOrDefault();
                if (userinfo == null)
                {
                    b = true;
                }
            }
            return Json(b);
        }

        //修改用户信息的action
        public ActionResult UpdUser()
        {
            int id = int.Parse(Request.Params["uid"]);
            string userid = Request.Params["userid"];
            string username = Request.Params["username"];
            string userpwd = Request.Params["userpwd"];
            string roleid = Request.Params["roleid"];
            DateTime overdate;
            DateTime outdate;
           
            int status = int.Parse(Request.Params["status"]);

            var role = (from r in medc.ps_role
                        where r.roleName == roleid
                        select r).FirstOrDefault();

            var user = (from u in medc.ps_user
                        where u.id == id
                        select u).FirstOrDefault();

            //获得session中保存的值 
            int usersid = int.Parse(Session["userid"].ToString());
            var pu = (from u in medc.ps_user
                      where u.id == usersid
                      select u).FirstOrDefault();

            //修改数据 
            user.userCode = userid;
            user.userName = username;
            user.userPassword = userpwd;
            user.roleId = role.id;
            if (Request.Params["overdate"] != "")
            {
                overdate = Convert.ToDateTime(Request.Params["overdate"]);
                user.entryDate = overdate;
            }
            if (Request.Params["outdate"] != "")
            {
                outdate = Convert.ToDateTime(Request.Params["outdate"]);
                user.dimissionDate = outdate;
            }
            user.userStatus = status;

            bool b = true;

            try
            {
                //保存数据 
                medc.SubmitChanges();

                ps_logs logs = new ps_logs()
                {
                    userCode = pu.userCode,
                    userName = pu.userName,
                    operateInfo = "修改用户信息",
                    operateDateTime = DateTime.Now
                };
                medc.ps_logs.InsertOnSubmit(logs);
                medc.SubmitChanges();

            }
            catch (Exception)
            {
                b = false;
            }
            return Json(b);
        }

        public ActionResult ChangeStatus()
        {
            int userid = int.Parse(Request.Params["userid"]);
            int status = int.Parse(Request.Params["status"]);

            var user = (from u in medc.ps_user
                        where u.id == userid
                        select u).FirstOrDefault();

            //修改数据 
            user.userStatus = status;

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

        public ActionResult DelData()
        {
            int id = int.Parse(Request.Params["userid"]);
            var user = (from u in medc.ps_user
                        where u.id == id
                        select u).FirstOrDefault();

            //获得session中保存的值 
            int usersid = int.Parse(Session["userid"].ToString());
            var pu = (from u in medc.ps_user
                      where u.id == usersid
                      select u).FirstOrDefault();

            //删除对象 
            medc.ps_user.DeleteOnSubmit(user);

            bool b = true;
            try
            {
                medc.SubmitChanges();

                ps_logs logs = new ps_logs()
                {
                    userCode = pu.userCode,
                    userName = pu.userName,
                    operateInfo = "删除用户信息",
                    operateDateTime = DateTime.Now
                };
                medc.ps_logs.InsertOnSubmit(logs);
                medc.SubmitChanges();
            }
            catch (Exception)
            {

                b = false;
            }
            return Json(b);
        }

        public ActionResult GetLogData()
        {
            string id = Request.Params["usercode"];
            int pageindex = int.Parse(Request.Params["pageindex"]);

            int skip = (pageindex - 1) * 6;

            var loglist = (from l in medc.ps_logs
                           where l.userCode == id
                           select l).ToList();

            //计算总页数 
            int pagecount = (int)Math.Ceiling(loglist.Count() / 6.0);


            DataSource ds = new DataSource();
            ds.loglist = loglist.Skip(skip).Take(6).ToList();
            ds.logcount = loglist.Count();
            ds.logpagecount = pagecount;
            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }

        //带条件查询 
        public ActionResult GetLogDataByDate()
        {
            int pageindex = int.Parse(Request.Params["pageindex"]);
            string usercode = Request.Params["usercode"];
            DateTime startdate;
            DateTime enddate;
            List<ps_logs> loglist = null;
            if (Request.Params["startdate"] != null&&Request.Params["enddate"]!=null)
            {
                startdate = Convert.ToDateTime(Request.Params["startdate"]);
                enddate = Convert.ToDateTime(Request.Params["enddate"]);
                loglist = (from l in medc.ps_logs
                           where l.userCode == usercode && l.operateDateTime >= startdate && l.operateDateTime <= enddate
                           select l).ToList();

            }
            else
            {
                loglist = (from l in medc.ps_logs
                           where l.userCode == usercode 
                           select l).ToList();

            }
            int skip = (pageindex - 1) * 6;

            //计算总页数 
            int pagecount = (int)Math.Ceiling(loglist.Count() / 6.0);

            DataSource ds = new DataSource();

            ds.loglist = loglist.Skip(skip).Take(6).ToList();

            ds.logcount = loglist.Count();
            ds.logpagecount = pagecount;

            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }
    }
}