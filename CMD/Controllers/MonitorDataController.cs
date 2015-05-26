using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Web.Routing;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using CMD.ViewModels;
using CMD.SystemClass;
using CMD.Models;
using TWCAlib;

namespace CMD.Controllers
{
    public class MonitorDataController : ApiController
    {
        SystemConfig Configer = new SystemConfig();
        OPLog OPLoger = new OPLog();
        ShareFunc SF = new ShareFunc();
        String log_Info = "Info";
        String log_Err = "Err";
        String op_name = "顯示中控台";

        // GET api/<controller>
        public string Get()
        {
            return "value";
        }

        // GET api/<controller>/5
        public MonitorDataViewModel Get(string s_class)
        {
            //初始化系統參數
            Configer.Init();
            SF.ConnStr = Configer.C_DBConnstring;

            //初始化回傳物件
            MonitorDataViewModel MV = new MonitorDataViewModel();
            List<int> ProdStatusCountList = new List<int>();
            List<int> TestStatusCountList = new List<int>();

            DataTable ProdScanDT = new DataTable();
            DataTable TestScanDT = new DataTable();

            ProdScanDT = getSHOWDATA("0");
            TestScanDT = getSHOWDATA("1");
            
            //取得目前Session
            var session = SessionStateUtility.GetHttpSessionStateFromContext(HttpContext.Current);
            bool UseCertLogin=Convert.ToBoolean(session["UseCertLogin"].ToString());
            int UserRole= Convert.ToInt16(session["UserRole"].ToString());
            int FuncID=11;
            //判斷使用者是否有權限編輯監控項目
            MV.caneditMonitProperty = SF.isedit(UseCertLogin, UserRole, FuncID);
            //MV.caneditMonitProperty = SF.isedit(true, 1, 11);

            if (ProdScanDT != null)
            {
                MV.ProdbtnInfos = getbtnInfo(ProdScanDT);
                ProdStatusCountList = getStatusCount("0", ProdScanDT);
                MV.ProdbtnClass = getPosbtnCSSClass(ProdStatusCountList);
            }
            if (TestScanDT != null)
            {
                MV.TestbtnInfos = getbtnInfo(TestScanDT);
                TestStatusCountList = getStatusCount("1", TestScanDT);
                MV.TestbtnClass = getPosbtnCSSClass(TestStatusCountList);
            }

            MV.ErrorData = getErrorData(s_class,Configer.SelectTopN);

            return MV;
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {

        }

        /// <summary>
        /// 取得監控結果資料
        /// </summary>
        /// <param name="_s_class">環境代碼(正式(0)、測試(1)、異地(2)、驗證(3))</param>
        /// <returns>監控資料DataTable</returns>
        /// <remarks>1030505 黃富彥</remarks>
        private DataTable getSHOWDATA(string _s_class)
        {
            DataTable ResultDT = new DataTable();
            SqlCommand SqlComm = new SqlCommand();
            StringBuilder SqlStr = new StringBuilder();
            string s_class = _s_class;


            string op_action = null;
            DateTime op_stime = default(DateTime);
            DateTime op_etime = default(DateTime);
            int op_a_count = 0;
            int op_s_count = 0;
            int op_f_count = 0;
            string op_msg = string.Empty;
            bool op_result = false;

            SF.op_name = op_name;

            string MailSubject = string.Empty;
            StringBuilder MailBody = new StringBuilder();
            string SendResult = string.Empty;

            op_action = "取得" + SF.getClassNameOrCode("5", s_class) + "環境監控結果資料";

            SqlStr.Append("exec MDRSYS.usp_getSHOWDATA @class;");

            SqlParameter Pclass = new SqlParameter();
            Pclass.ParameterName = "@class";
            Pclass.DbType = DbType.String;
            Pclass.Size = 10;
            Pclass.Value = s_class;
            SqlComm.Parameters.Add(Pclass);

            SqlComm.CommandText = SqlStr.ToString();

            try
            {
                using (SqlComm)
                {
                    op_stime = DateTime.Now;
                    ResultDT = DataProcessor.SelectDataTable(Configer.C_DBConnstring, SqlComm);
                    op_etime = DateTime.Now;
                }

                string SelectResult = ResultDT.Rows[0]["result"].ToString();

                if (SelectResult == "0")
                {
                    op_a_count = ResultDT.Rows.Count;
                    op_s_count = op_a_count;
                    op_result = true;

                    SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]成功,本次共取得" + op_a_count.ToString() + "筆資料", log_Info);

                    //寫入DB Log
                    OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                    SF.log2DB(OPLoger, Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver);

                    return ResultDT;
                }
                else if (SelectResult == "-2")
                {
                    op_msg = "本次查詢無資料";
                    SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢無資料,請查詢Debug Log得到詳細資訊", log_Info);
                    SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢無資料", log_Err);
                    SF.logandshowInfo("執行SQL:[" + SqlStr.ToString() + "]", log_Err);
                    SF.logandshowInfo("環境:[" + s_class + "]", log_Err);

                    //寫入DB Log
                    OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                    SF.log2DB(OPLoger, Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver);

                    return null;
                }
                else
                {
                    op_etime = DateTime.Now;
                    string ErrorNumber = ResultDT.Rows[0]["ErrorNumber"].ToString();
                    string ErrorSeverity = ResultDT.Rows[0]["ErrorSeverity"].ToString();
                    string ErrorState = ResultDT.Rows[0]["ErrorState"].ToString();
                    string ErrorProcedure = ResultDT.Rows[0]["ErrorProcedure"].ToString();
                    string ErrorLine = ResultDT.Rows[0]["ErrorLine"].ToString();
                    string ErrorMessage = ResultDT.Rows[0]["ErrorMessage"].ToString();

                    op_msg = "錯誤訊息:[" + ErrorMessage + "];" + "發生錯誤行數:[" + ErrorLine + "];" + "錯誤碼:[" + ErrorNumber + "];" + "錯誤嚴重性(17以上請通知資料庫管理人員):[" + ErrorSeverity + "];" + "發生錯誤程式:[" + ErrorProcedure + "];" + "錯誤狀態:[" + ErrorState + "];";

                    SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢發生異常,請查詢Debug Log得到詳細資訊", log_Info);
                    SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢發生異常", log_Err);
                    SF.logandshowInfo("執行SQL:[" + SqlStr.ToString() + "]", log_Err);
                    SF.logandshowInfo("環境:[" + s_class + "]", log_Err);
                    SF.logandshowInfo("錯誤訊息:[" + op_msg + "]", log_Err);

                    //寫入DB Log
                    OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                    SF.log2DB(OPLoger, Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver);

                    //通知系統管理人員
                    MailSubject = "[異常]中控處理系統-" + op_name + "模組執行[" + op_action + "]失敗";
                    MailBody.Append("<table>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("[" + op_name + "]執行[" + op_action + "]失敗,詳細資訊如下");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("執行SQL:[" + SqlStr.ToString() + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("環境:[" + s_class + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("錯誤訊息:[" + ErrorMessage + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("發生錯誤行數:[" + ErrorLine + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("錯誤碼:[" + ErrorNumber + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("錯誤嚴重性(17以上請通知資料庫管理人員):[" + ErrorSeverity + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("發生錯誤程式:[" + ErrorProcedure + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("錯誤狀態:[" + ErrorState + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("</table>");

                    SF.EmailNotify2Sys(Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver, true, MailSubject, MailBody.ToString());

                    return null;
                }
            }
            catch (Exception ex)
            {
                op_etime = DateTime.Now;
                op_msg = "錯誤訊息:[" + ex.ToString() + "]";

                SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢發生異常,請查詢Debug Log得到詳細資訊", log_Info);
                SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢發生異常", log_Err);
                SF.logandshowInfo("執行SQL:[" + SqlStr.ToString() + "]", log_Err);
                SF.logandshowInfo(op_msg, log_Err);

                //寫入DB Log
                OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                SF.log2DB(OPLoger, Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver);

                //通知系統管理人員
                MailSubject = "[異常]中控處理系統-" + op_name + "模組執行[" + op_action + "]失敗";
                MailBody.Append("<table>");
                MailBody.Append("<tr><td>");
                MailBody.Append("[" + op_name + "]執行[" + op_action + "]失敗,詳細資訊如下");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append("執行SQL:[" + SqlStr.ToString() + "]");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append("環境:[" + s_class + "]");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append("錯誤訊息:[" + op_msg + "]");
                MailBody.Append("</td></tr>");
                MailBody.Append("</table>");

                SF.EmailNotify2Sys(Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver, true, MailSubject, MailBody.ToString());

                return null;
            }
        }

        /// <summary>
        /// 取得監控按鈕資訊
        /// </summary>
        /// <param name="_SourceDT">監控結果資料DataTable</param>
        /// <returns>按鈕資訊陣列</returns>
        /// <remarks>1030506 黃富彥</remarks>
        private IEnumerable<btnInfos> getbtnInfo(DataTable _SourceDT)
        {
            List<btnInfos> ResultbtnInfo = new List<btnInfos>();
            DataTable SourceDT = _SourceDT;
            int TmpIcount = SourceDT.Rows.Count - 1;

            for (int i = 0; i <= TmpIcount; i++)
            {
                btnInfos TmpbtnInfo = new btnInfos();
                //Dim id As String
                //Dim idInfo As String = String.Empty
                string B_Color = null;
                string B_Text = null;

                string Name = SourceDT.Rows[i]["value"].ToString();
                int Normal = Convert.ToInt16(SourceDT.Rows[i]["Normal"].ToString());
                int Warn = Convert.ToInt16(SourceDT.Rows[i]["Warn"].ToString());
                int Err = Convert.ToInt16(SourceDT.Rows[i]["Error"].ToString());
                int Fatal = Convert.ToInt16(SourceDT.Rows[i]["Fatal"].ToString());
                int Unknown = Convert.ToInt16(SourceDT.Rows[i]["Unknown"].ToString());
                int Used = Convert.ToInt16(SourceDT.Rows[i]["Used"].ToString());
                int NotUsed = Convert.ToInt16(SourceDT.Rows[i]["NotUsed"].ToString());

                //idInfo = getbtnidInfo(Name)
                //id = getClassNameOrCode(1, NowPos) & SplitSymbol3 & idInfo
                B_Text = Name; //+ Environment.NewLine + (Normal + Warn).ToString() + Configer.SplitSymbol3 + (Err + Fatal).ToString() + Configer.SplitSymbol3 + Unknown.ToString();
                B_Color = getbtnColor(Normal, Warn, Err, Fatal, Unknown, NotUsed);

                TmpbtnInfo.btnText = B_Text;
                TmpbtnInfo.btnClass = B_Color;
                TmpbtnInfo.btnWidth = Configer.btnWidth;
                TmpbtnInfo.btnHeight = Configer.btnHeight;
                TmpbtnInfo.xoffset = Configer.xoffset;
                TmpbtnInfo.yoffset = Configer.yoffset;

                ResultbtnInfo.Add(TmpbtnInfo);
            }

            return ResultbtnInfo;
        }

        /// <summary>
        /// 取得按鈕顏色
        /// </summary>
        /// <param name="_Normal">Normal筆數</param>
        /// <param name="_Warn">Warn筆數</param>
        /// <param name="_Err">Error筆數</param>
        /// <param name="_Fatal">Fatal筆數</param>
        /// <param name="_Unknown">Unknown筆數</param>
        /// <returns>按鈕顏色</returns>
        /// <remarks>1030506 黃富彥</remarks>
        private string getbtnColor(int _Normal, int _Warn, int _Err, int _Fatal, int _Unknown, int _NotUse)
        {

            int Normal = _Normal;
            int Warn = _Warn;
            int Err = _Err;
            int Fatal = _Fatal;
            int Unknown = _Unknown;
            int NotUse = _NotUse;

            string ResultColor = "btn btn-default";

            if (Fatal > 0)
            {
                ResultColor = "btn btn-danger";
            }
            else
            {
                if (Err > 0)
                {
                    ResultColor = "btn btn-danger";
                }
                else
                {
                    if (Unknown > 0)
                    {
                        ResultColor = "btn btn-warning";
                    }
                    else
                    {
                        if (Normal > 0)
                        {
                            ResultColor = "btn btn-success";
                        }
                        else
                        {
                            if (NotUse > 0)
                            {
                                ResultColor = "btn btn-default";
                            }
                        }
                    }
                }
            }

            return ResultColor;
        }

        /// <summary>
        /// 取得各種狀態總和
        /// </summary>
        /// <param name="_s_class">環境代碼(正式(0)、測試(1)、異地(2)、驗證(3))</param>
        /// <param name="_SourceDT">監控結果資料DataTable</param>
        /// <returns>統計陣列(index 0:Normal,index 1:Warn,index 2:Error,index 3:Fatal,index 4:Unknown,index 5:Ack)</returns>
        /// <remarks>1030505 黃富彥</remarks>
        private List<int> getStatusCount(string _s_class, DataTable _SourceDT)
        {
            DataTable SourceDT = _SourceDT;
            List<int> ResultList = new List<int>();
            string s_class = _s_class;
            int Normal = 0;
            int Warn = 0;
            int Err = 0;
            int Fatal = 0;
            int Unknown = 0;
            int Used = 0;
            int NotUsed = 0;
            int Ack = 0;

            int TmpIcount = SourceDT.Rows.Count - 1;

            for (int i = 0; i <= TmpIcount; i++)
            {
                Normal = Normal + Convert.ToInt16(SourceDT.Rows[i]["Normal"].ToString());
                Warn = Warn + Convert.ToInt16(SourceDT.Rows[i]["Warn"].ToString());
                Err = Err + Convert.ToInt16(SourceDT.Rows[i]["Error"].ToString());
                Fatal = Fatal + Convert.ToInt16(SourceDT.Rows[i]["Fatal"].ToString());
                Unknown = Unknown + Convert.ToInt16(SourceDT.Rows[i]["Unknown"].ToString());
                Used = Used + Convert.ToInt16(SourceDT.Rows[i]["Used"].ToString());
                NotUsed = NotUsed + Convert.ToInt16(SourceDT.Rows[i]["NotUsed"].ToString());
            }

            ResultList.Add(Normal);
            ResultList.Add(Warn);
            ResultList.Add(Err);
            ResultList.Add(Fatal);
            ResultList.Add(Unknown);

            Ack = getAckCount(s_class);
            ResultList.Add(Ack);
            ResultList.Add(Normal + Warn + Err + Fatal + Unknown);
            ResultList.Add(Used);
            ResultList.Add(NotUsed);

            return ResultList;
        }

        /// <summary>
        /// 取得各環境按鈕顯示樣式
        /// </summary>
        /// <param name="_s_class">環境代碼(正式(0)、測試(1)、異地(2)、驗證(3))</param>
        /// <param name="_statuscountlist">統計陣列</param>
        /// <remarks>1030505 黃富彥</remarks>
        private string getPosbtnCSSClass(List<int> _statuscountlist)
        {
            //string s_class = _s_class;
            List<int> statuscountlist = _statuscountlist;
            int T_Normal = 0;
            int T_Warn = 0;
            int T_Error = 0;
            int T_Fatal = 0;
            int T_Unknown = 0;
            int T_NotUse = 0;
            string btnColor = "btn btn-default";

            T_Normal = statuscountlist[0];
            T_Warn = statuscountlist[1];
            T_Error = statuscountlist[2];
            T_Fatal = statuscountlist[3];
            T_Unknown = statuscountlist[4];
            T_NotUse = statuscountlist[6];

            btnColor = getbtnColor(T_Normal, T_Warn, T_Error, T_Fatal, T_Unknown, T_NotUse);

            return btnColor;
        }

        /// <summary>
        /// 取得待復歸筆數
        /// </summary>
        /// <param name="_s_class">環境代碼(正式(0)、測試(1)、異地(2)、驗證(3))</param>
        /// <returns>待復歸筆數</returns>
        /// <remarks>1030506 黃富彥</remarks>
        private int getAckCount(string _s_class)
        {
            DataTable ResultDT = new DataTable();
            int Result = 0;
            SqlCommand SqlComm = new SqlCommand();
            StringBuilder SqlStr = new StringBuilder();
            string s_class = _s_class;

            //OPLoger OPLoger = new OPLoger();
            string op_action = null;
            DateTime op_stime = default(DateTime);
            DateTime op_etime = default(DateTime);
            int op_a_count = 0;
            int op_s_count = 0;
            int op_f_count = 0;
            string op_msg = string.Empty;
            bool op_result = false;

            string MailSubject = string.Empty;
            StringBuilder MailBody = new StringBuilder();
            string SendResult = string.Empty;

            op_action = "取得" + SF.getClassNameOrCode("5", s_class) + "環境待復歸資料筆數";

            SqlStr.Append("exec MDRSYS.usp_getAckCount @class;");

            SqlParameter Pclass = new SqlParameter();
            Pclass.ParameterName = "@class";
            Pclass.DbType = DbType.String;
            Pclass.Size = 10;
            Pclass.Value = s_class;
            SqlComm.Parameters.Add(Pclass);

            SqlComm.CommandText = SqlStr.ToString();

            try
            {
                using (SqlComm)
                {
                    op_stime = DateTime.Now;
                    ResultDT = DataProcessor.SelectDataTable(Configer.C_DBConnstring, SqlComm);
                    op_etime = DateTime.Now;
                }

                string SelectResult = ResultDT.Rows[0]["result"].ToString();

                if (SelectResult == "0")
                {
                    Result = Convert.ToInt16(ResultDT.Rows[0]["selectMsg"].ToString());
                    op_a_count = 1;
                    op_s_count = op_a_count;
                    op_result = true;

                    SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]成功,本次共取得" + op_a_count.ToString() + "筆資料", log_Info);

                    //寫入DB Log
                    OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                    SF.log2DB(OPLoger, Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver);

                    return Result;
                }
                else if (SelectResult == "-2")
                {
                    op_msg = "目前無待復歸資料";
                    op_result = true;

                    SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]成功," + op_msg + "", log_Info);

                    //寫入DB Log
                    OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                    SF.log2DB(OPLoger, Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver);

                    return Result;
                }
                else
                {
                    string ErrorNumber = ResultDT.Rows[0]["ErrorNumber"].ToString();
                    string ErrorSeverity = ResultDT.Rows[0]["ErrorSeverity"].ToString();
                    string ErrorState = ResultDT.Rows[0]["ErrorState"].ToString();
                    string ErrorProcedure = ResultDT.Rows[0]["ErrorProcedure"].ToString();
                    string ErrorLine = ResultDT.Rows[0]["ErrorLine"].ToString();
                    string ErrorMessage = ResultDT.Rows[0]["ErrorMessage"].ToString();

                    op_msg = "錯誤訊息:[" + ErrorMessage + "];" + "發生錯誤行數:[" + ErrorLine + "];" + "錯誤碼:[" + ErrorNumber + "];" + "錯誤嚴重性(17以上請通知資料庫管理人員):[" + ErrorSeverity + "];" + "發生錯誤程式:[" + ErrorProcedure + "];" + "錯誤狀態:[" + ErrorState + "];";

                    SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢發生異常,請查詢Debug Log得到詳細資訊", log_Info);
                    SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢發生異常", log_Err);
                    SF.logandshowInfo("執行SQL:[" + SqlStr.ToString() + "]", log_Err);
                    SF.logandshowInfo("環境:[" + s_class + "]", log_Err);
                    SF.logandshowInfo("錯誤訊息:[" + op_msg + "]", log_Err);

                    //寫入DB Log
                    OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                    SF.log2DB(OPLoger, Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver);

                    //通知系統管理人員
                    MailSubject = "[異常]中央監控系統-" + op_name + "模組執行[" + op_action + "]失敗";
                    MailBody.Append("<table>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("[" + op_name + "]執行[" + op_action + "]失敗,詳細資訊如下");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("執行SQL:[" + SqlStr.ToString() + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("環境:[" + s_class + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("錯誤訊息:[" + ErrorMessage + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("發生錯誤行數:[" + ErrorLine + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("錯誤碼:[" + ErrorNumber + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("錯誤嚴重性(17以上請通知資料庫管理人員):[" + ErrorSeverity + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("發生錯誤程式:[" + ErrorProcedure + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("<tr><td>");
                    MailBody.Append("錯誤狀態:[" + ErrorState + "]");
                    MailBody.Append("</td></tr>");
                    MailBody.Append("</table>");

                    SF.EmailNotify2Sys(Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver, true, MailSubject, MailBody.ToString());

                    return Result;
                }
            }
            catch (Exception ex)
            {
                op_msg = "錯誤訊息:[" + op_msg + "]";

                SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢發生異常,請查詢Debug Log得到詳細資訊", log_Info);
                SF.logandshowInfo("[" + op_name + "]執行[" + op_action + "]失敗,本次查詢發生異常", log_Err);
                SF.logandshowInfo("執行SQL:[" + SqlStr.ToString() + "]", log_Err);
                SF.logandshowInfo("環境:[" + s_class + "]", log_Err);
                SF.logandshowInfo(op_msg, log_Err);

                //寫入DB Log
                OPLoger.SetOPLog(op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);
                SF.log2DB(OPLoger, Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver);

                //通知系統管理人員
                MailSubject = "[異常]中央監控系統-" + op_name + "模組執行[" + op_action + "]失敗";
                MailBody.Append("<table>");
                MailBody.Append("<tr><td>");
                MailBody.Append("[" + op_name + "]執行[" + op_action + "]失敗,詳細資訊如下");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append("執行SQL:[" + SqlStr.ToString() + "]");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append("環境:[" + s_class + "]");
                MailBody.Append("</td></tr>");
                MailBody.Append("<tr><td>");
                MailBody.Append(op_msg);
                MailBody.Append("</td></tr>");
                MailBody.Append("</table>");

                SF.EmailNotify2Sys(Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver, true, MailSubject, MailBody.ToString());

                return Result;
            }
        }

        /// <summary>
        /// 取得異常顯示清單
        /// </summary>
        /// <param name="s_class"></param>
        /// <returns></returns>
        private IEnumerable<v_Monitor_Data> getErrorData(string s_class,int TopN)
        {
            //初始化系統參數
            Configer.Init();
            SF.ConnStr = Configer.C_DBConnstring;
            
            using (CMSEntities CMS = new CMSEntities())
            {

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

                var statusList = new string[] { "Error", "Fatal" };
                op_action = "取得異常檢視資料作業";

                try
                {
                    op_stime = DateTime.Now;
                    var MD = CMS.v_Monitor_Data
                        //.Where(b => b.回報結果 != "未啟用")
                        .Where(b => b.s_class == s_class)
                        .Where(b => statusList.Contains(b.回報結果))
                        .OrderBy(b => b.順序).ThenBy(b => b.監控項目編號)
                        .ToList();

                    if (TopN > 0)
                    {
                        MD = MD.Take(TopN).ToList();
                    }
                 
                    op_etime = DateTime.Now;

                    if (MD != null)
                    {
                        op_result = true;
                        op_a_count = MD.Count;
                        op_s_count = op_a_count;
                        op_msg = "[" + op_name + "]執行[" + op_action + "]成功,本次查詢共" + op_s_count.ToString() + "筆監控項目檢視資料";
                        SF.logandshowInfo(op_msg, log_Info);
                        return MD;
                    }
                    else
                    {
                        op_result = true;
                        op_msg = "[" + op_name + "]執行[" + op_action + "]成功,本次查詢無資料";
                        SF.logandshowInfo(op_msg, log_Info);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    string MailSubject = null;
                    StringBuilder MailBody = null;
                    string nowclass = SF.getClassNameOrCode("5", s_class);

                    op_msg = "[" + op_name + "]執行[" + op_action + "]失敗,錯誤訊息[" + ex.ToString() + "]";
                    SF.logandshowInfo(op_msg, log_Err);
                    SF.logandshowInfo("本次查詢環境為:[" + nowclass + "]", log_Err);

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
    }
}