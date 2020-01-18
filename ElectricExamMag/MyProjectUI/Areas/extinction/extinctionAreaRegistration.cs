using System.Web.Mvc;

namespace MyProjectUI.Areas.extinction
{
    public class extinctionAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "extinction";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "extinction_default",
                "extinction/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}