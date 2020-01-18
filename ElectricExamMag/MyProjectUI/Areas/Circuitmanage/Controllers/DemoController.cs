using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyProjectUI.Models;
using Newtonsoft.Json;
using System.Data.Linq;

namespace MyProjectUI.Areas.Circuitmanage.Controllers
{
    public class DemoController : Controller
    {
        // GET: XiTong/Demo
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult TestView()
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult MyLogin()
        {
            bool b = false;
            string u2_input = Request.Params["u2_input"];
            string u3_input = Request.Params["u3_input"];
            MyElectrCheck_DBDataContext mdc = new MyElectrCheck_DBDataContext();
            var result = (from u in mdc.ps_user
                          where u.userName == u2_input && u.userPassword == u3_input
                          select u).FirstOrDefault();
            if (result!=null)
            {
                Session["userid"] = result.id;
                b = true;
            }

            return Json(b);
        }
    }
}