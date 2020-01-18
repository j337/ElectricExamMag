using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyProjectUI.Controllers
{
    public class CommController : Controller
    {
        // GET: Comm
        public ActionResult ShareView()
        {
            return View();
        }
    }
}