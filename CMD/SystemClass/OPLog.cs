using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMD.SystemClass
{
    public class OPLog
    {
        
        
        public string op_name { get; set; }
        public string op_action { get; set; }
        public DateTime op_stime { get; set; }
        public DateTime op_etime { get; set; }
        public int op_a_count { get; set; }
        public int op_s_count { get; set; }
        public int op_f_count { get; set; }
        public string op_msg { get; set; }
        public Boolean op_result { get; set; }


        public void SetOPLog(string op_name, string op_action, DateTime op_stime, DateTime op_etime, int op_a_count, int op_s_count, int op_f_count, string op_msg, bool op_result)
        {
            this.op_name = op_name;
            this.op_action = op_action;

            this.op_stime = op_stime;
            this.op_etime = op_etime;

            this.op_a_count = op_a_count;
            this.op_s_count = op_s_count;
            this.op_f_count = op_f_count;

            this.op_msg = op_msg;
            this.op_result = op_result;
        }
    }
}