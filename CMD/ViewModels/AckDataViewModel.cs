using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMD.ViewModels
{
    public class AckDataViewModel
    {
        public bool caneditMonitProperty { get; set; }
        public string nowClass { get; set; }
        public string nowType { get; set; }
        public string nowSys { get; set; }
        public string nowAckType { get; set; }
        public string nowAckReason { get; set; }
        public SelectList ClassList { get; set; }
        public SelectList TypeList { get; set; }
        public SelectList SysList { get; set; }
        public SelectList AckTypeList { get; set; }
        public SelectList AckReasonList { get; set; }
        public PagedList.IPagedList<v_ACK_Data> AckData { get; set; }
    }
}