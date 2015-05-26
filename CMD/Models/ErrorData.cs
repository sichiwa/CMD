using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMD.Models
{
    public class ErrorData
    {
        public string IsHostMonitor { get; set; }
        public string s_no { get; set; }
        public string s_subject { get; set; }
        public string s_msg { get; set; }
        public DateTime n_time { get; set; }
        public string s_status { get; set; }
        public string s_class { get; set; }
        public string s_type { get; set; }
        public string s_sys { get; set; }
    }
}