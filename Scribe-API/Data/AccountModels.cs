using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GSAS_Web.Data
{
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        public string Email { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public bool SendRegisterEmail { get; set; }
        public string ReCaptchaToken { get; set; }

        internal AppUser ToAppUser()
        {
            AppUser user = new AppUser
            {
                UserName = this.Email,
                Email = this.Email,
                FirstName = this.FirstName,
                LastName = this.LastName,
            };
            return user;
        }
    }

    public class WebUserModel
    {
        public string Email { get; set; }
        public string Token { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Organization { get; set; }
        public string Country { get; set; }
        public int? UserGroupId { get; set; }
        public string UserGroup { get; set; }
        public string Role { get; set; }
        public bool HasAcceptedEula { get; set; }
        public string MicrosoftId { get; set; }

        public WebUserModel()
        {
        }

        public WebUserModel(AppUser appUser, string token)
        {
            this.Email = appUser.Email;
            this.Token = token;
            this.FirstName = appUser.FirstName;
            this.LastName = appUser.LastName;
            this.HasAcceptedEula = appUser.HasAcceptedEula;
        }


    }

    public class ConfirmModel
    {
        public string UserId { get; set; }
        public string Token { get; set; }
        public string reCaptchaToken { get; set; }
    }
}
