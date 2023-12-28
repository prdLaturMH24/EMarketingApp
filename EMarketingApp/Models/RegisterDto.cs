using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EMarketingApp.Models
{
    public class RegisterDto
    {
        public string UserName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [PasswordPropertyText]
        public string Password { get; set; }
        public string PhotoUrl { get; set; }
        [Phone]
        public string Phone { get; set; }
    }
}