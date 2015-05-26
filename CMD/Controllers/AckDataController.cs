using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Text;
using CMD.Models;
using CMD.SystemClass;
using CMD.ViewModels;
using TWCAlib;


namespace CMD.Controllers
{
    public class AckDataController : Controller
    {
        SystemConfig Configer = new SystemConfig();
        OPLog OPLoger = new OPLog();
        ShareFunc SF = new ShareFunc();
        String log_Info = "Info";
        String log_Err = "Err";
        String op_name = "顯示中控台";
        
        // GET: AckData
        public ActionResult Index()
        {
            //初始化系統參數
            Configer.Init();

            //共用涵式用
            SF.ConnStr = Configer.C_DBConnstring;
            SF.op_name = op_name;


            //if (sclass == null || sclass == "")
            //{
            //    sclass = "-1";
            //}

            //if (stype == null || stype == "")
            //{
            //    stype = "-1";
            //}

            //if (sys == null || sys == "")
            //{
            //    sys = "-1";
            //}

            //初始化回傳物件
            AckDataViewModel MV = getAckData("-1", "-1", "-1");
            if (MV != null)
            {
                if (MV ==null || MV.AckData.Count() <= 0)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return View(MV);
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult Index(QueryAckData QMD)
        {
            //初始化系統參數
            Configer.Init();

            //共用涵式用
            SF.ConnStr = Configer.C_DBConnstring;
            SF.op_name = op_name;

            //初始化回傳物件
            AckDataViewModel MV = getAckData(QMD.nowClass, QMD.nowType, QMD.nowSys);

            if (MV != null)
            {
                if (MV.AckData.Count() <= 0)
                {
                    return RedirectToAction("Index", "AckData");
                }
                else
                {
                    return View(MV);
                }
            }
            else
            {
                return RedirectToAction("Index", "AckData");
            }
        }

        //public string Ack(string AckList)
        //{
        //    //初始化系統參數
        //    Configer.Init();

        //    //共用涵式用
        //    SF.ConnStr = Configer.C_DBConnstring;
        //    SF.op_name = op_name;

        //    string op_action = null;
        //    DateTime op_stime = default(DateTime);
        //    DateTime op_etime = default(DateTime);
        //    int op_a_count = 0;
        //    int op_s_count = 0;
        //    int op_f_count = 0;
        //    string op_msg = string.Empty;
        //    bool op_result = false;

        //    string MailServer = Configer.MailServer;
        //    int MailServerPort = Configer.MailServerPort;
        //    string MailSender = Configer.MailSender;
        //    List<string> MailReceiver = Configer.MailReceiver;

        //    op_action = "監控項目復歸作業";

        //    try
        //    {

        //    }
        //    catch (Exception ex)
        //    {
                
        //        throw;
        //    }
        //}

        private AckDataViewModel getAckData(string s_class, string s_type, string s_sys)
        {
            AckDataViewModel MV = new AckDataViewModel();
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

            op_action = "取得待復歸資料作業";

            try
            {
                op_stime = DateTime.Now;
                using (CMSEntities CMS = new CMSEntities())
                {
                    Expression<Func<v_ACK_Data, bool>> AckDataClassWhereCondition;
                    if (s_class != "-1")
                    {
                        AckDataClassWhereCondition = b => b.s_class == s_class;
                    }
                    else
                    {
                        AckDataClassWhereCondition = b => 1 == 1;
                    }

                    Expression<Func<v_ACK_Data, bool>> AckDataTypeWhereCondition;
                    if (s_type != "-1")
                    {
                        AckDataTypeWhereCondition = b => b.s_type == s_type;
                    }
                    else
                    {
                        AckDataTypeWhereCondition = b => 1 == 1;
                    }
                    Expression<Func<v_ACK_Data, bool>> AckDataSysWhereCondition;
                    if (s_sys != "-1")
                    {
                        AckDataSysWhereCondition = b => b.s_sys == s_sys;
                    }
                    else
                    {
                        AckDataSysWhereCondition = b => 1 == 1;
                    }

                    var ackdatalist = CMS.v_ACK_Data
                                         .Where(AckDataClassWhereCondition)
                                         .Where(AckDataTypeWhereCondition)
                                         .Where(AckDataSysWhereCondition)
                                         .OrderBy(b => b.監控項目編號)
                                         .OrderBy(b => b.監控項目主旨)
                                         .OrderBy(b => b.回報狀態)
                                         .OrderBy(b => b.異常發生時間).ToList();

                    op_etime = DateTime.Now;
                    //取得環境清單
                    MV.ClassList = SF.getClassList(s_class);
                    //取得分類清單
                    MV.TypeList = SF.getTypeList(s_type);
                    //取得系統別清單
                    MV.SysList = SF.getSysList(s_type, s_sys);
                    //取得復歸資料清單
                    MV.AckData = ackdatalist;
                    if (ackdatalist != null && ackdatalist.Count>0)
                    {
                        op_result = true;
                        op_a_count = ackdatalist.Count;
                        op_s_count = op_a_count;
                        op_msg = "[" + op_name + "]執行[" + op_action + "]成功,本次查詢共" + op_s_count.ToString() + "筆待復歸檢視資料";
                        SF.logandshowInfo(op_msg, log_Info);
                        return MV;
                    }
                    else {
                        op_result = true;
                        TempData["QueryMsg"] = "<script>alert('目前無待復歸資料')</script>";
                        op_msg = "[" + op_name + "]執行[" + op_action + "]成功,本次查詢無資料";
                        SF.logandshowInfo(op_msg, log_Info);
                        return MV;
                    }
                }
            }
            catch (Exception ex)
            {

                string MailSubject = null;
                StringBuilder MailBody = null;
                //string nowclass = SF.getClassNameOrCode("5", s_class);

                TempData["QueryMsg"] = "<script>alert('取得待復歸資料異常，請洽系統管理人員')</script>";
                op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,查詢環境:[" + s_class + "],查詢分類:["+ s_type +"],查詢系統:["+ s_sys +"],錯誤訊息[" + ex.ToString() + "]";
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

                return null;
            }
            finally
            {
                //寫入DB Log
                OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                SF.log2DB(OPLoger, MailServer, MailServerPort, MailSender, MailReceiver);
            }
        }

        /// <summary>
        /// 取得復歸原因選單
        /// </summary>
        /// <param name="t_id">分類編號</param>
        /// <returns></returns>
        public JsonResult getAckReasonList(int a_id)
        {
            using (CMSEntities CMS = new CMSEntities())
            {
                Expression<Func<v_ACKREASONList, bool>> TypeWhereCondition;
                if (a_id != -1)
                {
                    TypeWhereCondition = b => b.a_id == a_id;
                }
                else
                {
                    TypeWhereCondition = b => 1 == 1;
                }

                var AckReasonList = CMS.v_ACKREASONList
                    .Where(TypeWhereCondition).ToList();

                if (AckReasonList != null)
                {
                    return Json(AckReasonList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}