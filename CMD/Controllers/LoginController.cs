using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Configuration;
using CMD.Models;
using CMD.SystemClass;
using TWCAlib;

namespace CMD.Controllers
{
    public class LoginController : Controller
    {
        SystemConfig Configer = new SystemConfig();
        OPLog OPLoger = new OPLog();
        ShareFunc SF = new ShareFunc();

        // GET: Login
        public ActionResult Index()
        {
           
            return View();
        }

        [HttpPost]
        public ActionResult Index(LoginInfo model)
        {
            //初始化系統參數
            Configer.Init();
           
            AD AD = new AD();
            VA VA = new VA();
            LoginProcessor LP = new LoginProcessor();
            bool UseCertLogin = model.UseCertLogin;
            string LDAPName = WebConfigurationManager.AppSettings["LDAPName"];
            string VAVerifyURL = WebConfigurationManager.AppSettings["VAVerifyURL"];
            string ConnStr = Configer.C_DBConnstring;
            Boolean ContinueLogin = true;

            //Log紀錄用
            string op_name = model.UserID;
            string op_action = "系統登入作業";
            DateTime op_stime = default(DateTime);
            DateTime op_etime = default(DateTime);
            int op_a_count = 1;
            int op_s_count = 0;
            int op_f_count = 0;
            string op_msg = string.Empty;
            bool op_result = true;

            string MailServer = Configer.MailServer;
	        int MailServerPort = Configer.MailServerPort;
	        string MailSender = Configer.MailSender;
	        List<string> MailReceiver = Configer.MailReceiver;
	        //string SendResult = string.Empty;

            //共用涵式用
            SF.ConnStr = ConnStr;
            SF.op_name = op_name;

            op_stime = DateTime.Now;
            if (ModelState.IsValid)
            {
                if (LDAPName == "" || VAVerifyURL == "")
                {
                    //缺少系統參數，需記錄錯誤
                    op_etime = DateTime.Now;
                    op_f_count=1;
                    op_msg = "登入失敗，錯誤訊息:[缺少系統參數LDAPName或AVerifyURL]";
                    op_result = false;
                    OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                    SF.log2DB(OPLoger, MailServer, MailServerPort, MailSender, MailReceiver);
                    ContinueLogin = false;
                }
                if (ContinueLogin)
                {
                    AD.UserName = model.UserID;
                    AD.Pwd = model.Pwd;
                    AD.validType = AD.ValidType.Domain;
                    AD.LDAPName = LDAPName;

                    VA.SignData = model.SignData;
                    VA.Plaintext = model.Plaintext;
                    VA.txnCode = "TxnCode";
                    VA.VAVerifyURL = VAVerifyURL;
                    VA.Tolerate = 120;

                    if (LP.DoLogin(UseCertLogin, AD, VA) == true)
                    {
                        //登入成功，需紀錄
                        op_etime = DateTime.Now;
                        op_s_count = 1;
                        op_msg = "登入成功";
                        OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                        SF.log2DB(OPLoger, MailServer, MailServerPort, MailSender, MailReceiver);
                        Session["UseCertLogin"] = UseCertLogin;
                        //Session["UseCertLogin"] = true;
                        Session["UserID"] = model.UserID;
                        //Session["UserID"] = "TAS191";
                        Session["UserRole"] = getUserRole(model.UserID);
                        return RedirectToAction("Index", "Home", new { s_class = "0" });
                    }
                    else
                    {
                        //string a=VA.ResultStr;
                      
                        //登入失敗，需記錄錯誤
                        op_etime = DateTime.Now;
                        op_f_count = 1;
                        if (UseCertLogin)
                        {
                            op_msg = "登入失敗，錯誤訊息:[AD或VA驗證失敗]";
                        }
                        else
                        {
                            op_msg = "登入失敗，錯誤訊息:[AD驗證失敗]";
                        }
                        op_result = false;
                        OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                        SF.log2DB(OPLoger, MailServer, MailServerPort, MailSender, MailReceiver);
                        TempData["LoginMsg"] = op_msg;
                        return RedirectToAction("Index", "Login");
                    }
                }
                else
                {
                    TempData["LoginMsg"] = "登入失敗，錯誤訊息:[系統登入參數遺失]";
                    return RedirectToAction("Index", "Login");
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        /// <summary>
        /// 取得使用者角色ID
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        private int getUserRole(string UserID)
        {
            int Role = -1;
            using (CMSEntities CMS = new CMSEntities())
            {
                Role = Convert.ToInt16(CMS.usp_getUserRole(UserID).First());
                if (Role <= 0)
                {
                    Role = Configer.PublicRoleID;
                }
            }
            return Role;
        }
    }

   
}