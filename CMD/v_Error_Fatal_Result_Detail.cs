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
    
    public partial class v_Error_Fatal_Result_Detail
    {
        public int id { get; set; }
        public string s_no { get; set; }
        public System.DateTime s_time { get; set; }
        public string s_cost { get; set; }
        public string s_status { get; set; }
        public string s_subject { get; set; }
        public string s_content { get; set; }
        public string s_position { get; set; }
        public Nullable<int> MailServerProfile { get; set; }
        public Nullable<int> TextMessageProfile { get; set; }
        public Nullable<bool> w_sendmail { get; set; }
        public Nullable<bool> w_sendmessage { get; set; }
        public int w_notify { get; set; }
        public int w_mail_notify_limit { get; set; }
        public int w_message_notify_limit { get; set; }
        public int w_mail_notify_now { get; set; }
        public int w_message_notify_now { get; set; }
        public Nullable<bool> e_sendmail { get; set; }
        public Nullable<bool> e_sendmessage { get; set; }
        public int e_notify { get; set; }
        public int e_mail_notify_limit { get; set; }
        public int e_message_notify_limit { get; set; }
        public int e_mail_notify_now { get; set; }
        public int e_message_notify_now { get; set; }
        public Nullable<bool> f_sendmail { get; set; }
        public Nullable<bool> f_sendmessage { get; set; }
        public int f_notify { get; set; }
        public int f_mail_notify_limit { get; set; }
        public int f_message_notify_limit { get; set; }
        public int f_mail_notify_now { get; set; }
        public int f_message_notify_now { get; set; }
        public string s_msg { get; set; }
    }
}