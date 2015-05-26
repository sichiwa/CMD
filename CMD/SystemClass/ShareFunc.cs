using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using TWCAlib;
using NLog;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Net;

namespace CMD.SystemClass
{
    public class ShareFunc
    {
        public static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private string _ConnStr;

        private string _op_name;
        private string log_Info = "Info";
        private string log_Err = "Err";
        public enum MailPriority : int
        {
            Low = 0,
            Middle = 1,
            High = 2
        }

        /// <summary>
        /// 資料庫連線字串
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ConnStr
        {
            get { return _ConnStr; }
            set { _ConnStr = value; }
        }

        /// <summary>
        /// 處理模組名稱
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string op_name
        {
            get { return _op_name; }
            set { _op_name = value; }
        }

        /// <summary>
        /// 將資訊記錄至Log檔中
        /// </summary>
        /// <param name="_Str">顯示資訊</param>
        /// <param name="_Mode">Err:記錄至Debug log;Info記錄至Info log:</param>
        /// <remarks>2014/03/04 黃富彥</remarks>
        public void logandshowInfo(string _Str, string _Mode)
        {
            if ((_Mode == "Err"))
            {
                logger.Error(_Str);
            }
            else
            {
                logger.Info(_Str);
            }
        }

        /// <summary>
        /// 將執行結果寫入資料庫
        /// </summary>
        /// <param name="_OPLogger">OPLoger類別</param>
        /// <remarks>2014/03/04 黃富彥</remarks>
        public void log2DB(OPLog _OPLogger, string _MailServer, int _MailServerPort, string _MailSender, List<string> _MailReceiver)
        {
            OPLog OPLoger = _OPLogger;
            SqlCommand SqlComm = new SqlCommand();
            StringBuilder SqlStr = new StringBuilder();
            DataTable ResultDT = new DataTable();

            string MailServer = _MailServer;
            int MailServerPort = _MailServerPort;
            string MailSender = _MailSender;
            List<string> MailReceiver = _MailReceiver;
            string MailSubject = string.Empty;
            StringBuilder MailBody = new StringBuilder();
            string SendResult = string.Empty;

            try
            {
                SqlStr.Append("exec MDRSYS.usp_insert2SYSTEMLOG @sys_name,@sys_action,@sys_stime,@sys_etime,@sys_a_count,@sys_s_count,@sys_f_count,@sys_msg,@sys_result;");

                SqlParameter Pop_name = new SqlParameter();
                Pop_name.ParameterName = "@sys_name";
                Pop_name.DbType = DbType.String;
                Pop_name.Size = 50;
                Pop_name.Value = OPLoger.op_name;
                SqlComm.Parameters.Add(Pop_name);

                SqlParameter Pop_action = new SqlParameter();
                Pop_action.ParameterName = "@sys_action";
                Pop_action.DbType = DbType.String;
                Pop_action.Size = 100;
                Pop_action.Value = OPLoger.op_action;
                SqlComm.Parameters.Add(Pop_action);

                SqlParameter Pop_stime = new SqlParameter();
                Pop_stime.ParameterName = "@sys_stime";
                Pop_stime.DbType = DbType.DateTime;
                Pop_stime.Value = OPLoger.op_stime;
                SqlComm.Parameters.Add(Pop_stime);

                SqlParameter Pop_etime = new SqlParameter();
                Pop_etime.ParameterName = "@sys_etime";
                Pop_etime.DbType = DbType.DateTime;
                Pop_etime.Value = OPLoger.op_etime;
                SqlComm.Parameters.Add(Pop_etime);

                SqlParameter Pop_a_count = new SqlParameter();
                Pop_a_count.ParameterName = "@sys_a_count";
                Pop_a_count.DbType = DbType.Int64;
                Pop_a_count.Value = OPLoger.op_a_count;
                SqlComm.Parameters.Add(Pop_a_count);

                SqlParameter Pop_s_count = new SqlParameter();
                Pop_s_count.ParameterName = "@sys_s_count";
                Pop_s_count.DbType = DbType.Int64;
                Pop_s_count.Value = OPLoger.op_s_count;
                SqlComm.Parameters.Add(Pop_s_count);

                SqlParameter Pop_f_count = new SqlParameter();
                Pop_f_count.ParameterName = "@sys_f_count";
                Pop_f_count.DbType = DbType.Int64;
                Pop_f_count.Value = OPLoger.op_f_count;
                SqlComm.Parameters.Add(Pop_f_count);

                SqlParameter Pop_msg = new SqlParameter();
                Pop_msg.ParameterName = "@sys_msg";
                Pop_msg.DbType = DbType.String;
                Pop_msg.Size = 4000;
                Pop_msg.Value = OPLoger.op_msg;
                SqlComm.Parameters.Add(Pop_msg);

                SqlParameter Pop_result = new SqlParameter();
                Pop_result.ParameterName = "@sys_result";
                Pop_result.DbType = DbType.Boolean;
                Pop_result.Value = OPLoger.op_result;
                SqlComm.Parameters.Add(Pop_result);

                SqlComm.CommandText = SqlStr.ToString();

                using (SqlComm)
                {
                    ResultDT = DataProcessor.SelectDataTable(this.ConnStr, SqlComm);
                }

                if (ResultDT.Rows[0]["result"].ToString() == "0")
                {
                    //寫入文字檔Log
                    logandshowInfo("[" + op_name + "]執行[寫入資料庫紀錄作業]成功", log_Info);
                }
                else
                {
                    //寫入文字檔Log
                    logandshowInfo("[" + this.op_name + "]執行[寫入資料庫紀錄作業]失敗,請查詢Debug Log得到詳細資訊", log_Info);
                    logandshowInfo("[" + this.op_name + "]執行[寫入資料庫紀錄作業]失敗,詳細資訊如下", log_Err);
                    logandshowInfo("執行SQL:[" + SqlStr.ToString() + "]", log_Err);
                    logandshowInfo("處理模組名稱:[" + OPLoger.op_name + "]", log_Err);
                    logandshowInfo("執行作業名稱:[" + OPLoger.op_action + "]", log_Err);
                    logandshowInfo("處理結果:[" + OPLoger.op_result.ToString() + "]", log_Err);
                    logandshowInfo("起始時間:[" + OPLoger.op_stime.ToString() + "]", log_Err);
                    logandshowInfo("結束時間:[" + OPLoger.op_etime.ToString() + "]", log_Err);
                    logandshowInfo("處理總筆數:[" + OPLoger.op_a_count.ToString() + "]", log_Err);
                    logandshowInfo("處理成功筆數:[" + OPLoger.op_s_count.ToString() + "]", log_Err);
                    logandshowInfo("處理失敗筆數:[" + OPLoger.op_f_count.ToString() + "]", log_Err);
                    logandshowInfo("作業訊息:[" + OPLoger.op_msg + "]", log_Err);
                    logandshowInfo("錯誤訊息:[" + ResultDT.Rows[0]["InsertMsg"].ToString() + "]", log_Err);

                    //通知系統管理人員
                    MailSubject = "[異常]中控處理系統-顯示中控台模組執行[寫入資料庫紀錄作業]失敗";
                    MailBody.Append("<table>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("[" + this.op_name + "]執行[寫入系統紀錄檔作業]失敗,詳細資訊如下");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("執行SQL:[" + SqlStr.ToString() + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("處理模組名稱:[" + OPLoger.op_name + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("執行作業名稱:[" + OPLoger.op_action + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("處理結果:[" + OPLoger.op_result.ToString() + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("起始時間:[" + OPLoger.op_stime.ToString() + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("結束時間:[" + OPLoger.op_etime.ToString() + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("處理總筆數:[" + OPLoger.op_a_count.ToString() + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("處理成功筆數:[" + OPLoger.op_s_count.ToString() + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("處理失敗筆數:[" + OPLoger.op_f_count.ToString() + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("作業訊息:[" + OPLoger.op_msg + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("錯誤訊息:[" + ResultDT.Rows[0]["InsertMsg"].ToString() + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("</table>");

                    EmailNotify2Sys(MailServer, MailServerPort, MailSender, MailReceiver, false, MailSubject, MailBody.ToString());
                }

            }
            catch (Exception ex)
            {
                //異常
                //寫入文字檔Log
                logandshowInfo("[" + this.op_name + "]執行[寫入資料庫紀錄作業]發生未預期的異常,請查詢Debug Log得到詳細資訊", log_Info);
                logandshowInfo("[" + this.op_name + "]執行[寫入資料庫紀錄作業]發生未預期的異常,詳細資訊如下", log_Err);
                logandshowInfo("執行SQL:[" + SqlStr.ToString() + "]", log_Err);
                logandshowInfo("處理模組名稱:[" + OPLoger.op_name + "]", log_Err);
                logandshowInfo("執行作業名稱:[" + OPLoger.op_action + "]", log_Err);
                logandshowInfo("處理結果:[" + OPLoger.op_result.ToString() + "]", log_Err);
                logandshowInfo("起始時間:[" + OPLoger.op_stime.ToString() + "]", log_Err);
                logandshowInfo("結束時間:[" + OPLoger.op_etime.ToString() + "]", log_Err);
                logandshowInfo("處理總筆數:[" + OPLoger.op_a_count.ToString() + "]", log_Err);
                logandshowInfo("處理成功筆數:[" + OPLoger.op_s_count.ToString() + "]", log_Err);
                logandshowInfo("處理失敗筆數:[" + OPLoger.op_f_count.ToString() + "]", log_Err);
                logandshowInfo("作業訊息:[" + OPLoger.op_msg + "]", log_Err);
                logandshowInfo("錯誤訊息:[" + ex.ToString() + "]", log_Err);

                //通知系統管理人員
                MailSubject = "[異常]中控處理系統-顯示中控台模組執行[寫入資料庫紀錄作業]失敗";
                MailBody.Append("<table>");
                MailBody.Append("<tr><td>");
                MailBody.Append("[" + this.op_name + "]執行[寫入系統紀錄檔作業]發生未預期的異常,詳細資訊如下");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append("執行SQL:[" + SqlStr.ToString() + "]");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append("處理模組名稱:[" + OPLoger.op_name + "]");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append("執行作業名稱:[" + OPLoger.op_action + "]");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append("處理結果:[" + OPLoger.op_result.ToString() + "]");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append("起始時間:[" + OPLoger.op_stime.ToString() + "]");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append("結束時間:[" + OPLoger.op_etime.ToString() + "]");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append("處理總筆數:[" + OPLoger.op_a_count.ToString() + "]");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append("處理成功筆數:[" + OPLoger.op_s_count.ToString() + "]");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append("處理失敗筆數:[" + OPLoger.op_f_count.ToString() + "]");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append("作業訊息:[" + OPLoger.op_msg + "]");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append("錯誤訊息:[" + ex.ToString() + "]");
                MailBody.Append("</td></tr>");
                MailBody.Append("</table>");

                EmailNotify2Sys(MailServer, MailServerPort, MailSender, MailReceiver, false, MailSubject, MailBody.ToString());
            }

        }

        /// <summary>
        /// 寄送郵件
        /// </summary>
        /// <param name="_MailServer">郵件主機位置</param>
        /// <param name="_MailServerPort">郵件主機服務Port</param>
        /// <param name="_MailSender">寄件者</param>
        /// <param name="_MailReceivers">收件者清單</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string SendEmail(string _MailServer, int _MailServerPort, string _MailSender, List<string> _MailReceivers, string _Subject, string _Body)
        {
            string SendResult = null;
            MailProcessor MailP = new MailProcessor();
            string MailServer = _MailServer;
            int MailServerPort = _MailServerPort;
            string MailSender = _MailSender;
            List<string> MailReceivers = _MailReceivers;
            string MailSubject = _Subject;
            string SendBody = _Body;
            List<System.Net.Mail.Attachment> MailA = new List<System.Net.Mail.Attachment>();
            List<string> MailCC = new List<string>();

            MailP.setMailProcossor(MailSender, MailReceivers, MailCC, MailSubject, SendBody, MailA, MailServer, MailServerPort, false, true,
            MailPriority.High.ToString(), 65001);

            SendResult = MailP.Send();

            return SendResult;
        }

        /// <summary>
        /// Email通知系統管理人員
        /// </summary>
        /// <param name="_WitreDB">是否要將通知結果寫入資料庫</param>
        /// <param name="_MailSubject">郵件主旨</param>
        /// <param name="_MailBody">郵件內容</param>
        /// <remarks></remarks>
        public void EmailNotify2Sys(string _MailServer, int _MailServerPort, string _MailSender, List<string> _MailReceiver, bool _WitreDB, string _MailSubject, string _MailBody)
        {
            OPLog OPLoger = new OPLog();
            string op_action = null;
            DateTime op_stime = default(DateTime);
            DateTime op_etime = default(DateTime);
            int op_a_count = 0;
            int op_s_count = 0;
            int op_f_count = 0;
            string op_msg = string.Empty;
            bool op_result = false;

            bool WitreDB = _WitreDB;

            string MailServer = _MailServer;
            int MailServerPort = _MailServerPort;
            string MailSender = _MailSender;
            List<string> MailReceiver = _MailReceiver;
            string MailSubject = _MailSubject;
            string MailBody = _MailBody;
            string SendResult = string.Empty;

            op_action = "通知系統管理人員作業";
            op_a_count = 1;

            //寄送通知信給系統管理人員
            op_stime = DateTime.Now;
            SendResult = SendEmail(MailServer, MailServerPort, MailSender, MailReceiver, MailSubject, MailBody.ToString());
            op_etime = DateTime.Now;

            if (SendResult == "success")
            {
                //寫入文字檔Log
                logandshowInfo("[" + this.op_name + "]執行[" + op_action + "]成功", log_Info);

                op_s_count = 1;
                op_result = true;
            }
            else
            {
                //寫入文字檔Log
                logandshowInfo("[" + this.op_name + "]執行[通知系統管理人員作業]失敗,請查詢Debug Log得到詳細資訊", log_Info);
                logandshowInfo("[" + this.op_name + "]執行[通知系統管理人員作業]失敗,詳細資訊如下", log_Err);
                logandshowInfo("錯誤訊息:[" + SendResult + "]", log_Err);

                op_msg = SendResult;
                op_f_count = 1;
            }

            if (WitreDB == true)
            {
                //寫入DB Log
                OPLoger.SetOPLog(this.op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                log2DB(OPLoger, MailServer, MailServerPort, MailSender, MailReceiver);
            }
        }

        /// <summary>
        /// 環境代碼轉換
        /// </summary>
        /// <param name="_Mode">轉換模式(0:英文代碼轉中文;1:英文代碼轉系統代碼;2:中文代碼轉英文;3:中文代碼轉系統代碼;4:系統代碼代碼轉英文;5:系統代碼代碼轉中文)</param>
        /// <param name="_value">待轉換值</param>
        /// <returns>轉換後結果</returns>
        /// <remarks>1030506 黃富彥</remarks>
        public string getClassNameOrCode(string _Mode, string _value)
        {
            string Mode = _Mode;
            string value = _value;
            string Result = string.Empty;

            switch (Mode)
            {
                case "0":
                    //英文代碼轉中文
                    switch (value)
                    {
                        case "Prod":
                            Result = "正式";
                            break;
                        case "Test":
                            Result = "測試";
                            break;
                        case "Backup":
                            Result = "異地";
                            break;
                        case "Verify":
                            Result = "驗證";
                            break;
                    }
                    break;
                case "1":
                    //英文代碼轉系統代碼
                    switch (value)
                    {
                        case "Prod":
                            Result = "0";
                            break;
                        case "Test":
                            Result = "1";
                            break;
                        case "Backup":
                            Result = "2";
                            break;
                        case "Verify":
                            Result = "3";
                            break;
                    }
                    break;
                case "2":
                    //中文代碼轉英文
                    switch (value)
                    {
                        case "正式":
                            Result = "Prod";
                            break;
                        case "測試":
                            Result = "Test";
                            break;
                        case "異地":
                            Result = "Backup";
                            break;
                        case "驗證":
                            Result = "Varify";
                            break;
                    }
                    break;
                case "3":
                    //中文代碼轉系統代碼
                    switch (value)
                    {
                        case "正式":
                            Result = "0";
                            break;
                        case "測試":
                            Result = "1";
                            break;
                        case "異地":
                            Result = "2";
                            break;
                        case "驗證":
                            Result = "3";
                            break;
                    }
                    break;
                case "4":
                    //系統代碼代碼轉英文
                    switch (value)
                    {
                        case "0":
                            Result = "Prod";
                            break;
                        case "1":
                            Result = "Test";
                            break;
                        case "2":
                            Result = "Backup";
                            break;
                        case "3":
                            Result = "Verify";
                            break;
                    }
                    break;
                case "5":
                    //系統代碼代碼轉中文
                    switch (value)
                    {
                        case "0":
                            Result = "正式";
                            break;
                        case "1":
                            Result = "測試";
                            break;
                        case "2":
                            Result = "異地";
                            break;
                        case "3":
                            Result = "驗證";
                            break;
                    }
                    break;
            }

            return Result;
        }

        public SelectList getClassList(string nowClass)
        {
            using (CMSEntities CMS = new CMSEntities())
            {
                var ClassListCollection = CMS.v_ClassList
                                             .ToList();

                SelectList ClassList = new SelectList(ClassListCollection, "id", "value", nowClass);

                return ClassList;
            }

        }

        public SelectList getTypeList(string nowType)
        {
            using (CMSEntities CMS = new CMSEntities())
            {
                var TypeListCollection = CMS.v_TypeList
                                             .ToList();

                SelectList TypeList = new SelectList(TypeListCollection, "id", "value", nowType);

                return TypeList;
            }
        }

        public SelectList getSysList(string nowType, string nowSys)
        {
            using (CMSEntities CMS = new CMSEntities())
            {
                var SysListCollection = CMS.v_SysList
                                           .Where(b => b.t_id == nowType)
                                           .ToList();

                SelectList SysList = new SelectList(SysListCollection, "id", "value", nowSys);

                return SysList;
            }
        }

        public SelectList getUserList(string nowUser)
        {
            using (CMSEntities CMS = new CMSEntities())
            {
                var UserListCollection = CMS.v_UserList
                                            .ToList();

                SelectList UserList = new SelectList(UserListCollection, "id", "value", nowUser);

                return UserList;
            }
        }

        public SelectList getStatusList(string nowStatus)
        {
            using (CMSEntities CMS = new CMSEntities())
            {
                var StatusListCollection = CMS.v_StatusList
                                              .ToList();

                SelectList StatusList = new SelectList(StatusListCollection, "id", "value", nowStatus);

                return StatusList;
            }
        }

        public SelectList getSubjectList(string nowClass, string nowType, string nowSys, string sno)
        {
            using (CMSEntities CMS = new CMSEntities())
            {
                var SubjectCollection = CMS.v_SubjectList
                                      .Where(b => b.s_class == nowClass)
                                      .Where(b => b.s_type == nowType)
                                      .Where(b => b.s_sys == nowSys)
                                      .ToList();

                SelectList SubjectList = new SelectList(SubjectCollection, "id", "value", sno);

                return SubjectList;
            }
        }

        public SelectList getMailServerProfileList(int nowMailServer)
        {
            using (CMSEntities CMS = new CMSEntities())
            {
                var MailServerProfileCollection = CMS.v_MailServerProfileList
                                                     .ToList();

                SelectList MailServerProfileList = new SelectList(MailServerProfileCollection, "id", "value");

                return MailServerProfileList;
            }
        }

        public SelectList getTextMessageProfileList(int nowTextMessageServer)
        {
            using (CMSEntities CMS = new CMSEntities())
            {
                var TextMessageProfileCollection = CMS.v_TextMessageProfileList
                                                      .ToList();

                SelectList TextMessageProfileList = new SelectList(TextMessageProfileCollection, "id", "value");

                return TextMessageProfileList;
            }
        }

        public SelectList getNotifyGroupList(int nowNotifyGroup)
        {
            using (CMSEntities CMS = new CMSEntities())
            {
                var NotifyGroupCollection = CMS.v_NotifyGroupList
                                               .ToList();

                SelectList NotifyGroupList = new SelectList(NotifyGroupCollection, "id", "value");

                return NotifyGroupList;
            }
        }

        public SelectList getAckTypeList(string nowAckType)
        {
            using (CMSEntities CMS = new CMSEntities())
            {
                var AckTypeListCollection = CMS.v_ACKTYPEList
                                               .ToList();

                SelectList AckTypeList = new SelectList(AckTypeListCollection, "id", "value", nowAckType);

                return AckTypeList;
            }
        }

        public SelectList getAckReasonList(int nowAckType, string nowAckReason)
        {
            using (CMSEntities CMS = new CMSEntities())
            {
                var AckReasonListCollection = CMS.v_ACKREASONList
                                                 .Where(b => b.a_id == nowAckType)
                                                 .ToList();

                SelectList AckReasonList = new SelectList(AckReasonListCollection, "id", "value", nowAckReason);

                return AckReasonList;
            }
        }

        public bool isedit(bool UseCertLogin, int RoleID, int FucnID)
        {
            bool Result = false;

            if (UseCertLogin != true)
            {

            }
            else
            {
                using (CMSEntities CMS = new CMSEntities())
                {
                    int RoleMappingCount = CMS.RoleFuncMapping
                                .Where(b => b.r_id == RoleID)
                                .Where(b => b.f_id == FucnID).Count();

                    if (RoleMappingCount > 0)
                    {

                        Result = true;
                    }
                }
            }
            return Result;
        }

        /// <summary>
        /// 檢查監控項目編號是否重複
        /// </summary>
        /// <param name="s_no">監控項目編號</param>
        /// <param name="CheckTarget">檢查資料表</param>
        /// <returns></returns>
        public bool CheckRepSNO(string s_no, string CheckTarget)
        {
            if (s_no != "")
            {
                int RepSNOCount = 0;
                using (CMSEntities CMS = new CMSEntities())
                {

                    if (CheckTarget == "MonitorProperty")
                    {
                        RepSNOCount = CMS.MonitorProperty
                                  .Where(b => b.s_no == s_no).Count();
                    }
                    else
                    {
                        if (CheckTarget == "TmpMonitorProperty")
                        {
                            RepSNOCount = CMS.TmpMonitorProperty
                                  .Where(b => b.s_no == s_no).Count();
                        }
                    }


                    if (RepSNOCount <= 0)
                    {
                        return false;
                    }
                    else
                    {
                        logandshowInfo("監控項目編號:[" + s_no + "]已重複", log_Info);
                        return true;
                    }
                }
            }
            else { return true; }
        }       
    }
}