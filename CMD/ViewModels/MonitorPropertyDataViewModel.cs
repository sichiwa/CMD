using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMD.ViewModels
{
    public class MonitorPropertyDataViewModel
    {
        public bool caneditMonitProperty { get; set; }
        public int needReviewCount { get; set; }
        public string nowClass { get; set; }
        public string nowType { get; set; }
        public string nowSys { get; set; }
        public string nowStatus { get; set; }
        public string nowUser { get; set; }
        public string nowSubject { get; set; }
        public SelectList ClassList { get; set; }
        public SelectList TypeList { get; set; }
        public SelectList SysList { get; set; }
        public SelectList StatusList { get; set; }
        public SelectList UserList { get; set; }
        //public IEnumerable<SelectListItem> SubjectList { get; set; }
        public IEnumerable<v_Monitor_Data> MonitorData { get; set; }
        
    }
}