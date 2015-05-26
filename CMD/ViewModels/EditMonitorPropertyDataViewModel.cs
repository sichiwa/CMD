using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMD.ViewModels
{
    public class EditMonitorPropertyDataViewModel
    {
        public string s_no { get; set; }
        public bool isenable { get; set; }
        public string s_class { get; set; }
        public string s_type { get; set; }
        public string s_sys { get; set; }
        public string s_subject { get; set; }
        public string s_content { get; set; }
        public string s_position { get; set; }
        public int s_timeout { get; set; }
        public int s_frequency { get; set; }
        public string params_host { get; set; }
        public string @params { get; set; }

        public int MailServerProfile { get; set; }
        public int TextMessageProfile { get; set; }
        public bool w_sendmail { get; set; }
        public bool w_sendmessage { get; set; }
        public int w_notify { get; set; }
        public int w_mail_notify_limit { get; set; }
        public int w_message_notify_limit { get; set; }
        public bool e_sendmail { get; set; }
        public bool e_sendmessage { get; set; }
        public int e_notify { get; set; }
        public int e_mail_notify_limit { get; set; }
        public int e_message_notify_limit { get; set; }
        public bool f_sendmail { get; set; }
        public bool f_sendmessage { get; set; }
        public int f_notify { get; set; }
        public int f_mail_notify_limit { get; set; }
        public int f_message_notify_limit { get; set; }
        public string sysadmin { get; set; }
        public SelectList ClassList { get; set; }
        public SelectList TypeList { get; set; }
        public SelectList SysList { get; set; }
        public SelectList MailServerProfileList { get; set; }
        public SelectList TextMessageProfileList { get; set; }
        public SelectList WarnNotifyGroupList { get; set; }
        public SelectList ErrorNotifyGroupList { get; set; }
        public SelectList FatalNotifyGroupList { get; set; }
        //public MonitorProperty MonitorProperty { get; set; }
    }
}