//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace CMD
{
    using System;
    using System.Collections.Generic;
    
    public partial class v_TmpMonitorProperty_Detail
    {
        public string s_class { get; set; }
        public string 環境 { get; set; }
        public string s_type { get; set; }
        public string 分類 { get; set; }
        public string s_sys { get; set; }
        public string 系統別 { get; set; }
        public string s_subject { get; set; }
        public string s_content { get; set; }
        public string s_position { get; set; }
        public int s_timeout { get; set; }
        public int s_frequency { get; set; }
        public int MailServerProfile { get; set; }
        public string 郵件設定檔名稱 { get; set; }
        public int TextMessageProfile { get; set; }
        public string 簡訊設定檔名稱 { get; set; }
        public Nullable<bool> w_sendmail { get; set; }
        public Nullable<bool> w_sendmessage { get; set; }
        public int w_notify { get; set; }
        public string Warn通知群組 { get; set; }
        public int w_mail_notify_limit { get; set; }
        public int w_message_notify_limit { get; set; }
        public int w_mail_notify_now { get; set; }
        public int w_message_notify_now { get; set; }
        public Nullable<bool> e_sendmail { get; set; }
        public Nullable<bool> e_sendmessage { get; set; }
        public int e_notify { get; set; }
        public string Error通知群組 { get; set; }
        public int e_mail_notify_limit { get; set; }
        public int e_message_notify_limit { get; set; }
        public int e_mail_notify_now { get; set; }
        public int e_message_notify_now { get; set; }
        public Nullable<bool> f_sendmail { get; set; }
        public Nullable<bool> f_sendmessage { get; set; }
        public int f_notify { get; set; }
        public string Fatal通知群組 { get; set; }
        public int f_mail_notify_limit { get; set; }
        public int f_message_notify_limit { get; set; }
        public int f_mail_notify_now { get; set; }
        public int f_message_notify_now { get; set; }
        public string sysadmin { get; set; }
        public string create_account { get; set; }
        public string update_account { get; set; }
        public System.DateTime create_time { get; set; }
        public System.DateTime update_time { get; set; }
        public string review_account { get; set; }
        public System.DateTime review_time { get; set; }
        public string s_no { get; set; }
        public string params_host { get; set; }
        public string MonitorToolParams { get; set; }
        public Nullable<bool> isenable { get; set; }
        public Nullable<bool> issync { get; set; }
    }
}