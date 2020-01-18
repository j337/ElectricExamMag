using System.Web.Mvc;

namespace MyProjectUI.Areas.XiTong
{
    public class XiTongAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "XiTong";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "XiTong_default",
                "XiTong/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}