using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMD.Models
{
    public class QueryAckData
    {
        public int PageIndex { get; set; }
        public string nowClass { get; set; }
        public string nowType { get; set; }
        public string nowSys { get; set; }
        public string nowUser { get; set; }
        public DateTime STime { get; set; }
        public DateTime ETime { get; set; }
    }
}