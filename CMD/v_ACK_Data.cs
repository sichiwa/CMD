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
    
    public partial class v_ACK_Data
    {
        public int 復歸編號 { get; set; }
        public string 監控項目編號 { get; set; }
        public System.DateTime 異常發生時間 { get; set; }
        public string 環境 { get; set; }
        public string 分類 { get; set; }
        public string 系統別 { get; set; }
        public string 監控項目主旨 { get; set; }
        public string s_class { get; set; }
        public string s_type { get; set; }
        public string s_sys { get; set; }
        public bool is_ack { get; set; }
        public string 回報狀態 { get; set; }
        public Nullable<System.DateTime> 異常恢復時間 { get; set; }
        public Nullable<System.DateTime> 復歸時間 { get; set; }
        public string 復歸人員 { get; set; }
        public string 復歸原因分類 { get; set; }
        public string 復歸原因 { get; set; }
        public string id { get; set; }
        public string 系統負責人 { get; set; }
        public string 回報資料 { get; set; }
    }
}