using GSAS_Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;

namespace GSAS_Web.Data
{
    public class EmailService
    {
        GSAS_Context _context;
        UserManager<AppUser> _userManager;
        HttpContext _httpContext;

        public string FromAddress = "gsas-noreply@sandia.gov";
        public string SmptClientAddress = "mailgate.sandia.gov";
        public int SmptPort = 25;
        private string httpStart = "http";
        public EmailService(GSAS_Context context, UserManager<AppUser> userManager, HttpContext httpContext)
        {
            _context = context;
            _userManager = userManager;
            _httpContext = httpContext;
        }
        internal void SendEmail(string subject, string body, string destinationAddress)
        {

            var message = new MailMessage(FromAddress, destinationAddress);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            var smpt = new SmtpClient(SmptClientAddress, SmptPort);
            smpt.UseDefaultCredentials = true;
            smpt.Send(message);
        }

        internal async Task SendRegisterEmail(AppUser user)
        {
            var baseUrl = new Uri(_httpContext.Request.GetDisplayUrl()).Authority;
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var url = $"{baseUrl}/account/reset-password?email={user.Email}&token={Uri.EscapeDataString(token)}";
            var body = $"<p>You have been registered with an account </p><p>Click <a href=\"{httpStart}://{url}\">here</a> to create your password or copy the following link </p> <p> {httpStart}://{url}</p>";
            try
            {
                SendEmail("GSAS Account Created", body, user.Email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }

        internal async Task SendRequestEmail(string[] emails, RequestModel requestModel)
        {
            string tools = "";
            var request = requestModel.request;
            foreach(var toolId in requestModel.toolIds)
            {
                if (tools != "") tools += ", ";
                Tool tool = await _context.Tool.FindAsync(toolId);
                tools += tool.Name;
            }
            
            string subject = "GSAS Software Request";
            string body = $"<b>GSAS Software Request</b>\n<p><b>Name:</b> {request.FirstName} {request.LastName}</p><p><b>Country of Citizenship:</b> {request.Country}</p><p><b>Email Address:</b> {request.Email}</p><p><b>Phone Number:</b> {request.Phone}</p><p><b>Name of Organization/Company:</b> {request.Organization}<p><b>Address of Company:</b>{request.Address}</p><p><b>Company Type:</b> {request.CompanyType}</p><p><b>Position of Requestor:</b> {request.Position}</p><p><b>User has authority to agree on behalf of company:</b> {(request.HasAuthority ? "True" : "False")}</p><p><b>Number of Interal Users:</b> {request.UserCount}</p><p><b>Are any users non US Persons:</b> {(request.HasNonUsUsers ? "Yes" : "No")}</p><p><b>Software Requested:</b> {tools}</p><p><b>Purpose for using Software:</b> {request.Purpose}</p><p><b>Additional Comments:</b> {request.Comments}";
            foreach(var email in emails)
            {
                try
                {
                    SendEmail(subject, body, email);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error sending email: {ex.Message}");
                }
            }
        }

        internal async Task SendForgotPasswordEmail(AppUser user)
        {
            var baseUrl = new Uri(_httpContext.Request.GetDisplayUrl()).Authority;
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var url = $"{baseUrl}/account/reset-password?email={user.Email}&token={Uri.EscapeDataString(token)}";
            var body = $"<p>You have requested a password change in GSAS.</p><p>Click <a href=\"{httpStart}://{url}\">here</a> to create your password or copy the following link </p> <p> {httpStart}://{url}</p>";
            try
            {
                SendEmail("GSAS Account Change Password", body, user.Email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }
}
