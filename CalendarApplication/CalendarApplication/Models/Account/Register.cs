using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CalendarApplication.Models.Account
{
    public class Register
    {
        
        [StringLength(45, ErrorMessage = "The {0} field must be less than {1} characters long.")]
        [Display(Name = "Your real name")]
        public string RealName { get; set; }

        [StringLength(45, ErrorMessage = "The {0} field must be at less than {1} characters.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [StringLength(45, ErrorMessage = "The {0} field must be at less than {1} characters.")]
        [Display(Name = "Phone number")]
        public string Phone { get; set; }

        [Required]
        [StringLength(45, ErrorMessage = "The {0} field must be less than {1} characters long.")]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} field must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name = "Please retype your password")]
        public string PasswordConfirm { get; set; }
    }
}