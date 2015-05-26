using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMD.ViewModels
{
    public class CreateMonitorPropertyDataViewModel
    {

        public string s_class { get; set; }
        public string s_type { get; set; }
        public string s_sys { get; set; }
        public string MailServerProfile { get; set; }
        public string TextMessageProfile { get; set; }
        public string w_notify { get; set; }
        public string e_notify { get; set; }
        public string f_notify { get; set; }
        public SelectList ClassList { get; set; }
        public SelectList TypeList { get; set; }
        public SelectList SysList { get; set; }
        public SelectList MailServerProfileList { get; set; }
        public SelectList TextMessageProfileList { get; set; }
        public SelectList WarnNotifyGroupList { get; set; }
        public SelectList ErrorNotifyGroupList { get; set; }
        public SelectList FatalNotifyGroupList { get; set; }
    }
}