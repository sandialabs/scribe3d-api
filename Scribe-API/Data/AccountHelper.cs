using GSAS_Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GSAS_Web.Data
{
    public class AccountHelper
    {

        public string GenerateJSONWebToken(AppUser userInfo, IConfiguration config)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
             new Claim(JwtRegisteredClaimNames.UniqueName, userInfo.UserName),
             new Claim(JwtRegisteredClaimNames.Email, userInfo.Email),
             //new Claim(JwtRegisteredClaimNames.GivenName, userInfo.FirstName),
             //new Claim(JwtRegisteredClaimNames.FamilyName, userInfo.LastName),
             new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
             };

            var token = new JwtSecurityToken(config["Jwt:Issuer"],
            config["Jwt:Issuer"],
            claims,
            expires: DateTime.Now.AddMinutes(120),
            signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        internal static bool IsValidRole(string role)
        {
            var roles = Enum.GetValues(typeof(AppRole)).Cast<AppRole>().Select(w => w.ToString());
            return roles.Contains(role);
        }


        public static string SanitizeToken(string token)
        {
            return token.Trim().Replace(" ", "+");
        }

    }
}
