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
    public class RoleController : Controller
    {
        // GET: XiTong/Role
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetData()
        {
            int pageindex = int.Parse(Request.Params["pageindex"]);

            MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();

            int skip = (pageindex - 1) * 6;

            var rolelist = (from r in medc.ps_role
                            select r).ToList();

            DataSource ds = new DataSource();

            ds.rolelist = rolelist.Skip(skip).Take(6).ToList();

            ds.rolecount = rolelist.Count();

            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }
        public ActionResult SelectData()
        {
            string rolename="";
            int status = -1;
            if (Request.Params["rolename"] != null)
            {
                rolename = Request.Params["rolename"];
            }
            if (Request.Params["status"] != null)
            {
                if (Request.Params["status"].ToString() != "no")
                {
                    status = int.Parse(Request.Params["status"]);
                }

            }
            int pageindex = int.Parse(Request.Params["pageindex"]);

            int skip = (pageindex - 1) * 6;

            MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();

            List<ps_role> list = new List<ps_role>();

            var rolelist = (from r in medc.ps_role
                            select r).ToList();

            if (status != -1&&rolename!="")
            {
                foreach(var v in rolelist)
                {
                    if (v.roleName.Contains(rolename) && v.isStart == status)
                    {
                        list.Add(v);
                      
                    }
                }
                
            }else if (status != -1)
            {
                foreach (var v in rolelist)
                {
                    if ( v.isStart == status)
                    {
                        list.Add(v);
                    }
                }
            }else if(rolename!="")
            {
                foreach (var v in rolelist)
                {
                    if (v.roleName.Contains(rolename))
                    {
                        list.Add(v);
                    }
                }
            }
            else
            {
                foreach (var v in rolelist)
                {
                    
                        list.Add(v);
                    
                }
            }

            DataSource ds = new DataSource();

            ds.rolelist = list.Skip(skip).Take(6).ToList();

            ds.rolecount = list.Count();

            string jsonstr = JsonConvert.SerializeObject(ds);

            return Content(jsonstr);
        }

        public ActionResult CheckInfo()
        {
            string roleid;
            string rolename;
            bool b = false;
            MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();

            if (Request.Params["roleid"] != null)
            {
                roleid = Request.Params["roleid"];
                var role = (from r in medc.ps_role
                            where r.id == roleid
                            select r).FirstOrDefault();
                if (role == null)
                {
                    b = true;
                }

            }
            if (Request.Params["rolename"] != null)
            {
                rolename = Request.Params["rolename"];
                var role = (from r in medc.ps_role
                            where r.roleName == rolename
                            select r).FirstOrDefault();
                if (role == null)
                {
                    b = true;
                }
            }


            return Json(b);
        }
        public ActionResult UpdCheck()
        {
            string roleid = Request.Params["roleid"];
            string rolename = Request.Params["rolename"];
           

            MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();

            bool b = false;
          
            var role = (from r in medc.ps_role
                        where r.id == roleid
                        select r).FirstOrDefault();

            if (role.roleName == rolename)
            {
                b = true;
            }
            else
            {
                var roleinfo = (from r in medc.ps_role
                            where r.roleName == rolename
                            select r).FirstOrDefault();
                if (roleinfo == null)
                {
                    b = true;
                }
            }
            return Json(b);
        }

        //添加角色信息 
        public ActionResult AddRole()
        {
            string roleid = Request.Params["roleid"];
            string rolename = Request.Params["rolename"];
            int status = int.Parse(Request.Params["status"]);
           
            MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();

            //获得session中保存的值 
            int id = int.Parse(Session["userid"].ToString());
            var user = (from u in medc.ps_user
                        where u.id == id
                        select u).FirstOrDefault();

            //创建实体类对象 
            ps_role role = new ps_role()
            {
                id = roleid,
                roleName = rolename,
                creationTime = DateTime.Now,
                createdBy = user.userCode,
                lastUpdateTime = DateTime.Now,
                isStart = status
            };

            bool b = true;

            //添加到数据库 
            medc.ps_role.InsertOnSubmit(role);
            try
            {
                //保存数据 
                medc.SubmitChanges();

                ps_logs logs = new ps_logs()
                {
                    userCode = user.userCode,
                    userName = user.userName,
                    operateInfo = "添加角色信息",
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

        //修改角色信息 
        public ActionResult UpdRole()
        {
            string roleid = Request.Params["roleid"];
            string rolename = Request.Params["rolename"];
            int status = int.Parse(Request.Params["status"]);

            MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();

            //获得session中保存的值 
            int id = int.Parse(Session["userid"].ToString());
            var user = (from u in medc.ps_user
                        where u.id == id
                        select u).FirstOrDefault();

            var role = (from r in medc.ps_role
                        where r.id == roleid
                        select r).FirstOrDefault();

            role.roleName = rolename;
            role.lastUpdateTime = DateTime.Now;
            role.isStart = status;

            bool b = true;

            try
            {
                //保存数据 
                medc.SubmitChanges();

                ps_logs logs = new ps_logs()
                {
                    userCode = user.userCode,
                    userName = user.userName,
                    operateInfo = "修改角色信息",
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
            string id = Request.Params["roleid"];
            int status = int.Parse(Request.Params["status"]);

            MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();

            var role = (from r in medc.ps_role
                        where r.id == id
                        select r).FirstOrDefault();

            bool b = true;

            //修改数据 
            role.isStart = status;

            try
            {
                //保存数据 
                medc.SubmitChanges();
            }
            catch (Exception)
            {
                b = false;
            }
            return Json(b);
        }
        public ActionResult SelectHaveUser()
        {
            string rid = Request.Params["roleid"];

            MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();

            var user = (from u in medc.ps_user
                        where u.roleId == rid
                        select u).ToList();

            bool b = false;

            if (user.Count() > 0)
            {
                b = true;
            }
            return Json(b);
        }
        //删除数据 
        public ActionResult DelData()
        {
            string rid = Request.Params["roleid"];

            MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();

            //获得session中保存的值 
            int id = int.Parse(Session["userid"].ToString());
            var user = (from u in medc.ps_user
                        where u.id == id
                        select u).FirstOrDefault();

            var role = (from u in medc.ps_role
                        where u.id == rid
                        select u).FirstOrDefault();

            bool b = true;

            medc.ps_role.DeleteOnSubmit(role);

            try
            {
                medc.SubmitChanges();

                ps_logs logs = new ps_logs()
                {
                    userCode = user.userCode,
                    userName = user.userName,
                    operateInfo = "删除角色信息",
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

    }
}