using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class GmailService
{
    static string[] Scopes = { Google.Apis.Gmail.v1.GmailService.Scope.GmailSend };
    static string ApplicationName = "Sick Leave Emailing Handler";
    private readonly string _credentialsPath;

    public GmailService(string credentialsPath)
    {
        _credentialsPath = credentialsPath;
    }

    public async Task<UserCredential> GetUserCredentialAsync()
    {
        using (var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read))
        {
            string credPath = "token.json";
            return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true));
        }
    }

    public async Task SendEmailAsync(string senderName, string senderEmail, string recipient, string subject, string body, string attachmentPath)
    {
        var credential = await GetUserCredentialAsync();

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(senderName, senderEmail));
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
            await client.ConnectAsync("smtp.gmail.com", 587, false);
            var oauth2 = new SaslMechanismOAuth2(credential.UserId, credential.Token.AccessToken);
            await client.AuthenticateAsync(oauth2);
            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);
        }
    }
}
