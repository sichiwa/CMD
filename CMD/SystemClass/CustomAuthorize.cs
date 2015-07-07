using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace CMD.SystemClass
{
    public class CustomAuthorize : AuthorizeAttribute
    {        
        /// <summary>
        /// 功能代號
        /// </summary>
        public string f_id
        {
            get;
            set;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }
            //檢查是否有權限
            if (string.IsNullOrEmpty(f_id))
            {
                //如果功能代號是空值，代號就給控制項名稱
                f_id = filterContext.RouteData.Values["controller"] as string;
                //filterContext.Result = new HttpUnauthorizedResult();
            }
            if (AuthorizeCore(filterContext.HttpContext))
            {
                //驗証通過
                return;
            }
            else //if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // 驗証失敗, redirect to login page
                filterContext.Result = new HttpUnauthorizedResult();
                //redirect to login page
            }
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool result = false;
            if (httpContext.Session["UserID"] != null && httpContext.Session["UserRole"] != null)
            {
                //已登入，所以先檢查是否有Session沒有的話，就從cookie中取出來
                if (CheckLoginInfo(httpContext)==true)
                {
                    result = true; //檢查是否有權限使用該功能 CheckAuthorization(AppFunctionId));
                }
            }
            return result;
        }

        private bool CheckLoginInfo(HttpContextBase httpContext)
        {
            bool result = false;
            bool UseCertLogin = Convert.ToBoolean(httpContext.Session["UseCertLogin"].ToString());
            string UserID = httpContext.Session["UserID"].ToString();
            int UserRole =Convert.ToInt32( httpContext.Session["UserRole"].ToString());
            int FuncID=-1 ;

            if (f_id != "Home")
            {
                using (CMSEntities CMS = new CMSEntities())
                {
                    var func = CMS.Funcs
                                .Where(b => b.controller == f_id).First();

                    if (func != null)
                    {
                        FuncID = func.f_id;
                    }
                }

                if (string.IsNullOrEmpty(httpContext.Session["UserID"].ToString()) == false)
                {
                    
                    using (CMSEntities CMS = new CMSEntities())
                    {
                        int RoleMappingCount = CMS.RoleFuncMapping
                                    .Where(b => b.r_id == UserRole)
                                    .Where(b => b.f_id == FuncID).Count();

                        if (RoleMappingCount > 0)
                        {
                            result = true;
                        }
                    }
                }
            }
            else
            {
                result = true;
            }
            
            return result;
        }
    }
}