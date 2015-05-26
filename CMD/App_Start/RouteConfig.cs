using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace CMD
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
 
            routes.MapRoute(
                name: "MonitorPropertyRoute",
                url: "MonitorProperty/{action}/{name}/{sclass}/{sno}",
                defaults: new
                {
                    controller = "MonitorProperty",
                    action = "Index",
                    name = UrlParameter.Optional,
                    sclass = UrlParameter.Optional,
                    sno=UrlParameter.Optional
                }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
