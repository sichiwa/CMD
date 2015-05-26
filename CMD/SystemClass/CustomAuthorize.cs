using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMD.SystemClass
{
    public class CustomAuthorize : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");

            if (string.IsNullOrEmpty(httpContext.Session["UserID"].ToString()))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}