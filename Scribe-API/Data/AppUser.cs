using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSAS_Web.Data
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool HasAcceptedEula { get; set; }
        public string Organization { get; set; }
        public string Country { get; set; }
    }
    //public class AppUser : IdentityUser
    //{
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //    public bool RequestGranted { get; set; }
    //    public DateTimeOffset? RequestDate { get; set; }
    //    public bool RequestDenied { get; set; }
    //    public bool HasAcceptedEula { get; set; }
    //    public string GrantedByUser { get; set; }
    //    public string Country { get; set; }
    //    public string Phone { get; set; }
    //    public string Organization { get; set; }
    //    public string Address { get; set; }
    //    public string CompanyType { get; set; }
    //    public string Position { get; set; }
    //    public string UserCount { get; set; }
    //    public bool HasAuthority { get; set; }
    //    public string Purpose { get; set; }
    //    public string Comments { get; set; }
    //}
}
