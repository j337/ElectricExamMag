using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyProjectUI.Models;
using MyProjectUI.Areas.XiTong.Models;
namespace MyProjectUI.Areas.XiTong.Controllers
{
    public class DemoController : Controller
    {
        MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();
        // GET: XiTong/Demo
        public ActionResult Index()
        {
            return View();
        }
       public ActionResult Login()
        {
            return View();
        }

        //验证登录 
        public ActionResult CheckLogin()
        {
            //获得用户名和密码 
            string userid = Request.Params["userid"];
            string userpwd = Request.Params["userpwd"];

            MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();
            var result = (from u in medc.ps_user
                          where u.userCode == userid && u.userPassword == userpwd
                          select u).FirstOrDefault();

            bool b = false;
            if (result != null)
            {
                b = true;
                Session["userid"] = result.id;
                Session["userCode"] =result.userCode;
                Session["roleid"] =result.roleId;

                //修改数据 
                result.lastLoginTime = DateTime.Now;
                result.lastUpdateTime = DateTime.Now;

                try
                {
                    medc.SubmitChanges();
                }
                catch (Exception)
                {
                    b = false;
                }

                if (b)
                {
                    //添加logo日志 
                    ps_logs logs = new ps_logs()
                    {
                        userCode = result.userCode,
                        userName = result.userName,
                        operateInfo = "登录系统",
                        operateDateTime = DateTime.Now
                    };
                    medc.ps_logs.InsertOnSubmit(logs);
                    //保存数据 
                    medc.SubmitChanges();

                }

            }
            return Json(b);
        }

        public void TreeData(int? parentid, MyMenus mm)
        {
            List<ps_function> list = GetMenusData(parentid);

            foreach (var v in list)
            {
                var fun = (from f in medc.ps_function
                           where f.id == v.parentId
                           select f).FirstOrDefault();
                MyMenus mymenus = null;
                if (fun != null)
                {
                    mymenus = new MyMenus()
                    {
                        id = v.id,
                        funcode = v.functionCode,
                        funname = v.functionName,
                        funurl = v.funcUrl,
                        parentid = v.parentId,
                        parentname = fun.functionName,
                        isStart = v.isStart
                    };
                }
                else
                {
                    mymenus = new MyMenus()
                    {
                        id = v.id,
                        funcode = v.functionCode,
                        funname = v.functionName,
                        funurl = v.funcUrl,
                        parentid = v.parentId,
                        parentname = null,
                        isStart = v.isStart
                    };
                }

                mm.MMlist.Add(mymenus);

                //递归调用 
                TreeData(v.id, mymenus);

            }
        }

        //获得menus数据的方法 
        public List<ps_function> GetMenusData(int? parentid)
        {

            //获得登录的userid
            int id = int.Parse(Session["userid"].ToString());

            var user = (from a in medc.ps_user
                        where a.id == id
                        select a).FirstOrDefault();

            //获得菜单信息 
            var funs = (from a in medc.ps_function
                        join b in medc.ps_role_premission
                        on a.functionCode equals b.functionCode
                        where a.parentId==parentid&& b.roleId == user.roleId
                        select a).ToList();

            return funs;
        }

        //构建树形菜单的action
        public ActionResult MyFunction()
        {
            int userid = int.Parse(Session["userid"].ToString());

            var user = (from u in medc.ps_user
                        where u.id == userid
                        select u).FirstOrDefault();
            //查询所属角色 
            var role = (from r in medc.ps_role
                        where r.id == user.roleId
                        select r).FirstOrDefault();

            DataSource ds = new DataSource();

            ds.usercode = user.userCode;
            ds.rolename = role.roleName;

            MyMenus mm = new MyMenus();

            TreeData(0, mm);

            ds.mm = mm;

            ds.userinfo = user;

            return Json(ds,JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdPwd()
        {
            string pwd = Request.Params["pwd"];

            int userid = int.Parse(Session["userid"].ToString());

            var user = (from u in medc.ps_user
                        where u.id == userid
                        select u).FirstOrDefault();

            //修改数据 
            user.userPassword = pwd;
            user.lastUpdateTime = DateTime.Now;

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

       public ActionResult ExitLogin()
        {
            var user = (from u in medc.ps_user
                        where u.id == int.Parse(Session["userid"].ToString())
                        select u).FirstOrDefault();

            //添加数据 
            ps_logs logs = new ps_logs()
            {
                userCode = user.userCode,
                userName = user.userName,
                operateDateTime = DateTime.Now,
                operateInfo = "退出系统"
            };

            bool b = true;

            try
            {
                medc.SubmitChanges();
            }
            catch (Exception)
            {
                b = false;
            }

            return Json(b,JsonRequestBehavior.AllowGet);
        }



    }

    
}