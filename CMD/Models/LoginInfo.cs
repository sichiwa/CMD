using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CMD.Models
{
    public class LoginInfo
    {
        [Display(Name = "網域帳號")]
        [StringLength(6)]
        [Required]
        public string UserID { get; set; }
        [Display(Name = "網域密碼")]
        [Required]
        public string Pwd { get; set; }
        [Display(Name = "員工憑證登入")]
        public bool UseCertLogin { get; set; }
        public string Plaintext { get; set; }
        public string SignData { get; set; }
    }
}