using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.IO;
using System.Threading.Tasks;

public class GmailService
{
    private readonly string _senderEmail;
    private readonly string _appPassword;

    public GmailService(string senderEmail, string appPassword)
    {
        _senderEmail = senderEmail;
        _appPassword = appPassword;
    }

    public async Task SendEmailAsync(string senderName, string recipient, string subject, string body, string attachmentPath)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(senderName, _senderEmail));
        mimeMessage.To.Add(new MailboxAddress("", recipient));
        mimeMessage.Subject = subject;

        var bodyPart = new TextPart("plain") { Text = body };
        var attachment = new MimePart("image", "jpeg")
        {
            Content = new MimeContent(File.OpenRead(attachmentPath), ContentEncoding.Default),
            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
            ContentTransferEncoding = ContentEncoding.Base64,
            FileName = Path.GetFileName(attachmentPath)
        };

        var multipart = new Multipart("mixed");
        multipart.Add(bodyPart);
        multipart.Add(attachment);

        mimeMessage.Body = multipart;

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_senderEmail, _appPassword);
            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);
        }
    }
}