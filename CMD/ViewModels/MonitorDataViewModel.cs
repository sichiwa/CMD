using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CMD.Models;

namespace CMD.ViewModels
{
    public class MonitorDataViewModel
    {
        public bool caneditMonitProperty { get; set; }
        public string ProdbtnClass { get; set; }
        public string TestbtnClass { get; set; }
        public IEnumerable<btnInfos> ProdbtnInfos { get; set; }
        public IEnumerable<btnInfos> TestbtnInfos { get; set; }
        public IEnumerable<v_Monitor_Data> ErrorData { get; set; }
    }
}