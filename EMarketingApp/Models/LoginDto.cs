using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace EMarketingApp.Models
{
    public class LoginDto
    {
        public string Username { get; set; }
        [PasswordPropertyText]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}