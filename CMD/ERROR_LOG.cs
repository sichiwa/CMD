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
    
    public partial class ERROR_LOG
    {
        public int id { get; set; }
        public string s_no { get; set; }
        public System.DateTime s_time { get; set; }
        public string s_cost { get; set; }
        public string s_recv_cost { get; set; }
        public string s_status { get; set; }
        public string s_msg { get; set; }
        public string s_remote_ip { get; set; }
        public Nullable<int> s_source { get; set; }
        public bool is_notify { get; set; }
        public bool is_transfer { get; set; }
    }
}