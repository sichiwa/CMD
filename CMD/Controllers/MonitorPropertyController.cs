using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using CMD.Models;
using CMD.SystemClass;
using CMD.ViewModels;
using TWCAlib;
using PagedList;

namespace CMD.Controllers
{
    [CustomAuthorize]
    public class MonitorPropertyController : Controller
    {
        SystemConfig Configer = new SystemConfig();
        OPLog OPLoger = new OPLog();
        ShareFunc SF = new ShareFunc();
        String log_Info = "Info";
        String log_Err = "Err";
        String op_name = "顯示中控台";

        // GET: MonitorProperty
        public ActionResult Index(string name, string sclass)
        {
            //初始化系統參數
            Configer.Init();

            //共用涵式用
            SF.ConnStr = Configer.C_DBConnstring;
            SF.op_name = op_name;

            //初始化回傳物件
            MonitorPropertyDataViewModel MV = getMonitorPropertyData(name, sclass);
            if (MV != null)
            {
                return View(MV);
            }
            else
            {
                if (name == "EC+ CA")
                {
                    name = "EC  CA";
                }

                return RedirectToAction("Index", "MonitorProperty", new { name = name, sclass = "0" });
            }
        }

        [HttpPost]
        public ActionResult Index(QueryMonitorPropertyData QMD)
        {
            //初始化系統參數
            Configer.Init();

            //共用涵式用
            SF.ConnStr = Configer.C_DBConnstring;
            SF.op_name = op_name;

            //初始化回傳物件
            MonitorPropertyDataViewModel MV = getMonitorPropertyData(QMD.nowClass, QMD.nowType, QMD.nowSys,QMD.sno, QMD.nowStatus, QMD.nowUser);

            using (CMSEntities CMS = new CMSEntities())
            {
                string nowClass = "";
                string nowType = "";
                string nowSys = "";
                string SysName = "";

                var Sys = CMS.v_SysList
                                      .Where(b => b.t_id == QMD.nowType)
                                      .Where(b => b.id == QMD.nowSys).First();

                nowClass = QMD.nowClass;
                nowType = QMD.nowType;
                nowSys = QMD.nowSys;

                if (Sys.value == "EC+ CA")
                {
                    SysName = "EC  CA";
                }

                if (MV != null)
                {
                    if (MV.MonitorData != null)
                    {
                        if (MV.MonitorData.Count() <= 0)
                        {
                            return RedirectToAction("Index", "MonitorProperty", new { name = SysName, sclass = QMD.nowClass });
                        }
                        else
                        {
                            return View(MV);
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index", "MonitorProperty", new { name = SysName, sclass = QMD.nowClass });
                    }
                }
                else
                {
                    return RedirectToAction("Index", "MonitorProperty", new { name = SysName, sclass = QMD.nowClass });
                }
            }
        }

        public ActionResult Create()
        {
            CreateMonitorPropertyDataViewModel MV = new CreateMonitorPropertyDataViewModel();
            MV.s_class = "-1";
            MV.s_type = "-1";
            MV.s_sys = "-1";
            MV.MailServerProfile = "1";
            MV.TextMessageProfile = "1";
            MV.w_notify = "1";
            MV.e_notify = "1";
            MV.f_notify = "1";
            //取得環境清單
            MV.ClassList = SF.getClassList("-1");
            //取得分類清單
            MV.TypeList = SF.getTypeList("-1");
            //取得系統別清單
            MV.SysList = SF.getSysList("-1", "-1");
            //取得郵件主機設定清單
            MV.MailServerProfileList = SF.getMailServerProfileList(-1);
            //取得簡訊主機設定清單
            MV.TextMessageProfileList = SF.getTextMessageProfileList(-1);
            //取得Warn通知群組清單
            MV.WarnNotifyGroupList = SF.getNotifyGroupList(-1);
            //取得Error通知群組清單
            MV.ErrorNotifyGroupList = SF.getNotifyGroupList(-1);
            //取得Fatal通知群組清單
            MV.FatalNotifyGroupList = SF.getNotifyGroupList(-1);

            return View(MV);
        }

        [HttpPost]
        public ActionResult Create(TmpMonitorProperty TMP)
        {
            //初始化系統參數
            Configer.Init();

            //共用涵式用
            SF.ConnStr = Configer.C_DBConnstring;
            SF.op_name = op_name;

            string op_action = null;
            DateTime op_stime = default(DateTime);
            DateTime op_etime = default(DateTime);
            int op_a_count = 0;
            int op_s_count = 0;
            int op_f_count = 0;
            string op_msg = string.Empty;
            bool op_result = false;

            string MailServer = Configer.MailServer;
            int MailServerPort = Configer.MailServerPort;
            string MailSender = Configer.MailSender;
            List<string> MailReceiver = Configer.MailReceiver;

            op_action = "監控項目新增作業";

            try
            {
                op_stime = DateTime.Now;
                if (CheckInput4Create(TMP) == true)
                {
                    TMP.create_account = Session["UserID"].ToString();
                    TMP.create_time = DateTime.Now;
                    TMP.update_account = Session["UserID"].ToString();
                    TMP.update_time = DateTime.Now;
                    TMP.w_mail_notify_now = 0;
                    TMP.e_mail_notify_now = 0;
                    TMP.f_mail_notify_now = 0;
                    TMP.w_message_notify_now = 0;
                    TMP.e_message_notify_now = 0;
                    TMP.f_message_notify_now = 0;

                    using (CMSEntities CMS = new CMSEntities())
                    {
                        CMS.TmpMonitorProperty.Add(TMP);
                        CMS.SaveChanges();
                        op_etime = DateTime.Now;
                        op_result = true;
                        op_a_count = 1;
                        op_s_count = op_a_count;
                        op_msg = "[" + op_name + "]執行[" + op_action + "]成功,監控項目編號[" + TMP.s_no + "]";
                        SF.logandshowInfo(op_msg, log_Info);

                        var Sys = CMS.v_SysList
                                        .Where(b => b.t_id == TMP.s_type)
                                        .Where(b => b.id == TMP.s_sys).First();

                        return RedirectToAction("Index", "MonitorProperty", new { name = Sys.value, sclass = TMP.s_class });
                    }
                }
                else
                {
                    op_etime = DateTime.Now;
                    op_result = true;
                    op_a_count = 1;
                    op_f_count = op_a_count;
                    op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,監控項目編號[" + TMP.s_no + "],錯誤訊息[輸入參數檢查失敗]";
                    SF.logandshowInfo(op_msg, log_Info);
                    return RedirectToAction("Create", "MonitorProperty");
                }
            }
            catch (Exception ex)
            {
                op_etime = DateTime.Now;
                string MailSubject = null;
                StringBuilder MailBody = null;

                TempData["CreateMsg"] = "<script>alert('監控項目新增作業異常，請洽系統管理人員')</script>";
                op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,監控項目編號[" + TMP.s_no + "],錯誤訊息[" + ex.ToString() + "]";
                SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢發生異常,請查詢Debug Log得到詳細資訊", log_Info);
                SF.logandshowInfo(op_msg, log_Err);

                //通知系統管理人員
                MailSubject = "[異常]中控處理系統-" + op_name + "模組執行[" + op_action + "]失敗";
                MailBody.Append("<table>");
                MailBody.Append("<tr><td>");
                MailBody.Append("[" + op_name + "]執行[" + op_action + "]失敗,詳細資訊如下");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append(op_msg);
                MailBody.Append("</td></tr>");
                MailBody.Append("</table>");

                SF.EmailNotify2Sys(Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver, true, MailSubject, MailBody.ToString());

                return RedirectToAction("Create", "MonitorProperty");
            }
            finally
            {
                //寫入DB Log
                OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                SF.log2DB(OPLoger, MailServer, MailServerPort, MailSender, MailReceiver);
            }
        }

        public ActionResult Edit(string sno)
        {
            //初始化系統參數
            Configer.Init();

            //共用涵式用
            SF.ConnStr = Configer.C_DBConnstring;
            SF.op_name = op_name;

            string op_action = null;
            DateTime op_stime = default(DateTime);
            DateTime op_etime = default(DateTime);
            int op_a_count = 0;
            int op_s_count = 0;
            int op_f_count = 0;
            string op_msg = string.Empty;
            bool op_result = false;

            string MailServer = Configer.MailServer;
            int MailServerPort = Configer.MailServerPort;
            string MailSender = Configer.MailSender;
            List<string> MailReceiver = Configer.MailReceiver;

            op_action = "取得監控項目詳細設定值作業";

            using (CMSEntities CMS = new CMSEntities())
            {
                try
                {
                    op_stime = DateTime.Now;
                    var MD = CMS.MonitorProperty
                        .Where(b => b.s_no == sno).First();
                    if (MD != null)
                    {

                        EditMonitorPropertyDataViewModel MV = new EditMonitorPropertyDataViewModel();

                        //取得環境清單
                        MV.ClassList = SF.getClassList(MD.s_class);
                        //取得分類清單
                        MV.TypeList = SF.getTypeList(MD.s_type);
                        //取得系統別清單
                        MV.SysList = SF.getSysList(MD.s_type, MD.s_sys);
                        //取得郵件主機設定清單
                        MV.MailServerProfileList = SF.getMailServerProfileList(-1);
                        //取得簡訊主機設定清單
                        MV.TextMessageProfileList = SF.getTextMessageProfileList(-1);
                        //取得Warn通知群組清單
                        MV.WarnNotifyGroupList = SF.getNotifyGroupList(-1);
                        //取得Error通知群組清單
                        MV.ErrorNotifyGroupList = SF.getNotifyGroupList(-1);
                        //取得Fatal通知群組清單
                        MV.FatalNotifyGroupList = SF.getNotifyGroupList(-1);
                        //取得監控項目設定
                        MV.s_no = MD.s_no;
                        MV.s_class = MD.s_class;
                        MV.s_type = MD.s_type;
                        MV.s_sys = MD.s_sys;
                        MV.s_subject = MD.s_subject;
                        MV.s_content = MD.s_content;
                        MV.s_position = MD.s_position;
                        MV.s_frequency = MD.s_frequency;
                        MV.s_timeout = MD.s_timeout;
                        MV.sysadmin = MD.sysadmin;
                        MV.MailServerProfile = Convert.ToInt16(MD.MailServerProfile);
                        MV.TextMessageProfile = Convert.ToInt16(MD.TextMessageProfile);
                        MV.w_sendmail = Convert.ToBoolean(MD.w_sendmail);
                        MV.w_sendmessage = Convert.ToBoolean(MD.w_sendmessage);
                        MV.w_notify = Convert.ToInt16(MD.w_notify);
                        MV.w_mail_notify_limit = Convert.ToInt16(MD.w_mail_notify_limit);
                        MV.w_message_notify_limit = Convert.ToInt16(MD.w_message_notify_limit);
                        MV.e_sendmail = Convert.ToBoolean(MD.e_sendmail);
                        MV.e_sendmessage = Convert.ToBoolean(MD.e_sendmessage);
                        MV.e_notify = Convert.ToInt16(MD.e_notify);
                        MV.e_mail_notify_limit = Convert.ToInt16(MD.e_mail_notify_limit);
                        MV.e_message_notify_limit = Convert.ToInt16(MD.e_message_notify_limit);
                        MV.f_sendmail = Convert.ToBoolean(MD.f_sendmail);
                        MV.f_sendmessage = Convert.ToBoolean(MD.f_sendmessage);
                        MV.f_notify = Convert.ToInt16(MD.f_notify);
                        MV.f_mail_notify_limit = Convert.ToInt16(MD.f_mail_notify_limit);
                        MV.f_message_notify_limit = Convert.ToInt16(MD.f_message_notify_limit);

                        op_etime = DateTime.Now;
                        op_result = true;
                        op_a_count = 1;
                        op_s_count = op_a_count;
                        op_msg = "[" + op_name + "]執行[" + op_action + "]成功,監控項目編號[" + sno + "]";
                        SF.logandshowInfo(op_msg, log_Info);

                        if (SF.CheckRepSNO(sno, "TmpMonitorProperty") == true)
                        {
                            TempData["NotAllowEdit"] = "Y";
                            TempData["EditMsg"] = "<script>alert('監控項目正在被編輯');</script>";
                            op_msg = op_msg + "正在被編輯中";
                            SF.logandshowInfo(op_msg, log_Info);
                        }

                        return View(MV);
                    }
                    else
                    {
                        op_etime = DateTime.Now;
                        TempData["EditMsg"] = "<script>alert('取得監控項目資料失敗');</script>";
                        op_f_count = 1;
                        op_a_count = op_f_count;
                        op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,監控項目編號[" + sno + "],錯誤訊息:[本次查詢無資料]";
                        SF.logandshowInfo(op_msg, log_Err);

                        string _s_class = sno.Substring(0, 1);
                        string _s_type = sno.Substring(1, 1);
                        string _s_sys = sno.Substring(2, 2);

                        var Sys = CMS.v_SysList
                                     .Where(b => b.t_id == _s_type)
                                     .Where(b => b.id == _s_sys).First();

                        return RedirectToAction("Index", "MonitorProperty", new { name = Sys.value, sclass = _s_class });
                    }
                }
                catch (Exception ex)
                {
                    op_etime = DateTime.Now;
                    TempData["EditMsg"] = "<script>alert('取得監控項目資料失敗');</script>";
                    op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,監控項目編號[" + sno + "],錯誤訊息:[" + ex.ToString() + "]";
                    SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢發生異常,請查詢Debug Log得到詳細資訊", log_Info);
                    SF.logandshowInfo(op_msg, log_Err);

                    string MailSubject = null;
                    StringBuilder MailBody = null;

                    //通知系統管理人員
                    MailSubject = "[異常]中控處理系統-" + op_name + "模組執行[" + op_action + "]失敗";
                    MailBody.Append("<table>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("[" + op_name + "]執行[" + op_action + "]失敗,詳細資訊如下");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("監控項目編號:[" + sno + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append(op_msg);
                    MailBody.Append("</td></tr>");
                    MailBody.Append("</table>");

                    SF.EmailNotify2Sys(Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver, true, MailSubject, MailBody.ToString());

                    string _s_class = sno.Substring(0, 1);
                    string _s_type = sno.Substring(1, 1);
                    string _s_sys = sno.Substring(2, 2);

                    var Sys = CMS.v_SysList
                                  .Where(b => b.t_id == _s_type)
                                  .Where(b => b.id == _s_sys).First();

                    return RedirectToAction("Index", "MonitorProperty", new { name = Sys.value, sclass = _s_class });
                }
                finally
                {
                    //寫入DB Log
                    OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                    SF.log2DB(OPLoger, MailServer, MailServerPort, MailSender, MailReceiver);
                }
            }
        }

        [HttpPost]
        public ActionResult Edit(TmpMonitorProperty TMP)
        {
            //初始化系統參數
            Configer.Init();

            //共用涵式用
            SF.ConnStr = Configer.C_DBConnstring;
            SF.op_name = op_name;

            string op_action = null;
            DateTime op_stime = default(DateTime);
            DateTime op_etime = default(DateTime);
            int op_a_count = 0;
            int op_s_count = 0;
            int op_f_count = 0;
            string op_msg = string.Empty;
            bool op_result = false;

            string MailServer = Configer.MailServer;
            int MailServerPort = Configer.MailServerPort;
            string MailSender = Configer.MailSender;
            List<string> MailReceiver = Configer.MailReceiver;

            op_action = "監控項目修改作業";

            try
            {
                op_stime = DateTime.Now;
                if (CheckInput4Edit(TMP) == true)
                {
                    TMP.update_account = Session["UserID"].ToString();
                    TMP.update_time = DateTime.Now;

                    using (CMSEntities CMS = new CMSEntities())
                    {
                        CMS.TmpMonitorProperty.Add(TMP);
                        CMS.SaveChanges();
                        op_etime = DateTime.Now;
                        op_result = true;
                        op_a_count = 1;
                        op_s_count = op_a_count;
                        op_msg = "[" + op_name + "]執行[" + op_action + "]成功,監控項目編號[" + TMP.s_no + "]";
                        SF.logandshowInfo(op_msg, log_Info);

                        var Sys = CMS.v_SysList
                                        .Where(b => b.t_id == TMP.s_type)
                                        .Where(b => b.id == TMP.s_sys).First();

                        return RedirectToAction("Index", "MonitorProperty", new { name = Sys.value, sclass = TMP.s_class });
                    }
                }
                else
                {
                    op_etime = DateTime.Now;
                    op_a_count = 1;
                    op_f_count = op_a_count;
                    op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,監控項目編號[" + TMP.s_no + "],錯誤訊息[輸入參數檢查失敗]";
                    SF.logandshowInfo(op_msg, log_Info);
                    return RedirectToAction("Edit", "MonitorProperty", new { sno = TMP.s_no });
                }
            }
            catch (Exception ex)
            {
                op_etime = DateTime.Now;
                op_result = false;
                TempData["EditMsg"] = "<script>alert('監控項目修改作業失敗');</script>";
                op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,監控項目編號[" + TMP.s_no + "],錯誤訊息:[" + ex.ToString() + "]";
                SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢發生異常,請查詢Debug Log得到詳細資訊", log_Info);
                SF.logandshowInfo(op_msg, log_Err);

                string MailSubject = null;
                StringBuilder MailBody = null;

                //通知系統管理人員
                MailSubject = "[異常]中控處理系統-" + op_name + "模組執行[" + op_action + "]失敗";
                MailBody.Append("<table>");
                MailBody.Append("<tr><td>");
                MailBody.Append("[" + op_name + "]執行[" + op_action + "]失敗,詳細資訊如下");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append("監控項目編號:[" + TMP.s_no + "]");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append(op_msg);
                MailBody.Append("</td></tr>");
                MailBody.Append("</table>");

                SF.EmailNotify2Sys(Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver, true, MailSubject, MailBody.ToString());

                return RedirectToAction("Edit", "MonitorProperty", new { sno = TMP.s_no });
            }
            finally
            {
                //寫入DB Log
                OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                SF.log2DB(OPLoger, MailServer, MailServerPort, MailSender, MailReceiver);
            }
        }

        /// <summary>
        /// 取得監控項目詳細資料
        /// </summary>
        /// <param name="sno">監控項目編號</param>
        /// <returns></returns>
        public ActionResult Detail(string sno)
        {
            //初始化系統參數
            Configer.Init();

            //共用涵式用
            SF.ConnStr = Configer.C_DBConnstring;
            SF.op_name = op_name;

            string op_action = null;
            DateTime op_stime = default(DateTime);
            DateTime op_etime = default(DateTime);
            int op_a_count = 0;
            int op_s_count = 0;
            int op_f_count = 0;
            string op_msg = string.Empty;
            bool op_result = false;

            string MailServer = Configer.MailServer;
            int MailServerPort = Configer.MailServerPort;
            string MailSender = Configer.MailSender;
            List<string> MailReceiver = Configer.MailReceiver;

            op_action = "取得監控項目詳細設定值作業";

            using (CMSEntities CMS = new CMSEntities())
            {
                try
                {
                    op_stime = DateTime.Now;
                    var MD = CMS.v_MonitorProperty_Detail
                                     .Where(b => b.s_no == sno).First();
                    if (MD != null)
                    {
                        op_etime = DateTime.Now;
                        op_result = true;
                        op_a_count = 1;
                        op_s_count = op_a_count;
                        op_msg = "[" + op_name + "]執行[" + op_action + "]成功,監控項目編號[" + sno + "]";
                        SF.logandshowInfo(op_msg, log_Info);
                        return View(MD);
                    }
                    else
                    {
                        op_etime = DateTime.Now;
                        TempData["GetDetailMsg"] = "<script>alert('取得監控項目資料失敗');</script>";
                        op_f_count = 1;
                        op_a_count = op_f_count;
                        op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,監控項目編號[" + sno + "],錯誤訊息:[本次查詢無資料]";
                        SF.logandshowInfo(op_msg, log_Err);
                        return RedirectToAction("Index", "MonitorProperty", new { name = "EC+ CA", sclass = "0" });
                    }
                }
                catch (Exception ex)
                {
                    op_etime = DateTime.Now;
                    TempData["GetDetailMsg"] = "<script>alert('取得監控項目資料失敗');</script>";
                    op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,監控項目編號[" + sno + "],錯誤訊息:[" + ex.ToString() + "]";
                    SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢發生異常,請查詢Debug Log得到詳細資訊", log_Info);
                    SF.logandshowInfo(op_msg, log_Err);

                    string MailSubject = null;
                    StringBuilder MailBody = null;

                    //通知系統管理人員
                    MailSubject = "[異常]中控處理系統-" + op_name + "模組執行[" + op_action + "]失敗";
                    MailBody.Append("<table>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("[" + op_name + "]執行[" + op_action + "]失敗,詳細資訊如下");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("監控項目編號:[" + sno + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append(op_msg);
                    MailBody.Append("</td></tr>");
                    MailBody.Append("</table>");

                    SF.EmailNotify2Sys(Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver, true, MailSubject, MailBody.ToString());

                    return RedirectToAction("Index", "MonitorProperty", new { name = "EC+ CA", sclass = "0" });
                }
                finally
                {
                    //寫入DB Log
                    OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                    SF.log2DB(OPLoger, MailServer, MailServerPort, MailSender, MailReceiver);
                }
            }
        }

        public ActionResult ReviewIndex()
        {
            //初始化系統參數
            Configer.Init();

            //共用涵式用
            SF.ConnStr = Configer.C_DBConnstring;
            SF.op_name = op_name;

            string op_action = null;
            DateTime op_stime = default(DateTime);
            DateTime op_etime = default(DateTime);
            int op_a_count = 0;
            int op_s_count = 0;
            int op_f_count = 0;
            string op_msg = string.Empty;
            bool op_result = false;

            string MailServer = Configer.MailServer;
            int MailServerPort = Configer.MailServerPort;
            string MailSender = Configer.MailSender;
            List<string> MailReceiver = Configer.MailReceiver;

            op_action = "取得待覆核監控項目清單作業";

            using (CMSEntities CMS = new CMSEntities())
            {
                try
                {
                    op_stime = DateTime.Now;
                    string UserID = Session["UserID"].ToString();
                    var MD = CMS.v_TmpMonitorproperty_View_Detail
                                .Where(b => b.update_account != UserID).ToList();

                    if (MD != null && MD.Count > 0)
                    {
                        op_etime = DateTime.Now;
                        op_result = true;
                        op_a_count = MD.Count;
                        op_s_count = op_a_count;
                        op_msg = "[" + op_name + "]執行[" + op_action + "]成功,共取得[" + op_s_count.ToString() + "]待覆核監控項目";
                        SF.logandshowInfo(op_msg, log_Info);
                        return View(MD);
                    }
                    else
                    {
                        op_etime = DateTime.Now;
                        TempData["GetReviewListMsg"] = "<script>alert('無待覆核監控項目');</script>";
                        op_result = true;
                        op_a_count = 1;
                        op_s_count = op_a_count;
                        op_msg = "[" + op_name + "]執行[" + op_action + "]成功,錯誤訊息:[本次查詢無資料]";
                        SF.logandshowInfo(op_msg, log_Err);
                        return RedirectToAction("Index", "MonitorProperty", new { name = "EC+ CA", sclass = "0" });
                    }

                }
                catch (Exception ex)
                {
                    op_etime = DateTime.Now;
                    TempData["GetReviewListMsg"] = "<script>alert('取得待覆核監控項目清單失敗');</script>";
                    op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,錯誤訊息:[" + ex.ToString() + "]";
                    SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢發生異常,請查詢Debug Log得到詳細資訊", log_Info);
                    SF.logandshowInfo(op_msg, log_Err);

                    string MailSubject = null;
                    StringBuilder MailBody = null;

                    //通知系統管理人員
                    MailSubject = "[異常]中控處理系統-" + op_name + "模組執行[" + op_action + "]失敗";
                    MailBody.Append("<table>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("[" + op_name + "]執行[" + op_action + "]失敗,詳細資訊如下");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append(op_msg);
                    MailBody.Append("</td></tr>");
                    MailBody.Append("</table>");

                    SF.EmailNotify2Sys(Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver, true, MailSubject, MailBody.ToString());

                    return RedirectToAction("Index", "MonitorProperty", new { name = "EC+ CA", sclass = "0" });
                }
                finally
                {
                    //寫入DB Log
                    OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                    SF.log2DB(OPLoger, MailServer, MailServerPort, MailSender, MailReceiver);
                }
            }
        }

        public ActionResult Review(string sno)
        {
            //初始化系統參數
            Configer.Init();

            //共用涵式用
            SF.ConnStr = Configer.C_DBConnstring;
            SF.op_name = op_name;

            string op_action = null;
            DateTime op_stime = default(DateTime);
            DateTime op_etime = default(DateTime);
            int op_a_count = 0;
            int op_s_count = 0;
            int op_f_count = 0;
            string op_msg = string.Empty;
            bool op_result = false;

            string MailServer = Configer.MailServer;
            int MailServerPort = Configer.MailServerPort;
            string MailSender = Configer.MailSender;
            List<string> MailReceiver = Configer.MailReceiver;

            op_action = "取得待覆核監控項目詳細設定值作業";

            using (CMSEntities CMS = new CMSEntities())
            {
                try
                {
                    op_stime = DateTime.Now;

                    var MD = CMS.TmpMonitorProperty
                        .Where(b => b.s_no == sno).First();
                    if (MD != null)
                    {

                        ReviewMonitorPropertyDataViewModel MV = new ReviewMonitorPropertyDataViewModel();

                        //取得環境清單
                        MV.ClassList = SF.getClassList(MD.s_class);
                        //取得分類清單
                        MV.TypeList = SF.getTypeList(MD.s_type);
                        //取得系統別清單
                        MV.SysList = SF.getSysList(MD.s_type, MD.s_sys);
                        //取得郵件主機設定清單
                        MV.MailServerProfileList = SF.getMailServerProfileList(-1);
                        //取得簡訊主機設定清單
                        MV.TextMessageProfileList = SF.getTextMessageProfileList(-1);
                        //取得Warn通知群組清單
                        MV.WarnNotifyGroupList = SF.getNotifyGroupList(-1);
                        //取得Error通知群組清單
                        MV.ErrorNotifyGroupList = SF.getNotifyGroupList(-1);
                        //取得Fatal通知群組清單
                        MV.FatalNotifyGroupList = SF.getNotifyGroupList(-1);
                        //取得監控項目設定
                        MV.s_no = MD.s_no;
                        MV.s_class = MD.s_class;
                        MV.s_type = MD.s_type;
                        MV.s_sys = MD.s_sys;
                        MV.s_subject = MD.s_subject;
                        MV.s_content = MD.s_content;
                        MV.s_position = MD.s_position;
                        MV.s_frequency = MD.s_frequency;
                        MV.s_timeout = MD.s_timeout;
                        MV.sysadmin = MD.sysadmin;
                        MV.MailServerProfile = Convert.ToInt16(MD.MailServerProfile);
                        MV.TextMessageProfile = Convert.ToInt16(MD.TextMessageProfile);
                        MV.w_sendmail = Convert.ToBoolean(MD.w_sendmail);
                        MV.w_sendmessage = Convert.ToBoolean(MD.w_sendmessage);
                        MV.w_notify = Convert.ToInt16(MD.w_notify);
                        MV.w_mail_notify_limit = Convert.ToInt16(MD.w_mail_notify_limit);
                        MV.w_message_notify_limit = Convert.ToInt16(MD.w_message_notify_limit);
                        MV.e_sendmail = Convert.ToBoolean(MD.e_sendmail);
                        MV.e_sendmessage = Convert.ToBoolean(MD.e_sendmessage);
                        MV.e_notify = Convert.ToInt16(MD.e_notify);
                        MV.e_mail_notify_limit = Convert.ToInt16(MD.e_mail_notify_limit);
                        MV.e_message_notify_limit = Convert.ToInt16(MD.e_message_notify_limit);
                        MV.f_sendmail = Convert.ToBoolean(MD.f_sendmail);
                        MV.f_sendmessage = Convert.ToBoolean(MD.f_sendmessage);
                        MV.f_notify = Convert.ToInt16(MD.f_notify);
                        MV.f_mail_notify_limit = Convert.ToInt16(MD.f_mail_notify_limit);
                        MV.f_message_notify_limit = Convert.ToInt16(MD.f_message_notify_limit);
                        MV.issync = Convert.ToBoolean(MD.issync);
                        MV.update_account = MD.update_account;
                        MV.update_time = Convert.ToDateTime(MD.update_time);

                        op_etime = DateTime.Now;
                        op_result = true;
                        op_a_count = 1;
                        op_s_count = op_a_count;
                        op_msg = "[" + op_name + "]執行[" + op_action + "]成功,監控項目編號[" + sno + "]";
                        SF.logandshowInfo(op_msg, log_Info);

                        return View(MV);
                    }
                    else
                    {
                        op_etime = DateTime.Now;
                        TempData["ReviewMsg"] = "<script>alert('取得待覆核監控項目資料失敗');</script>";
                        op_f_count = 1;
                        op_a_count = op_f_count;
                        op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,監控項目編號[" + sno + "],錯誤訊息:[本次查詢無資料]";
                        SF.logandshowInfo(op_msg, log_Err);

                        string _s_class = sno.Substring(0, 1);
                        string _s_type = sno.Substring(1, 1);
                        string _s_sys = sno.Substring(2, 2);

                        var Sys = CMS.v_SysList
                                     .Where(b => b.t_id == _s_type)
                                     .Where(b => b.id == _s_sys).First();

                        return RedirectToAction("Index", "MonitorProperty", new { name = Sys.value, sclass = _s_class });
                    }
                    //var newMD = CMS.v_TmpMonitorProperty_Detail
                    //                 .Where(b => b.s_no == sno).First();
                    //var oldMD = CMS.v_MonitorProperty_Detail
                    //                 .Where(b => b.s_no == sno).First();
                    //if (newMD != null)
                    //{

                    //    if (oldMD != null)
                    //    {

                    //        ReviewMonitorPropertyDataViewModel MV = new ReviewMonitorPropertyDataViewModel();
                    //        MV.oldMP = oldMD;
                    //        MV.newMP = newMD;

                    //        op_etime = DateTime.Now;
                    //        op_result = true;
                    //        op_a_count = 1;
                    //        op_s_count = op_a_count;
                    //        op_msg = "[" + op_name + "]執行[" + op_action + "]成功,監控項目編號[" + sno + "]";
                    //        SF.logandshowInfo(op_msg, log_Info);
                    //        return View(MV);
                    //    }
                    //    else
                    //    {
                    //        op_etime = DateTime.Now;
                    //        TempData["ReviewMsg"] = "<script>alert('取得待覆核監控項目資料失敗');</script>";
                    //        op_f_count = 1;
                    //        op_a_count = op_f_count;
                    //        op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,監控項目編號[" + sno + "],錯誤訊息:[取得原監控項目資料失敗]";
                    //        SF.logandshowInfo(op_msg, log_Err);
                    //        return RedirectToAction("Index", "MonitorProperty", new { name = "EC+ CA", sclass = "0" });
                    //    }
                    //}
                    //else
                    //{
                    //    op_etime = DateTime.Now;
                    //    TempData["ReviewMsg"] = "<script>alert('取得待覆核監控項目資料失敗');</script>";
                    //    op_f_count = 1;
                    //    op_a_count = op_f_count;
                    //    op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,監控項目編號[" + sno + "],錯誤訊息:[本次查詢無資料]";
                    //    SF.logandshowInfo(op_msg, log_Err);
                    //    return RedirectToAction("Index", "MonitorProperty", new { name = "EC+ CA", sclass = "0" });
                    //}
                }
                catch (Exception ex)
                {
                    op_etime = DateTime.Now;
                    TempData["ReviewMsg"] = "<script>alert('取得待覆核監控項目資料失敗');</script>";
                    op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,監控項目編號[" + sno + "],錯誤訊息:[" + ex.ToString() + "]";
                    SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢發生異常,請查詢Debug Log得到詳細資訊", log_Info);
                    SF.logandshowInfo(op_msg, log_Err);

                    string MailSubject = null;
                    StringBuilder MailBody = null;

                    //通知系統管理人員
                    MailSubject = "[異常]中控處理系統-" + op_name + "模組執行[" + op_action + "]失敗";
                    MailBody.Append("<table>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("[" + op_name + "]執行[" + op_action + "]失敗,詳細資訊如下");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("監控項目編號:[" + sno + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append(op_msg);
                    MailBody.Append("</td></tr>");
                    MailBody.Append("</table>");

                    SF.EmailNotify2Sys(Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver, true, MailSubject, MailBody.ToString());

                    return RedirectToAction("Index", "MonitorProperty", new { name = "EC+ CA", sclass = "0" });
                }
                finally
                {
                    //寫入DB Log
                    OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                    SF.log2DB(OPLoger, MailServer, MailServerPort, MailSender, MailReceiver);
                }
            }
        }

        [HttpPost]
        public ActionResult Review(TmpMonitorProperty TMP)
        {
            //初始化系統參數
            Configer.Init();

            //共用涵式用
            SF.ConnStr = Configer.C_DBConnstring;
            SF.op_name = op_name;

            string op_action = null;
            DateTime op_stime = default(DateTime);
            DateTime op_etime = default(DateTime);
            int op_a_count = 0;
            int op_s_count = 0;
            int op_f_count = 0;
            string op_msg = string.Empty;
            bool op_result = false;

            string MailServer = Configer.MailServer;
            int MailServerPort = Configer.MailServerPort;
            string MailSender = Configer.MailSender;
            List<string> MailReceiver = Configer.MailReceiver;

            op_action = "監控項目覆核作業";

            try
            {
                op_stime = DateTime.Now;
                using (CMSEntities CMS = new CMSEntities())
                {
                    var MP = CMS.MonitorProperty
                                .Where(b => b.s_no == TMP.s_no).First();

                    MP.review_account = Session["UserID"].ToString();
                    MP.review_time = DateTime.Now;

                    MP.s_subject = TMP.s_subject;
                    MP.s_content = TMP.s_content;
                    MP.s_position = TMP.s_position;
                    MP.s_timeout = TMP.s_timeout;
                    MP.s_frequency = TMP.s_frequency;
                    MP.params_host = TMP.params_host;
                    MP.@params = TMP.@params;
                    MP.MailServerProfile = TMP.MailServerProfile;
                    MP.TextMessageProfile = TMP.TextMessageProfile;
                    MP.sysadmin = TMP.sysadmin;
                    MP.w_notify = TMP.w_notify;
                    MP.w_sendmail = TMP.w_sendmail;
                    MP.w_sendmessage = TMP.w_sendmessage;
                    MP.w_mail_notify_limit = TMP.w_mail_notify_limit;
                    MP.w_message_notify_limit = TMP.w_message_notify_limit;

                    MP.e_notify = TMP.e_notify;
                    MP.e_sendmail = TMP.e_sendmail;
                    MP.e_sendmessage = TMP.e_sendmessage;
                    MP.e_mail_notify_limit = TMP.e_mail_notify_limit;
                    MP.e_message_notify_limit = TMP.e_message_notify_limit;

                    MP.f_notify = TMP.f_notify;
                    MP.f_sendmail = TMP.f_sendmail;
                    MP.f_sendmessage = TMP.f_sendmessage;
                    MP.f_mail_notify_limit = TMP.f_mail_notify_limit;
                    MP.f_message_notify_limit = TMP.f_message_notify_limit;

                    MP.update_account = TMP.update_account;
                    MP.update_time = TMP.update_time;

                    CMS.Entry(MP).State = EntityState.Modified;
                    CMS.SaveChanges();

                    if (TMP.issync == true)
                    {
                        List<string> SyncResultList = sync2MonitorTool("Sync", TMP.s_no, TMP.params_host, TMP.@params);
                        string SyncResult = string.Empty;
                        string SyncResultMsg = string.Empty;

                        SyncResult = SyncResultList[0];
                        SyncResultMsg = SyncResultList[1];

                        op_etime = DateTime.Now;

                        if (SyncResult == "0")
                        {
                            op_etime = DateTime.Now;
                            op_s_count = 1;
                            op_a_count = op_s_count;
                            op_msg = "[" + op_name + "]執行[" + op_action + "]成功,監控項目編號:[" + TMP.s_no + "]";
                            op_result = true;

                            //寫入文字檔Log
                            SF.logandshowInfo(op_msg, log_Info);

                            TempData["ReviewMsg"] = "<script>alert('監控項目覆核及同步參數至檢測工具作業成功');</script>";

                            return RedirectToAction("ReviewIndex", "MonitorProperty");
                        }
                        else
                        {
                            op_etime = DateTime.Now;
                            op_f_count = 1;
                            op_a_count = op_f_count;
                            op_msg = SyncResultMsg;

                            //寫入文字檔Log
                            SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,監控項目編號:[" + TMP.s_no + "]", log_Info);

                            TempData["ReviewMsg"] = "<script>alert('監控項目覆核作業成功，但同步參數至檢測工具失敗');</script>";

                            return RedirectToAction("ReviewIndex", "MonitorProperty");
                        }
                    }
                    else
                    {
                        op_etime = DateTime.Now;
                        op_result = true;
                        op_a_count = 1;
                        op_s_count = op_a_count;
                        op_msg = "[" + op_name + "]執行[" + op_action + "]成功,監控項目編號[" + TMP.s_no + "]";
                        SF.logandshowInfo(op_msg, log_Info);

                        TempData["ReviewMsg"] = "<script>alert('監控項目覆核作業成功');</script>";

                        return RedirectToAction("ReviewIndex", "MonitorProperty");
                    }
                }
            }
            catch (Exception ex)
            {
                op_etime = DateTime.Now;
                op_result = false;
                TempData["ReviewMsg"] = "<script>alert('監控項目覆核作業失敗');</script>";
                op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,監控項目編號[" + TMP.s_no + "],錯誤訊息:[" + ex.ToString() + "]";
                SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次作業發生異常,請查詢Debug Log得到詳細資訊", log_Info);
                SF.logandshowInfo(op_msg, log_Err);

                string MailSubject = null;
                StringBuilder MailBody = null;

                //通知系統管理人員
                MailSubject = "[異常]中控處理系統-" + op_name + "模組執行[" + op_action + "]失敗";
                MailBody.Append("<table>");
                MailBody.Append("<tr><td>");
                MailBody.Append("[" + op_name + "]執行[" + op_action + "]失敗,詳細資訊如下");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append("監控項目編號:[" + TMP.s_no + "]");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append(op_msg);
                MailBody.Append("</td></tr>");
                MailBody.Append("</table>");

                SF.EmailNotify2Sys(Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver, true, MailSubject, MailBody.ToString());

                return RedirectToAction("ReviewIndex", "MonitorProperty");
            }
            finally
            {
                //寫入DB Log
                OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                SF.log2DB(OPLoger, MailServer, MailServerPort, MailSender, MailReceiver);
            }
        }

        [HttpPost]
        public string Sync(string SyncList)
        {
            //初始化系統參數
            Configer.Init();

            //共用涵式用
            SF.ConnStr = Configer.C_DBConnstring;
            SF.op_name = op_name;

            string op_action = null;
            DateTime op_stime = default(DateTime);
            DateTime op_etime = default(DateTime);
            int op_a_count = 0;
            int op_s_count = 0;
            int op_f_count = 0;
            string op_msg = string.Empty;
            bool op_result = false;

            string MailServer = Configer.MailServer;
            int MailServerPort = Configer.MailServerPort;
            string MailSender = Configer.MailSender;
            List<string> MailReceiver = Configer.MailReceiver;

            op_action = "監控工具參數同步作業";

            //System.Threading.Thread.Sleep(3000); 

            string Result = "success";

            //return Result;

            try
            {
                op_stime = DateTime.Now;
                if (SyncList == "" || SyncList == null)
                {
                    op_etime = DateTime.Now;
                    op_msg = "輸入資料錯誤，未選擇監控項目";
                    //寫入文字檔Log
                    SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,錯誤訊息[" + op_msg + "]", log_Info);
                    Result = "請至少勾選一項目";

                    return Result;
                }
                else
                {
                    char[] sp = { ',' };
                    List<string> snolist = StringProcessor.SplitString2Array(SyncList, sp);

                    op_a_count = snolist.Count;

                    foreach (string item in snolist)
                    {
                        using (CMSEntities CMS = new CMSEntities())
                        {
                            string s_no = item;

                            var MP = CMS.MonitorProperty
                                        .Where(b => b.s_no == s_no).First();

                            if (MP != null)
                            {
                                string params_host = MP.params_host;
                                string MonitorToolParams = MP.@params;

                                List<string> SyncResultList = sync2MonitorTool("Sync", s_no, params_host, MonitorToolParams);
                                string SyncResult = string.Empty;
                                string SyncResultMsg = string.Empty;

                                SyncResult = SyncResultList[0];
                                SyncResultMsg = SyncResultList[1];

                                if (SyncResult == "0")
                                {
                                    op_s_count += 1;
                                    SF.logandshowInfo("監控項目[" + s_no + "]同步成功", log_Info);
                                }
                                else
                                {
                                    op_f_count += 1;
                                    SF.logandshowInfo("監控項目[" + s_no + "]同步失敗,錯誤原因:[" + SyncResultMsg + "]", log_Info);
                                }
                            }
                        }
                    }

                    if (op_f_count == 0)
                    {
                        op_etime = DateTime.Now;
                        op_result = true;
                        op_msg = "本次共同步作業全部成功";
                        //寫入文字檔Log
                        SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]成功," + op_msg, log_Info);

                        Result = op_msg;

                        return Result;
                    }
                    else
                    {
                        op_etime = DateTime.Now;
                        op_msg = "本次共同步成功[" + op_s_count.ToString() + "]筆，失敗[" + op_f_count.ToString() + "]筆";
                        SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗," + op_msg, log_Info);

                        Result = op_msg;

                        return Result;
                    }
                }
            }
            catch (Exception ex)
            {
                op_etime = DateTime.Now;
                op_result = false;
                op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,錯誤訊息:[" + ex.ToString() + "]";
                SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次作業發生異常,請查詢Debug Log得到詳細資訊", log_Info);
                SF.logandshowInfo(op_msg, log_Err);

                string MailSubject = null;
                StringBuilder MailBody = null;

                //通知系統管理人員
                MailSubject = "[異常]中控處理系統-" + op_name + "模組執行[" + op_action + "]失敗";
                MailBody.Append("<table>");
                MailBody.Append("<tr><td>");
                MailBody.Append("[" + op_name + "]執行[" + op_action + "]失敗,詳細資訊如下");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append(op_msg);
                MailBody.Append("</td></tr>");
                MailBody.Append("</table>");

                SF.EmailNotify2Sys(Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver, true, MailSubject, MailBody.ToString());

                Result = "同步監控項目作業發生異常";

                return Result;
            }
            finally
            {
                //寫入DB Log
                OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                SF.log2DB(OPLoger, MailServer, MailServerPort, MailSender, MailReceiver);
            }
        }

        public ActionResult MonitorResultIndex(string sno, int page = 1)
        {
            //初始化系統參數
            Configer.Init();

            //共用涵式用
            SF.ConnStr = Configer.C_DBConnstring;
            SF.op_name = op_name;

            string op_action = null;
            DateTime op_stime = default(DateTime);
            DateTime op_etime = default(DateTime);
            int op_a_count = 0;
            int op_s_count = 0;
            int op_f_count = 0;
            string op_msg = string.Empty;
            bool op_result = false;

            string MailServer = Configer.MailServer;
            int MailServerPort = Configer.MailServerPort;
            string MailSender = Configer.MailSender;
            List<string> MailReceiver = Configer.MailReceiver;

            op_action = "取得監控項目回報歷史資料作業";

            using (CMSEntities CMS = new CMSEntities())
            {
                try
                {
                    op_stime = DateTime.Now;
                    DateTime STime = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd") + " 00:00:00");
                    DateTime ETime = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd") + " 23:59:00");

                    int currentPage = page < 1 ? 1 : page;

                    //var MD = CMS.v_Monitor_All_Reult_History
                    //            .Where(b => b.s_no == sno)
                    //            .OrderByDescending(b => b.回報時間);

                    var MD = CMS.ALL_LOG
                                .Where(b => b.s_no == sno)
                                .OrderByDescending(b => b.s_time);

                    var ResultMD = MD.ToPagedList(currentPage, Configer.NumofgridviewPage_perrows);
                    //.Where(b => b.回報時間 >= STime)
                    //.Where(b => b.回報時間 <= ETime).ToList();

                    if (ResultMD != null && ResultMD.Count > 0)
                    {
                        op_etime = DateTime.Now;
                        MonitorResultHistory MV = new MonitorResultHistory();
                        MV.MonitorDataHistory = ResultMD;

                        var MP = CMS.v_MonitorProperty_Detail
                                    .Where(b => b.s_no == sno).First();

                        MV.監控項目主旨 = MP.s_subject;
                        MV.環境 = MP.環境;
                        MV.分類 = MP.分類;
                        MV.系統別 = MP.系統別;
                        MV.sno = MP.s_no;

                        //取得環境清單
                        MV.ClassList = SF.getClassList(MP.s_class);
                        //取得分類清單
                        MV.TypeList = SF.getTypeList(MP.s_type);
                        //取得系統別清單
                        MV.SysList = SF.getSysList(MP.s_type, MP.s_sys);
                        //取得回報狀態清單
                        MV.StatusList = SF.getStatusList("-1");
                        //取得監控項目主旨清單
                        MV.SubjectList = SF.getSubjectList(MP.s_class, MP.s_type, MP.s_sys, "");

                        op_result = true;
                        op_a_count = ResultMD.Count;
                        op_s_count = op_a_count;
                        op_msg = "[" + op_name + "]執行[" + op_action + "]成功,共取得[" + op_s_count.ToString() + "]監控項目回報歷史資料";
                        SF.logandshowInfo(op_msg, log_Info);
                        return View(MV);
                    }
                    else
                    {
                        op_etime = DateTime.Now;
                        TempData["GetMonitorResultMsg"] = "<script>alert('無監控項目回報歷史資料');</script>";
                        op_result = true;
                        op_a_count = 1;
                        op_s_count = op_a_count;
                        op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,錯誤訊息:[本次查詢無資料]";
                        SF.logandshowInfo(op_msg, log_Err);
                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (Exception ex)
                {

                    op_etime = DateTime.Now;
                    TempData["GetMonitorResultMsg"] = "<script>alert('取得堅控項目回報歷史資料失敗');</script>";
                    op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,錯誤訊息:[" + ex.ToString() + "]";
                    SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢發生異常,請查詢Debug Log得到詳細資訊", log_Info);
                    SF.logandshowInfo(op_msg, log_Err);

                    string MailSubject = null;
                    StringBuilder MailBody = null;

                    //通知系統管理人員
                    MailSubject = "[異常]中控處理系統-" + op_name + "模組執行[" + op_action + "]失敗";
                    MailBody.Append("<table>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("[" + op_name + "]執行[" + op_action + "]失敗,詳細資訊如下");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append(op_msg);
                    MailBody.Append("</td></tr>");
                    MailBody.Append("</table>");

                    SF.EmailNotify2Sys(Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver, true, MailSubject, MailBody.ToString());

                    return RedirectToAction("Index", "Home");
                }
                finally
                {
                    //寫入DB Log
                    OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                    SF.log2DB(OPLoger, MailServer, MailServerPort, MailSender, MailReceiver);
                }
            }
        }

        [HttpPost]
        public ActionResult MonitorResultIndex(QueryMonitorResultData QMR, int page = 1)
        {
            //初始化系統參數
            Configer.Init();

            //共用涵式用
            SF.ConnStr = Configer.C_DBConnstring;
            SF.op_name = op_name;

            string op_action = null;
            DateTime op_stime = default(DateTime);
            DateTime op_etime = default(DateTime);
            int op_a_count = 0;
            int op_s_count = 0;
            int op_f_count = 0;
            string op_msg = string.Empty;
            bool op_result = false;

            string MailServer = Configer.MailServer;
            int MailServerPort = Configer.MailServerPort;
            string MailSender = Configer.MailSender;
            List<string> MailReceiver = Configer.MailReceiver;

            op_action = "取得監控項目回報歷史資料作業";

            using (CMSEntities CMS = new CMSEntities())
            {
                try
                {
                    op_stime = DateTime.Now;
                    string sno = QMR.sno;
                    string nowStatus = QMR.nowStatus;
                    string nowClass = QMR.nowClass;
                    string nowType = QMR.nowType;
                    string nowSys = QMR.nowSys;
                    DateTime STime = QMR.STime;
                    DateTime ETime = QMR.ETime;

                    string TmpETime = ETime.ToString("yyyy/MM/dd") + " 23:59:59";
                    ETime = Convert.ToDateTime(TmpETime);

                    Expression<Func<ALL_LOG, bool>> StatusWhereCondition;
                    if (nowStatus != "-1")
                    {
                        StatusWhereCondition = b => b.s_status == nowStatus;
                    }
                    else
                    {
                        StatusWhereCondition = b => 1 == 1;
                    }

                    int currentPage = page < 1 ? 1 : page;

                    //var MD = CMS.v_Monitor_All_Reult_History
                    //           .Where(b => b.s_no == sno)
                    //           .Where(StatusWhereCondition)
                    //           .Where(b => b.回報時間 >= STime)
                    //           .Where(b => b.回報時間 <= ETime)
                    //           .OrderByDescending(b => b.回報時間);

                    var MD = CMS.ALL_LOG
                             .Where(b => b.s_no == sno)
                             .Where(StatusWhereCondition)
                             .Where(b => b.s_time >= STime)
                             .Where(b => b.s_time <= ETime)
                             .OrderByDescending(b => b.s_time);

                    var ResultMD = MD.ToPagedList(currentPage, Configer.NumofgridviewPage_perrows);

                    if (ResultMD != null && ResultMD.Count > 0)
                    {
                        op_etime = DateTime.Now;
                        MonitorResultHistory MV = new MonitorResultHistory();
                        MV.MonitorDataHistory = ResultMD;

                        var MP = CMS.v_MonitorProperty_Detail
                                    .Where(b => b.s_no == sno).First();

                        MV.sno = MP.s_no;
                        MV.監控項目主旨 = MP.s_subject;
                        MV.環境 = MP.環境;
                        MV.分類 = MP.分類;
                        MV.系統別 = MP.系統別;

                        //取得環境清單
                        MV.ClassList = SF.getClassList(MP.s_class);
                        //取得分類清單
                        MV.TypeList = SF.getTypeList(MP.s_type);
                        //取得系統別清單
                        MV.SysList = SF.getSysList(MP.s_type, MP.s_sys);
                        //取得回報狀態清單
                        MV.StatusList = SF.getStatusList("-1");
                        //取得監控項目主旨清單
                        MV.SubjectList = SF.getSubjectList(MP.s_class, MP.s_type, MP.s_sys, "");

                        op_result = true;
                        op_a_count = ResultMD.Count;
                        op_s_count = op_a_count;
                        op_msg = "[" + op_name + "]執行[" + op_action + "]成功,共取得[" + op_s_count.ToString() + "]監控項目回報歷史資料";
                        SF.logandshowInfo(op_msg, log_Info);
                        return View(MV);
                    }
                    else
                    {
                        op_etime = DateTime.Now;
                        TempData["GetMonitorResultMsg"] = "<script>alert('無監控項目回報歷史資料');</script>";
                        op_result = true;
                        op_a_count = 1;
                        op_s_count = op_a_count;
                        op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,錯誤訊息:[本次查詢無資料]";
                        SF.logandshowInfo(op_msg, log_Err);
                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (Exception ex)
                {

                    op_etime = DateTime.Now;
                    TempData["GetMonitorResultMsg"] = "<script>alert('取得堅控項目回報歷史資料失敗');</script>";
                    op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,錯誤訊息:[" + ex.ToString() + "]";
                    SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢發生異常,請查詢Debug Log得到詳細資訊", log_Info);
                    SF.logandshowInfo(op_msg, log_Err);

                    string MailSubject = null;
                    StringBuilder MailBody = null;

                    //通知系統管理人員
                    MailSubject = "[異常]中控處理系統-" + op_name + "模組執行[" + op_action + "]失敗";
                    MailBody.Append("<table>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("[" + op_name + "]執行[" + op_action + "]失敗,詳細資訊如下");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append(op_msg);
                    MailBody.Append("</td></tr>");
                    MailBody.Append("</table>");

                    SF.EmailNotify2Sys(Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver, true, MailSubject, MailBody.ToString());

                    return RedirectToAction("Index", "Home");
                }
                finally
                {
                    //寫入DB Log
                    OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                    SF.log2DB(OPLoger, MailServer, MailServerPort, MailSender, MailReceiver);
                }
            }
        }

        /// <summary>
        /// 取得監控項目檢視資料
        /// </summary>
        /// <param name="name"></param>
        /// <param name="s_class"></param>
        /// <returns></returns>
        private MonitorPropertyDataViewModel getMonitorPropertyData(string name, string s_class)
        {
            MonitorPropertyDataViewModel MV = new MonitorPropertyDataViewModel();

            if (name == "EC  CA")
            {
                name = "EC+ CA";
            }

            using (CMSEntities CMS = new CMSEntities())
            {
                string s_type = "-1";
                string s_sys = "-1";
                string s_status = "-1";
                string s_user = "-1";

                if (s_class == null)
                {
                    s_class = "-1";
                }

                var MD = new List<v_Monitor_Data>();

                string op_action = null;
                DateTime op_stime = default(DateTime);
                DateTime op_etime = default(DateTime);
                int op_a_count = 0;
                int op_s_count = 0;
                int op_f_count = 0;
                string op_msg = string.Empty;
                bool op_result = false;

                string MailServer = Configer.MailServer;
                int MailServerPort = Configer.MailServerPort;
                string MailSender = Configer.MailSender;
                List<string> MailReceiver = Configer.MailReceiver;

                op_action = "取得監控項目檢視資料作業";

                try
                {
                    op_stime = DateTime.Now;
                    char[] SplitSymbol3 = Configer.SplitSymbol3.ToCharArray();

                    if (name != null)
                    {
                        List<string> Ids = StringProcessor.SplitString2Array(getbtnidInfo(name), SplitSymbol3);

                        //取得監控項目資料
                        if (Ids.Count > 1)
                        {
                            s_type = Ids[0];
                            s_sys = Ids[1];
                        }
                        else
                        {
                            s_type = Ids[0];
                        }
                    }

                    Expression<Func<v_Monitor_Data, bool>> MonitorDataClassWhereCondition;
                    if (s_class != "-1")
                    {
                        MonitorDataClassWhereCondition = b => b.s_class == s_class;
                    }
                    else
                    {
                        MonitorDataClassWhereCondition = b => 1 == 1;
                    }

                    Expression<Func<v_Monitor_Data, bool>> MonitorDataTypeWhereCondition;
                    if (s_type != "-1")
                    {
                        MonitorDataTypeWhereCondition = b => b.s_type == s_type;
                    }
                    else
                    {
                        MonitorDataTypeWhereCondition = b => 1 == 1;
                    }
                    Expression<Func<v_Monitor_Data, bool>> MonitorDataSysWhereCondition;
                    if (s_sys != "-1")
                    {
                        MonitorDataSysWhereCondition = b => b.s_sys == s_sys;
                    }
                    else
                    {
                        MonitorDataSysWhereCondition = b => 1 == 1;
                    }

                    MD = CMS.v_Monitor_Data
                      .Where(MonitorDataClassWhereCondition)
                      .Where(MonitorDataTypeWhereCondition)
                      .Where(MonitorDataSysWhereCondition)
                      .OrderBy(b => b.順序).ThenBy(b => b.監控項目編號).ToList();


                    bool UseCertLogin = Convert.ToBoolean(Session["UseCertLogin"].ToString());
                    string UserID = Session["UserID"].ToString();
                    int UserRole = Convert.ToInt16(Session["UserRole"].ToString());
                    int FuncID = 11;
                    //取得是否可以編輯監控項目權限
                    MV.caneditMonitProperty = SF.isedit(UseCertLogin, UserRole, FuncID);
                    //取得目前使用者可以覆核的監控項目個數
                    MV.needReviewCount = CMS.TmpMonitorProperty
                       .Where(b => b.update_account != UserID).Count();
                    //取得環境清單
                    MV.ClassList = SF.getClassList(s_class);
                    //取得分類清單
                    MV.TypeList = SF.getTypeList(s_type);
                    //取得系統別清單
                    MV.SysList = SF.getSysList(s_type, s_sys);
                    //取得使用者清單
                    MV.UserList = SF.getUserList(s_user);
                    //取得回報狀態清單
                    MV.StatusList = SF.getStatusList(s_status);
                    //取得監控項目主旨清單
                    MV.SubjectList = SF.getSubjectList(s_class, s_type, s_sys, "");
                    //取得監控項目編號
                    MV.sno = "-1";
                    //取得目前環境
                    MV.nowClass = s_class;
                    //取得目前分類
                    MV.nowType = s_type;
                    //取得目前系統別
                    MV.nowSys = s_sys;
                    //取得目前回報狀態
                    MV.nowStatus = s_status;
                    //取得目前系統負責人
                    MV.nowUser = s_user;
                    op_etime = DateTime.Now;

                    if (MD != null && MD.Count > 0)
                    {
                        MV.MonitorData = MD;
                        op_result = true;
                        op_a_count = MD.Count;
                        op_s_count = op_a_count;
                        op_msg = "[" + op_name + "]執行[" + op_action + "]成功,本次查詢共" + op_s_count.ToString() + "筆監控項目檢視資料";
                        SF.logandshowInfo(op_msg, log_Info);
                        return MV;
                    }
                    else
                    {
                        TempData["QueryMsg"] = "<script>alert('取得監控項目資料異常，請洽系統管理人員')</script>";
                        op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,本次查詢無資料";
                        SF.logandshowInfo(op_msg, log_Info);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    op_etime = DateTime.Now;
                    string MailSubject = null;
                    StringBuilder MailBody = null;
                    string nowclass = SF.getClassNameOrCode("5", s_class);

                    TempData["QueryMsg"] = "<script>alert('取得監控項目資料異常，請洽系統管理人員')</script>";
                    op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,查詢環境:[" + nowclass + "],錯誤訊息[" + ex.ToString() + "]";
                    SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢發生異常,請查詢Debug Log得到詳細資訊", log_Info);
                    SF.logandshowInfo(op_msg, log_Err);


                    //通知系統管理人員
                    MailSubject = "[異常]中控處理系統-" + op_name + "模組執行[" + op_action + "]失敗";
                    MailBody.Append("<table>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("[" + op_name + "]執行[" + op_action + "]失敗,詳細資訊如下");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("環境:[" + nowclass + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append(op_msg);
                    MailBody.Append("</td></tr>");
                    MailBody.Append("</table>");

                    SF.EmailNotify2Sys(Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver, true, MailSubject, MailBody.ToString());

                    return null;
                    //throw;
                }
                finally
                {
                    //寫入DB Log
                    OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                    SF.log2DB(OPLoger, MailServer, MailServerPort, MailSender, MailReceiver);
                }
            }
        }

        /// <summary>
        ///  取得監控項目檢視資料
        /// </summary>
        /// <param name="nowClass"></param>
        /// <param name="nowType"></param>
        /// <param name="nowSys"></param>
        /// <param name="nowSatus"></param>
        /// <param name="nowUser"></param>
        /// <returns></returns>
        private MonitorPropertyDataViewModel getMonitorPropertyData(string nowClass, string nowType, string nowSys, string sno, string nowSatus, string nowUser)
        {
            MonitorPropertyDataViewModel MV = new MonitorPropertyDataViewModel();

            using (CMSEntities CMS = new CMSEntities())
            {
                string s_class = nowClass;
                string s_type = nowType;
                string s_sys = nowSys;
                string s_status = nowSatus;
                string s_user = nowUser;
                string s_sno = sno;

                var MD = new List<v_Monitor_Data>();

                string op_action = null;
                DateTime op_stime = default(DateTime);
                DateTime op_etime = default(DateTime);
                int op_a_count = 0;
                int op_s_count = 0;
                int op_f_count = 0;
                string op_msg = string.Empty;
                bool op_result = false;

                string MailServer = Configer.MailServer;
                int MailServerPort = Configer.MailServerPort;
                string MailSender = Configer.MailSender;
                List<string> MailReceiver = Configer.MailReceiver;

                op_action = "取得監控項目檢視資料作業";

                try
                {
                    op_stime = DateTime.Now;
                    Expression<Func<v_Monitor_Data, bool>> MonitorDataClassWhereCondition;
                    if (s_class != "-1")
                    {
                        MonitorDataClassWhereCondition = b => b.s_class == s_class;
                    }
                    else
                    {
                        MonitorDataClassWhereCondition = b => 1 == 1;
                    }

                    Expression<Func<v_Monitor_Data, bool>> MonitorDataTypeWhereCondition;
                    if (s_type != "-1")
                    {
                        MonitorDataTypeWhereCondition = b => b.s_type == s_type;
                    }
                    else
                    {
                        MonitorDataTypeWhereCondition = b => 1 == 1;
                    }
                    Expression<Func<v_Monitor_Data, bool>> MonitorDataSysWhereCondition;
                    if (s_sys != "-1")
                    {
                        MonitorDataSysWhereCondition = b => b.s_sys == s_sys;
                    }
                    else
                    {
                        MonitorDataSysWhereCondition = b => 1 == 1;
                    }
                    Expression<Func<v_Monitor_Data, bool>> MonitorDataStatusWhereCondition;
                    if (s_status != "-1")
                    {
                        int Status = int.Parse(s_status);
                        var StatusList = CMS.v_StatusList.Where(c => c.id == Status).FirstOrDefault();

                        MonitorDataStatusWhereCondition = b => b.回報結果 == StatusList.value;
                    }
                    else
                    {
                        MonitorDataStatusWhereCondition = b => 1 == 1;
                    }
                    Expression<Func<v_Monitor_Data, bool>> MonitorDataAdminWhereCondition;
                    if (s_user != "-1")
                    {
                        MonitorDataAdminWhereCondition = b => b.系統負責人.Contains(s_user);
                    }
                    else
                    {
                        MonitorDataAdminWhereCondition = b => 1 == 1;
                    }
                    Expression<Func<v_Monitor_Data, bool>> MonitorDataSNOWhereCondition;
                    if (s_sno != "-1")
                    {
                        MonitorDataSNOWhereCondition = b => b.監控項目編號 == s_sno;
                    }
                    else
                    {
                        MonitorDataSNOWhereCondition = b => 1 == 1;
                    }

                    MD = CMS.v_Monitor_Data
                         .Where(MonitorDataClassWhereCondition)
                         .Where(MonitorDataTypeWhereCondition)
                         .Where(MonitorDataSysWhereCondition)
                         .Where(MonitorDataStatusWhereCondition)
                         .Where(MonitorDataAdminWhereCondition)
                         .Where(MonitorDataSNOWhereCondition)
                         .OrderBy(b => b.順序).ThenBy(b => b.監控項目編號).ToList();

                    string UserID = Session["UserID"].ToString();//TAS191;
                    //取得是否可以編輯監控項目權限
                    MV.caneditMonitProperty = SF.isedit(Convert.ToBoolean(Session["UseCertLogin"].ToString()), Convert.ToInt16(Session["UserRole"].ToString()), 11);
                    //取得目前使用者可以覆核的監控項目個數
                    MV.needReviewCount = CMS.TmpMonitorProperty
                       .Where(b => b.update_account != UserID).Count();
                    //取得環境清單
                    MV.ClassList = SF.getClassList(s_class);
                    //取得分類清單
                    MV.TypeList = SF.getTypeList(s_type);
                    //取得系統別清單
                    MV.SysList = SF.getSysList(s_type, s_sys);
                    //取得使用者清單
                    MV.UserList = SF.getUserList(s_user);
                    //取得回報狀態清單
                    MV.StatusList = SF.getStatusList(s_status);
                    //取得監控項目主旨清單
                    MV.SubjectList = SF.getSubjectList(s_class, s_type, s_sys, s_sno);
                    //取得監控項目編號
                    MV.sno = s_sno;
                    //取得目前環境
                    MV.nowClass = s_class;
                    //取得目前分類
                    MV.nowType = s_type;
                    //取得目前系統別
                    MV.nowSys = s_sys;
                    //取得目前回報狀態
                    MV.nowStatus = s_status;
                    //取得目前系統負責人
                    MV.nowUser = s_user;
                    op_etime = DateTime.Now;

                    if (MD != null)
                    {
                        if (MD.Count > 0)
                        {
                            MV.MonitorData = MD;
                            op_result = true;
                            op_a_count = MD.Count;
                            op_s_count = op_a_count;
                            op_msg = "[" + op_name + "]執行[" + op_action + "]成功,本次查詢共" + op_s_count.ToString() + "筆監控項目檢視資料";
                            SF.logandshowInfo(op_msg, log_Info);
                            return MV;
                        }
                        else
                        {
                            TempData["QueryMsg"] = "<script>alert('本次查詢無資料');</script>";
                            op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,本次查詢無資料";
                            SF.logandshowInfo(op_msg, log_Info);

                            return MV;
                        }
                    }
                    else
                    {
                        TempData["QueryMsg"] = "<script>alert('本次查詢無資料');</script>";
                        op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,本次查詢無資料";
                        SF.logandshowInfo(op_msg, log_Info);

                        return MV;
                    }
                }
                catch (Exception ex)
                {
                    op_etime = DateTime.Now;
                    string MailSubject = null;
                    StringBuilder MailBody = null;
                    string nowclass = SF.getClassNameOrCode("5", s_class);

                    TempData["QueryMsg"] = "<script>alert('本次查詢發生異常，請洽系統管理人員'):";
                    op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,查詢環境:[" + nowclass + "],錯誤訊息[" + ex.ToString() + "]";
                    SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢發生異常,請查詢Debug Log得到詳細資訊", log_Info);
                    SF.logandshowInfo(op_msg, log_Err);


                    //通知系統管理人員
                    MailSubject = "[異常]中控處理系統-" + op_name + "模組執行[" + op_action + "]失敗";
                    MailBody.Append("<table>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("[" + op_name + "]執行[" + op_action + "]失敗,詳細資訊如下");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("環境:[" + nowclass + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append(op_msg);
                    MailBody.Append("</td></tr>");
                    MailBody.Append("</table>");

                    SF.EmailNotify2Sys(Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver, true, MailSubject, MailBody.ToString());

                    return null;
                    //throw;
                }
                finally
                {
                    //寫入DB Log
                    OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                    SF.log2DB(OPLoger, MailServer, MailServerPort, MailSender, MailReceiver);
                }
            }
        }

        /// <summary>
        /// 取得監控項目按鈕id資訊
        /// </summary>
        /// <param name="_name">監控按鈕名稱</param>
        /// <returns>監控項目按鈕id資訊</returns>
        /// <remarks>1030506 黃富彥</remarks>
        private string getbtnidInfo(string _name)
        {
            string Ids = null;
            string name = _name;
            string op_action = null;
            DateTime op_stime = default(DateTime);
            DateTime op_etime = default(DateTime);
            int op_a_count = 0;
            int op_s_count = 0;
            int op_f_count = 0;
            string op_msg = string.Empty;
            bool op_result = false;

            op_action = "取得" + name + "監控項目按鈕id資訊";

            try
            {

                op_stime = DateTime.Now;
                using (CMSEntities CMS = new CMSEntities())
                {
                    Ids = CMS.v_btn_IdInfo_Result
                       .Single(b => b.value == name).id;
                }
                op_etime = DateTime.Now;

                if (Ids != null)
                {
                    op_a_count = 1;
                    op_s_count = op_a_count;
                    op_result = true;

                    SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]成功,本次共取得" + op_a_count.ToString() + "筆資料", log_Info);

                    //寫入DB Log
                    OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                    SF.log2DB(OPLoger, Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver);
                }
                else
                {

                }
            }
            catch (Exception ex)
            {

                string MailSubject = null;
                StringBuilder MailBody = null;
                op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,監控項目按鈕名稱[" + name + "],錯誤訊息:[" + ex.ToString() + "]";

                SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢發生異常,請查詢Debug Log得到詳細資訊", log_Info);
                SF.logandshowInfo(op_msg, log_Err);

                //通知系統管理人員
                MailSubject = "[異常]中控處理系統-" + op_name + "模組執行[" + op_action + "]失敗";
                MailBody.Append("<table>");
                MailBody.Append("<tr><td>");
                MailBody.Append("[" + op_name + "]執行[" + op_action + "]失敗,詳細資訊如下");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append("監控項目按鈕名稱:[" + name + "]");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append(op_msg);
                MailBody.Append("</td></tr>");
                MailBody.Append("</table>");

                SF.EmailNotify2Sys(Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver, true, MailSubject, MailBody.ToString());
            }
            finally
            {
                //寫入DB Log
                OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                SF.log2DB(OPLoger, Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver);
            }
            return Ids;
        }

        /// <summary>
        /// 檢查新增監控項目輸入
        /// </summary>
        /// <param name="TMP">輸入內容</param>
        /// <returns></returns>
        private bool CheckInput4Create(TmpMonitorProperty TMP)
        {
            if (TMP.s_no == "" || TMP.s_no == null)
            {
                TempData["CreateMsg"] = "<script>alert('請填寫監控項目編號')</script>";
                return false;
            }
            else
            {
                if (SF.CheckRepSNO(TMP.s_no, "MonitorProperty") == true)
                {
                    TempData["CreateMsg"] = "<script>alert('監控項目編號重複，請重新填寫')</script>";
                    return false;
                }
                else
                {
                    if (SF.CheckRepSNO(TMP.s_no, "TmpMonitorProperty") == true)
                    {
                        TempData["CreateMsg"] = "<script>alert('監控項目編號正等待覆核，請重新填寫')</script>";
                        return false;
                    }
                    else
                    {

                        if (TMP.s_class == "-1")
                        {
                            TempData["CreateMsg"] = "<script>alert('請選擇環境')</script>";
                            return false;
                        }
                        else
                        {
                            if (TMP.s_type == "-1")
                            {
                                TempData["CreateMsg"] = "<script>alert('請選擇分類')</script>";
                                return false;
                            }
                            else
                            {
                                if (TMP.s_sys == "-1")
                                {
                                    TempData["CreateMsg"] = "<script>alert('請選擇系統')</script>";
                                    return false;
                                }
                                else
                                {

                                    if (TMP.s_class != TMP.s_no.Substring(0, 1))
                                    {
                                        TempData["CreateMsg"] = "<script>alert('監控項目編號格式錯誤,第一碼應為:[" + TMP.s_class + "]')</script>";
                                        return false;
                                    }
                                    else
                                    {
                                        if (TMP.s_type != TMP.s_no.Substring(1, 1))
                                        {
                                            TempData["CreateMsg"] = "<script>alert('監控項目編號格式錯誤,第二碼應為:[" + TMP.s_type + "]')</script>";
                                            return false;
                                        }
                                        else
                                        {
                                            if (TMP.s_sys != TMP.s_no.Substring(2, 2))
                                            {
                                                TempData["CreateMsg"] = "<script>alert('監控項目編號格式錯誤,第三和第四碼應為:[" + TMP.s_sys + "]')</script>";
                                                return false;
                                            }
                                            else
                                            {
                                                if (TMP.s_subject == "" || TMP.s_subject == null)
                                                {
                                                    TempData["CreateMsg"] = "<script>alert('請填寫監控項目主旨')</script>";
                                                    return false;
                                                }
                                                else
                                                {
                                                    if (TMP.s_content == "" || TMP.s_content == null)
                                                    {
                                                        TempData["CreateMsg"] = "<script>alert('請填寫監控項目內容')</script>";
                                                        return false;
                                                    }
                                                    else
                                                    {
                                                        if (TMP.s_frequency < 0)
                                                        {
                                                            TempData["CreateMsg"] = "<script>alert('監控項目頻率需大於0')</script>";
                                                            return false;
                                                        }
                                                        else
                                                        {
                                                            if (TMP.s_timeout < 0)
                                                            {
                                                                TempData["CreateMsg"] = "<script>alert('監控項目容忍時間需大於0')</script>";
                                                                return false;
                                                            }
                                                            else
                                                            {
                                                                if (TMP.sysadmin == "")
                                                                {
                                                                    TempData["CreateMsg"] = "<script>alert('請填寫系統管理人員帳號')</script>";
                                                                    return false;
                                                                }
                                                                else
                                                                {
                                                                    bool TmpResult = true;

                                                                    if (TMP.w_sendmail == true)
                                                                    {
                                                                        if (TMP.w_mail_notify_limit < 0 || TMP.w_mail_notify_limit == null)
                                                                        {
                                                                            TempData["CreateMsg"] = "<script>alert('啟用Warn Email通知時,通知次數上限應大於0')</script>";
                                                                            TmpResult = false;
                                                                        }
                                                                    }

                                                                    if (TMP.w_sendmessage == true)
                                                                    {
                                                                        if (TMP.w_message_notify_limit < 0 || TMP.w_message_notify_limit == null)
                                                                        {
                                                                            TempData["CreateMsg"] = "<script>alert('啟用Warn 簡訊通知時,通知次數上限應大於0')</script>";
                                                                            TmpResult = false;
                                                                        }
                                                                    }

                                                                    if (TMP.e_sendmessage == true)
                                                                    {
                                                                        if (TMP.e_message_notify_limit < 0 || TMP.e_message_notify_limit == null)
                                                                        {
                                                                            TempData["CreateMsg"] = "<script>alert('啟用Error 簡訊通知時,通知次數上限應大於0')</script>";
                                                                            TmpResult = false;
                                                                        }
                                                                    }

                                                                    if (TMP.e_sendmessage == true)
                                                                    {
                                                                        if (TMP.e_message_notify_limit < 0 || TMP.e_message_notify_limit == null)
                                                                        {
                                                                            TempData["CreateMsg"] = "<script>alert('啟用Error 簡訊通知時,通知次數上限應大於0')</script>";
                                                                            TmpResult = false;
                                                                        }
                                                                    }

                                                                    if (TMP.e_sendmessage == true)
                                                                    {
                                                                        if (TMP.e_message_notify_limit < 0 || TMP.e_message_notify_limit == null)
                                                                        {
                                                                            TempData["CreateMsg"] = "<script>alert('啟用Error 簡訊通知時,通知次數上限應大於0')</script>";
                                                                            TmpResult = false;
                                                                        }
                                                                    }

                                                                    if (TMP.f_sendmessage == true)
                                                                    {
                                                                        if (TMP.f_message_notify_limit < 0 || TMP.f_message_notify_limit == null)
                                                                        {
                                                                            TempData["CreateMsg"] = "<script>alert('啟用Error 簡訊通知時,通知次數上限應大於0')</script>";
                                                                            TmpResult = false;
                                                                        }
                                                                    }

                                                                    return TmpResult;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 檢查修改監控項目輸入
        /// </summary>
        /// <param name="TMP">輸入內容</param>
        /// <returns></returns>
        private bool CheckInput4Edit(TmpMonitorProperty TMP)
        {
            if (TMP.s_no == "" || TMP.s_no == null)
            {
                TempData["EditMsg"] = "<script>alert('請填寫監控項目編號')</script>";
                return false;
            }
            else
            {
                if (SF.CheckRepSNO(TMP.s_no, "TmpMonitorProperty") == true)
                {
                    TempData["EditMsg"] = "<script>alert('監控項目編號正等待覆核，請重新填寫')</script>";
                    return false;
                }
                else
                {

                    if (TMP.s_class == "-1")
                    {
                        TempData["EditMsg"] = "<script>alert('請選擇環境')</script>";
                        return false;
                    }
                    else
                    {
                        if (TMP.s_type == "-1")
                        {
                            TempData["EditMsg"] = "<script>alert('請選擇分類')</script>";
                            return false;
                        }
                        else
                        {
                            if (TMP.s_sys == "-1")
                            {
                                TempData["EditMsg"] = "<script>alert('請選擇系統')</script>";
                                return false;
                            }
                            else
                            {

                                if (TMP.s_class != TMP.s_no.Substring(0, 1))
                                {
                                    TempData["EditMsg"] = "<script>alert('監控項目編號格式錯誤,第一碼應為:[" + TMP.s_class + "]')</script>";
                                    return false;
                                }
                                else
                                {
                                    if (TMP.s_type != TMP.s_no.Substring(1, 1))
                                    {
                                        TempData["EditMsg"] = "<script>alert('監控項目編號格式錯誤,第二碼應為:[" + TMP.s_type + "]')</script>";
                                        return false;
                                    }
                                    else
                                    {
                                        if (TMP.s_sys != TMP.s_no.Substring(2, 2))
                                        {
                                            TempData["EditMsg"] = "<script>alert('監控項目編號格式錯誤,第三和第四碼應為:[" + TMP.s_sys + "]')</script>";
                                            return false;
                                        }
                                        else
                                        {
                                            if (TMP.s_subject == "" || TMP.s_subject == null)
                                            {
                                                TempData["EditMsg"] = "<script>alert('請填寫監控項目主旨')</script>";
                                                return false;
                                            }
                                            else
                                            {
                                                if (TMP.s_content == "" || TMP.s_content == null)
                                                {
                                                    TempData["EditMsg"] = "<script>alert('請填寫監控項目內容')</script>";
                                                    return false;
                                                }
                                                else
                                                {
                                                    if (TMP.s_frequency < 0)
                                                    {
                                                        TempData["EditMsg"] = "<script>alert('監控項目頻率需大於0')</script>";
                                                        return false;
                                                    }
                                                    else
                                                    {
                                                        if (TMP.s_timeout < 0)
                                                        {
                                                            TempData["EditMsg"] = "<script>alert('監控項目容忍時間需大於0')</script>";
                                                            return false;
                                                        }
                                                        else
                                                        {
                                                            if (TMP.sysadmin == "")
                                                            {
                                                                TempData["EditMsg"] = "<script>alert('請填寫系統管理人員帳號')</script>";
                                                                return false;
                                                            }
                                                            else
                                                            {
                                                                bool TmpResult = true;

                                                                if (TMP.w_sendmail == true)
                                                                {
                                                                    if (TMP.w_mail_notify_limit < 0 || TMP.w_mail_notify_limit == null)
                                                                    {
                                                                        TempData["EditMsg"] = "<script>alert('啟用Warn Email通知時,通知次數上限應大於0')</script>";
                                                                        TmpResult = false;
                                                                    }
                                                                }

                                                                if (TMP.w_sendmessage == true)
                                                                {
                                                                    if (TMP.w_message_notify_limit < 0 || TMP.w_message_notify_limit == null)
                                                                    {
                                                                        TempData["EditMsg"] = "<script>alert('啟用Warn 簡訊通知時,通知次數上限應大於0')</script>";
                                                                        TmpResult = false;
                                                                    }
                                                                }

                                                                if (TMP.e_sendmessage == true)
                                                                {
                                                                    if (TMP.e_message_notify_limit < 0 || TMP.e_message_notify_limit == null)
                                                                    {
                                                                        TempData["EditMsg"] = "<script>alert('啟用Error 簡訊通知時,通知次數上限應大於0')</script>";
                                                                        TmpResult = false;
                                                                    }
                                                                }

                                                                if (TMP.e_sendmessage == true)
                                                                {
                                                                    if (TMP.e_message_notify_limit < 0 || TMP.e_message_notify_limit == null)
                                                                    {
                                                                        TempData["EditMsg"] = "<script>alert('啟用Error 簡訊通知時,通知次數上限應大於0')</script>";
                                                                        TmpResult = false;
                                                                    }
                                                                }

                                                                if (TMP.e_sendmessage == true)
                                                                {
                                                                    if (TMP.e_message_notify_limit < 0 || TMP.e_message_notify_limit == null)
                                                                    {
                                                                        TempData["EditMsg"] = "<script>alert('啟用Error 簡訊通知時,通知次數上限應大於0')</script>";
                                                                        TmpResult = false;
                                                                    }
                                                                }

                                                                if (TMP.f_sendmessage == true)
                                                                {
                                                                    if (TMP.f_message_notify_limit < 0 || TMP.f_message_notify_limit == null)
                                                                    {
                                                                        TempData["EditMsg"] = "<script>alert('啟用Error 簡訊通知時,通知次數上限應大於0')</script>";
                                                                        TmpResult = false;
                                                                    }
                                                                }

                                                                return TmpResult;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 取得系統別選單
        /// </summary>
        /// <param name="t_id">分類編號</param>
        /// <returns></returns>
        public JsonResult getSysList(string t_id)
        {
            using (CMSEntities CMS = new CMSEntities())
            {
                Expression<Func<v_SysList, bool>> TypeWhereCondition;
                if (t_id != "-1")
                {
                    TypeWhereCondition = b => b.t_id == t_id;
                }
                else
                {
                    TypeWhereCondition = b => 1 == 1;
                }

                var SysList = CMS.v_SysList
                    .Where(TypeWhereCondition).ToList();

                if (SysList != null)
                {
                    return Json(SysList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 取得監控項目主旨選單
        /// </summary>
        /// <param name="t_id">分類編號</param>
        /// <returns></returns>
        public JsonResult getSubjectList(string nowClass, string nowType, string nowSys)
        {
            using (CMSEntities CMS = new CMSEntities())
            {
                var SubjectList = CMS.v_SubjectList
                                    .Where(b => b.s_class == nowClass)
                                    .Where(b => b.s_type == nowType)
                                    .Where(b => b.s_sys == nowSys).ToList();

                if (SubjectList != null)
                {
                    return Json(SubjectList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                }
            }

        }

        /// <summary>
        /// 取得下一號的監控項目編號
        /// </summary>
        /// <param name="s_class">環境</param>
        /// <param name="s_type">分類</param>
        /// <param name="s_sys">系統</param>
        /// <returns></returns>
        private string getNextSno(string s_class, string s_type, string s_sys)
        {
            string Result = "";

            using (CMSEntities CMS = new CMSEntities())
            {
                var NowSno = CMS.MonitorProperty
                                  .Where(b => b.s_class == s_class)
                                  .Where(b => b.s_type == s_type)
                                  .Where(b => b.s_sys == s_sys)
                                  .OrderByDescending(b => b.s_no)
                                  .First();

                if (NowSno != null)
                {
                    int NowSeq = Convert.ToInt16(NowSno.s_no.Substring(6, 4)) + 1;
                    Result = s_class + s_type + s_sys + "00" + NowSeq.ToString().PadLeft(4, '0');
                }

                return Result;
            }
        }

        /// <summary>
        /// 同步/檢查 檢測工具參數至檢測工具
        /// </summary>
        /// <param name="_Mode">執行模式(Sync:同步;Check:檢查)</param>
        /// <param name="_s_no">監控項目編號</param>
        /// <param name="_params_host">檢測工具接收窗口</param>
        /// <param name="_MonitorToolParams">檢測工具參數</param>
        /// <returns>執行結果陣列</returns>
        /// <remarks>1030521 黃富彥</remarks>
        private List<string> sync2MonitorTool(string _Mode, string _s_no, string _params_host, string _MonitorToolParams)
        {
            string Mode = _Mode;
            string s_no = _s_no;
            string params_host = _params_host;
            string MonitorToolParams = _MonitorToolParams;
            List<string> SyncResultList = new List<string>();
            SyncAndCheck SC = new SyncAndCheck();

            SC.ConnStr = Configer.C_DBConnstring;
            SC.op_name = op_name;

            SC.setSyncAndCheck(Mode, params_host, MonitorToolParams);
            SyncResultList = SC.doPost(Configer);

            return SyncResultList;

        }
    }
}