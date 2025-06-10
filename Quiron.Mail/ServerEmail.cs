using MailKit;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using System.Text;

namespace Quiron.Mail
{
    public class ServerEmail(IConfiguration configuration) : IServerEmail
    {
        protected virtual bool UserSsl => true;
        protected virtual bool ServerCertificateValidation => true;
        protected virtual SecureSocketOptions SecureSocketOptions => SecureSocketOptions.StartTls;

        public async virtual Task SendMailAsync(ParamEmail from, ParamEmail to, string subject
            , string message, MailAttachment[] mailAttachments, MessagePriority messagePriority = MessagePriority.Normal)
        {
            await this.SendMailAsync(from, [new MailboxAddress(to.Name, to.Email)], subject, message, mailAttachments, messagePriority);
        }

        public async virtual Task SendMailAsync(ParamEmail from, InternetAddressList mailboxAddresses, string subject
            , string message, MailAttachment[] mailAttachments, MessagePriority messagePriority = MessagePriority.Normal)
        {
            ArgumentException.ThrowIfNullOrEmpty(configuration["SMTP:Host"]);
            ArgumentException.ThrowIfNullOrEmpty(configuration["SMTP:Username"]);
            ArgumentException.ThrowIfNullOrEmpty(configuration["SMTP:Password"]);

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(from.Name, from.Email));
            mimeMessage.To.AddRange(mailboxAddresses);
            mimeMessage.Subject = subject;
            mimeMessage.Body = new TextPart(TextFormat.Html) { Text = message };
            mimeMessage.Priority = messagePriority;

            if (mailAttachments is not null)
            {
                foreach (var att in mailAttachments)
                {
                    var mimePart = new MimePart("text", "html")
                    {
                        ContentTransferEncoding = ContentEncoding.Base64,
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment) { FileName = att.FileName },
                        Content = new MimeContent(new MemoryStream(Encoding.UTF8.GetBytes(att.Base64)))
                    };

                    _ = mimeMessage.Attachments.Append(mimePart);
                }
            }

            using var client = new MailKit.Net.Smtp.SmtpClient();
            if (this.ServerCertificateValidation)
                client.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => { return this.ServerCertificateValidation; };

            try
            {
                int port = configuration["SMTP:Port"] is not null ? int.Parse(configuration["SMTP:Port"]!) : 0;
                await client.ConnectAsync(configuration["SMTP:Host"], port, this.SecureSocketOptions);
                await client.AuthenticateAsync(configuration["SMTP:Usermail"], configuration["SMTP:Password"]);

                await client.SendAsync(mimeMessage);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException("Argument null exception."
                    , ex);
            }
            catch (ObjectDisposedException ex)
            {
                throw new ObjectDisposedException("Object disposed exception."
                    , ex);
            }
            catch (ServiceNotConnectedException ex)
            {
                throw new ServiceNotConnectedException("Service not connected exception."
                    , ex);
            }
            catch (ServiceNotAuthenticatedException ex)
            {
                throw new ServiceNotAuthenticatedException("Service not authenticated exception."
                    , ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Invalid operation exception."
                    , ex);
            }
            catch (OperationCanceledException ex)
            {
                throw new OperationCanceledException("Operation canceled exception."
                    , ex);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}