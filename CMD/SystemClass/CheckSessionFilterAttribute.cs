using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Mvc;
using System.Web.Routing;

namespace CMD.SystemClass
{
    public class CheckSessionFilterAttribute : System.Web.Mvc.ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext httpcontext = HttpContext.Current;
            // 確認目前要求的HttpSessionState
            if (httpcontext.Session != null)
            {
                //確認Session是否為新建立
                if (httpcontext.Session.IsNewSession)
                {
                    //確認是否已存在cookies
                    String sessioncookie = httpcontext.Request.Headers["Cookie"];
                    if ((sessioncookie != null) && (sessioncookie.IndexOf("ASP.NET_SessionId") >= 0))
                    {
                        Logon(filterContext);
                    }
                }
            }
            base.OnActionExecuting(filterContext);
        }

        private void Logon(ActionExecutingContext filterContext)
        {
            RouteValueDictionary dictionary = new RouteValueDictionary
                (new
                {
                    controller = "Login",
                    action = "Index",
                    returnUrl = filterContext.HttpContext.Request.RawUrl
                });
            filterContext.Result = new RedirectToRouteResult(dictionary);
        }
    }
}