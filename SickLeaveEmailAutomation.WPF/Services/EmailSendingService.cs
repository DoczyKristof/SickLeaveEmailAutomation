using Microsoft.Extensions.Configuration;
using SickLeaveEmailAutomation.WPF.Model;
using System.Text;
using System.Threading.Tasks;

namespace SickLeaveEmailAutomation.WPF.Services
{
    public class EmailSendingService
    {
        private readonly IConfiguration _configuration;
        private GmailService _gmailService;

        public EmailSendingService(IConfiguration configuration)
        {
            _configuration = configuration;

            var email = _configuration["Gmail:MyEmail"];
            var appPassword = _configuration["Gmail:AppPassword"];
            _gmailService = new GmailService(email, appPassword);
        }

        public async Task SendEmail(ScanModel scanModel) 
        {
            string senderName = _configuration["Gmail:MyName"];
            string recipientEmail = _configuration["Gmail:RecipientEmail"];
            string recipientEmail2 = _configuration["Gmail:RecipientEmail2"];
            string recipientEmail3 = _configuration["Gmail:RecipientEmail3"];
            string subject = _configuration["Gmail:Subject"];
            string body = BuildEmailBody();

            await _gmailService.SendEmailAsync(senderName, new[] { recipientEmail, recipientEmail2, recipientEmail3 }, subject, body, scanModel.ImagePath);
        }

        private string BuildEmailBody()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Kedves Címzett!\n");
            sb.Append('\n');
            sb.Append("Csatolva küldöm a tárgyban megjelölt dokumentumot.\n");
            sb.Append('\n');
            sb.Append("Köszönettel:\n");
            sb.Append(_configuration["Gmail:Myname"]);

            return sb.ToString();
        }
    }
}