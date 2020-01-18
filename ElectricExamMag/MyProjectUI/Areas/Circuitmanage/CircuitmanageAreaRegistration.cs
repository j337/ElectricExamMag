using System.Web.Mvc;

namespace MyProjectUI.Areas.Circuitmanage
{
    public class CircuitmanageAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Circuitmanage";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Circuitmanage_default",
                "Circuitmanage/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}