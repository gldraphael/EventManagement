using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using F = System.IO.File;

using losol.EventManagement.Services;
using losol.EventManagement.Services.Messaging;
using losol.EventManagement.ViewModels;
using losol.EventManagement.Web.Services;
using losol.EventManagement.Web.ViewModels.Templates;
using Microsoft.AspNetCore.Authorization;

namespace losol.EventManagement.Web.Controllers.Api
{
    [Authorize(Policy = "AdministratorRole")]
    [Route("api/participants")]
    public class ParticipantsController : Controller
    {
        [HttpPost("email_certificates/for_event/{eventId}")]
        public async Task<IActionResult> GenerateCertificatesAndSendEmails([FromRoute]int eventId,
            [FromServices] IRegistrationService registrationService,
            [FromServices] CertificateWriter writer, 
            [FromServices] StandardEmailSender emailSender)
        {
            var certificates = await registrationService.CreateNewCertificates(eventId, User.Identity.Name);
            var emailTasks = certificates.Select(async c => {
                string filename = $"{DateTime.Now.ToString("u")}.pdf";
                var result = await writer.Write(filename, CertificateVM.From(c));
                var bytes = await F.ReadAllBytesAsync(writer.GetPathForFile(filename));
                return emailSender.SendAsync(new EmailMessage {
                    Email = c.RecipientUser.Email,
                    Subject = $"Certificate for {c.Title}",
                    Message = "Here's your certificate! Congratulations!", // TODO: Get this right
                    Attachment = new Attachment { Filename = "certificate.pdf", Bytes = bytes }
                });
            });
            await Task.WhenAll(emailTasks);
            return Ok(certificates.Select(c => new { c.CertificateId }));
        }

    }
}
