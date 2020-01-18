using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyProjectUI.Models;
using MyProjectUI.Areas.XiTong.Models;
namespace MyProjectUI.Areas.XiTong.Controllers
{
    public class PremissController : Controller
    {
        MyElectrCheck_DBDataContext medc = new MyElectrCheck_DBDataContext();
        // GET: XiTong/Premiss
        public ActionResult Index()
        {
            return View();
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
                        isStart=v.isStart
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
         
            var result = (from m in medc.ps_function
                          where m.parentId==parentid
                          select m).ToList();

            return result;
        }

        public ActionResult GetMenuData()
        {
            DataSource ds = new DataSource();

            MyMenus mm = new MyMenus();

            TreeData(1, mm);

            ds.mm = mm;

            //查询角色表数据 
            var rolelist = (from r in medc.ps_role
                            select r).ToList();

            ds.rolelist = rolelist;

            return Json(ds);
        }

        //获取详细信息 的action
        public ActionResult GetMenuDetails()
        {
            int id = int.Parse(Request.Params["funid"]);

            var parentfun = (from p in medc.ps_function
                             join p2 in medc.ps_function
                             on p.parentId equals p2.id
                             where p.id==id
                             select new {funcode=p.functionCode,funname=p.functionName,funurl=p.funcUrl,status=p.isStart,parentname=p2.functionName }).FirstOrDefault();

           
            return Json(parentfun);
        }

        public ActionResult GetRoleFun()
        {
            string roleid = Request.Params["roleid"];

            var result = (from r in medc.ps_role
                          join rp in medc.ps_role_premission
                          on r.id equals rp.roleId
                          join p in medc.ps_function
                          on rp.functionCode equals p.functionCode
                          where rp.roleId == roleid
                          select new { funcode = rp.functionCode }).ToList();


            return Json(result);
        }

        public ActionResult Checkfuncode()
        {
            string funcode = Request.Params["funcode"];
            var result = (from r in medc.ps_function
                          where r.functionCode == funcode
                          select r).FirstOrDefault();

            bool b = true;

            if (result != null)
            {
                b = false;
            }
            return Json(b);
        }

        public ActionResult AddMenusinfo()
        {
            string funcode = Request.Params["funcode"];
            string funname = Request.Params["funname"];
            string funurl = Request.Params["funurl"];
            int status = int.Parse(Request.Params["radio"]);
            int parentid = int.Parse(Request.Params["parentid"]);
            //获得session中的值 
            int userid = int.Parse(Session["userid"].ToString());
            var user = (from u in medc.ps_user
                        where u.id == userid
                        select u).FirstOrDefault();

            //创建function对象 
            ps_function fun = new ps_function()
            {
                functionCode = funcode,
                functionName = funname,
                creationTime = DateTime.Now,
                createBy = user.userCode,
                lastUpdateTime = DateTime.Now,
                funcUrl = funurl,
                isStart = status,
                parentId = parentid
            };

            //添加到数据库 
            medc.ps_function.InsertOnSubmit(fun);

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

        //修改菜单信息 
        public ActionResult UpdMenusinfo()
        {
            int id = int.Parse(Request.Params["id"]);
            string name = Request.Params["funname"];
            string funurl = Request.Params["funurl"];
            int status = int.Parse(Request.Params["status"]);

            var fun = (from f in medc.ps_function
                       where f.id == id
                       select f).FirstOrDefault();

            fun.functionName = name;
            fun.funcUrl = funurl;
            fun.isStart = status;

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

        //修改角色权限 
        public ActionResult UpdRolePremiss()
        {
            string roleid = Request.Params["roleid"];
            string funcode = Request.Params["fcodes"];

            var funlist = (from p in medc.ps_role_premission
                           where p.roleId == roleid
                           select p).ToList();


            int userid = int.Parse(Session["userid"].ToString());
            var user = (from u in medc.ps_user
                        where u.id == userid
                        select u).FirstOrDefault();

            bool b = true;

            if (funlist.Count() > 0)
            {
               
                        medc.ps_role_premission.DeleteAllOnSubmit(funlist);
                        try
                        {
                            medc.SubmitChanges();
                        }
                        catch (Exception)
                        {

                            b = false;
                        }
                
            }

            if (b)
            {
                var psfun = (from p in medc.ps_function
                             where p.functionName == "企业人力资源系统"
                             select p).FirstOrDefault();

                ps_role_premission rolepremiss = new ps_role_premission()
                {
                    roleId = roleid,
                    functionCode = psfun.functionCode,
                    creationTime = DateTime.Now,
                    createdBy = user.userCode,
                    lastUpdateTime = DateTime.Now,
                    isStart = 1
                };
                medc.ps_role_premission.InsertOnSubmit(rolepremiss);

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
                    if (funcode.IndexOf(',') > 0)
                    {
                        //分割funcode
                        string[] codelist = funcode.Split(',');

                        foreach (var c in codelist)
                        {
                            ps_role_premission rolepre = new ps_role_premission()
                            {
                                roleId = roleid,
                                functionCode = c,
                                creationTime = DateTime.Now,
                                createdBy = user.userCode,
                                lastUpdateTime = DateTime.Now,
                                isStart = 1
                            };
                            medc.ps_role_premission.InsertOnSubmit(rolepre);
                            try
                            {
                                medc.SubmitChanges();
                            }
                            catch (Exception)
                            {

                                b = false;
                            }
                        }
                    }
                    else
                    {
                        ps_role_premission rolepre = new ps_role_premission()
                        {
                            roleId = roleid,
                            functionCode = funcode,
                            creationTime = DateTime.Now,
                            createdBy = user.userCode,
                            lastUpdateTime = DateTime.Now,
                            isStart = 1
                        };
                        medc.ps_role_premission.InsertOnSubmit(rolepre);
                        try
                        {
                            medc.SubmitChanges();
                        }
                        catch (Exception)
                        {

                            b = false;
                        }
                    }
              
                }
            }

            return Json(b);
        }

        public ActionResult ChangeStatus()
        {
            int id = int.Parse(Request.Params["funid"]);
            int status = int.Parse(Request.Params["status"]);

            var premiss = (from p in medc.ps_function
                           where p.id == id
                           select p).FirstOrDefault();

            bool b = true;

            premiss.isStart = status;

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
                var menulist = (from m in medc.ps_function
                                where m.parentId == id
                                select m).ToList();



                if (menulist.Count() > 0)
                {
                    foreach (var v in menulist)
                    {
                        var fun = (from f in medc.ps_function
                                   where f.id == v.id
                                   select f).FirstOrDefault();

                        //修改数据 
                        fun.isStart = status;
                        try
                        {
                            medc.SubmitChanges();
                        }
                        catch (Exception)
                        {
                            b = false;
                        }
                    }
                }
            }
            return Json(b);
        }

        public ActionResult DelFun()
        {
            int funid = int.Parse(Request.Params["funid"]);
            var premiss = (from p in medc.ps_function
                           where p.id == funid
                           select p).FirstOrDefault();
            bool b = true;

            medc.ps_function.DeleteOnSubmit(premiss);

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
                var menulist = (from m in medc.ps_function
                                where m.parentId == funid
                                select m).ToList();



                if (menulist.Count() > 0)
                {
                    foreach (var v in menulist)
                    {
                        var fun = (from f in medc.ps_function
                                   where f.id == v.id
                                   select f).FirstOrDefault();

                        medc.ps_function.DeleteOnSubmit(fun);
                        try
                        {
                            medc.SubmitChanges();
                        }
                        catch (Exception)
                        {
                            b = false;
                        }
                    }
                }
            }
            return Json(b);
        }
    }
}