using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Net;
using System.Text;
using System.IO;
using TWCAlib;
using NLog;
using NLog.Config;

namespace CMD.SystemClass
{
    public class SyncAndCheck
    {
        private string _Mode = string.Empty;
        private string _PostPosition = string.Empty;

        private string _PoatData = string.Empty;
        private string _ConnStr;
        private string _op_name;

        ShareFunc SF = new ShareFunc();
        String log_Info = "Info";
        String log_Err = "Err";

        /// <summary>
        /// 模式
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Mode
        {
            get { return _Mode; }
            set { _Mode = value; }
        }

        /// <summary>
        /// 監控項目接收窗口
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string PostPosition
        {
            get { return _PostPosition; }
            set { _PostPosition = value; }
        }

        /// <summary>
        /// 同步資料
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string PoatData
        {
            get { return _PoatData; }
            set { _PoatData = value; }
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

        public void setSyncAndCheck(string Mode, string PostPosition, string PoatData)
        {
            this.Mode = Mode;
            this.PostPosition = PostPosition;
            this.PoatData = PoatData;
        }

        public List<string> doPost(SystemConfig _Configer)
        {
            List<string> ResultList = new List<string>();
            string ResultStr = string.Empty;
            string PostPosition = string.Empty;
            string PostHeader = string.Empty;
            string PostData = string.Empty;
            string Mode = string.Empty;
            SystemConfig Configer = _Configer;

            HttpWebRequest req ;
            WebResponse rep;

            SF.ConnStr = this.ConnStr;
            SF.op_name = this.op_name;

            OPLog OPLoger = new OPLog();
            string op_action = null;
            DateTime op_stime = default(DateTime);
            DateTime op_etime = default(DateTime);
            int op_a_count = 1;
            int op_s_count = 0;
            int op_f_count = 0;
            string op_msg = string.Empty;
            bool op_result = false;

            op_action = "檢測工具參數作業";

            if (Mode == "Check")
            {
                op_action = "檢查" + op_action;
            }
            else if (Mode == "Sync")
            {
                op_action = "同步" + op_action;
            }

            PostPosition = this.PostPosition;
            PostData = this.PoatData;
            Mode = this.Mode;

            try
            {
                op_stime = DateTime.Now;

                PostHeader = getPostHeadder(Mode);

                if (PostHeader == "NoHeader")
                {
                    op_etime = DateTime.Now;
                    op_f_count = 1;
                    op_msg = "錯誤訊息:[傳送資料資料有問題,未指定作業類型]";

                    //寫入文字檔Log
                    SF.logandshowInfo("[" + this.op_name + "]執行[" + op_action + "]失敗,請查詢Debug Log得到詳細資訊", log_Info);
                    SF.logandshowInfo("[" + this.op_name + "]執行[" + op_action + "]失敗", log_Err);
                    SF.logandshowInfo("傳送模式:[" + Mode + "]", log_Err);
                    SF.logandshowInfo("傳送位置:[" + PostPosition + "]", log_Err);
                    SF.logandshowInfo("傳送資料:[" + PostData + "]", log_Err);
                    SF.logandshowInfo(op_msg, log_Err);

                    //寫入DB Log
                    OPLoger.SetOPLog(this.op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);

                    SF.log2DB(OPLoger, Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver);

                    ResultList.Add("-3");
                    ResultList.Add(op_msg);
                    //MessageBox.Show(op_msg)
                }
                else
                {
                    byte[] reqcontent = Encoding.UTF8.GetBytes(PostHeader + SecurityProcessor.HtmlEncode(SecurityProcessor.TurnStrig2Base64byUTF8(_PoatData)));
                    req = (HttpWebRequest)System.Net.HttpWebRequest.Create(PostPosition);
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.Method = "POST";
                    req.ContentLength = reqcontent.Length;
                    using (Stream reqstteam = req.GetRequestStream())
                    {
                        reqstteam.Write(reqcontent, 0, reqcontent.Length);
                    }

                    rep = req.GetResponse();

                    using (Stream myStream = rep.GetResponseStream())
                    {
                        using (StreamReader myStreamReader = new StreamReader(myStream))
                         {
                              ResultStr = myStreamReader.ReadToEnd();
                         }
                    } 
                   
                    XmlDocument ResultXML = new XmlDocument();
                    ResultXML.LoadXml(ResultStr);

                    XmlNode RootNode = ResultXML.SelectSingleNode("MonitorTool_Property_" + Mode + "_Result");
                    string s_no = RootNode.SelectSingleNode("CHECK_NO").InnerText;
                    string Result = RootNode.SelectSingleNode(Mode + "Result").InnerText;
                    string Msg = RootNode.SelectSingleNode(Mode + "Msg").InnerText;

                    if (Result == "000")
                    {
                        op_etime = DateTime.Now;
                        op_result = true;
                        op_s_count = 1;

                        //寫入文字檔Log
                        SF.logandshowInfo("[" + this.op_name + "]執行[" + op_action + "]成功,監控項目編號[" + s_no + "]",log_Info);

                        //寫入DB Log
                        OPLoger.SetOPLog(this.op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);

                        SF.log2DB(OPLoger, Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver);

                        ResultList.Add("0");
                        ResultList.Add(Msg);
                    }
                    else
                    {
                        op_etime = DateTime.Now;
                        op_msg = Result + Msg;
                        op_f_count = 1;

                        //寫入文字檔Log
                        SF.logandshowInfo("[" + this.op_name + "]執行[" + op_action + "]失敗,監控項目編號[" + s_no + "],請查詢Debug Log得到詳細資訊", log_Info);
                        SF.logandshowInfo("[" + this.op_name + "]執行[" + op_action + "]失敗,監控項目編號[" + s_no + "]",log_Err);
                        SF.logandshowInfo("傳送模式:[" + Mode + "]", log_Err);
                        SF.logandshowInfo("傳送位置:[" + PostPosition + "]", log_Err);
                        SF.logandshowInfo("傳送資料:[" + PostData + "]", log_Err);
                        SF.logandshowInfo("回應代碼:" + Result, log_Err);
                        SF.logandshowInfo("回應訊息:" + Msg, log_Err);

                        //寫入DB Log
                        OPLoger.SetOPLog(this.op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);

                        SF.log2DB(OPLoger, Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver);

                        ResultList.Add("-9");
                        ResultList.Add(Msg);
                    }

                }
            }
            catch (Exception ex)
            {
                op_etime = DateTime.Now;
                op_msg = "錯誤訊息:[" + ex.ToString() + "]";
                op_f_count = 1;

                //寫入文字檔Log
                SF.logandshowInfo("[" + this.op_name + "]執行[" + op_action + "]失敗,請查詢Debug Log得到詳細資訊", log_Info);
                SF.logandshowInfo("[" + this.op_name + "]執行[" + op_action + "]失敗", log_Err);
                SF.logandshowInfo("傳送模式:[" + Mode + "]", log_Err);
                SF.logandshowInfo("傳送位置:[" + PostPosition + "]", log_Err);
                SF.logandshowInfo("傳送資料:[" + PostData + "]", log_Err);
                SF.logandshowInfo(op_msg, log_Err);

                //寫入DB Log
                OPLoger.SetOPLog(this.op_name, op_action, op_stime, op_etime, op_a_count, op_s_count, op_f_count, op_msg, op_result);

                SF.log2DB(OPLoger, Configer.MailServer, Configer.MailServerPort, Configer.MailSender, Configer.MailReceiver);

                ResultList.Add("-9");
                ResultList.Add(op_msg);
            }

            return ResultList;
        }

        public static string getPostHeadder(string _mode)
        {
            string ResultStr = string.Empty;
            string Mode = _mode;

            if (Mode == "Sync")
            {
                ResultStr = "MonitorTool_Property_Sync=";
            }
            else if (Mode == "Check")
            {
                ResultStr = "MonitorTool_Property_Check=";
            }
            else
            {
                ResultStr = "NoHeader";
            }

            return ResultStr;
        }

    }
}