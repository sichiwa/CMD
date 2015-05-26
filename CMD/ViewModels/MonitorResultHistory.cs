using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMD.ViewModels
{
    public class MonitorResultHistory
    {
        public string nowClass { get; set; }
        public string nowType { get; set; }
        public string nowSys { get; set; }
        public string nowStatus { get; set; }
        public string 監控項目主旨 { get; set; }
        public string 環境 { get; set; }
        public string 分類 { get; set; }
        public string 系統別 { get; set; }
        public string sno { get; set; }
        public SelectList ClassList { get; set; }
        public SelectList TypeList { get; set; }
        public SelectList SysList { get; set; }
        public SelectList StatusList { get; set; }
        public SelectList SubjectList { get; set; }
        public PagedList.IPagedList<v_Monitor_All_Reult_History> AckData { get; set; }
    }
}