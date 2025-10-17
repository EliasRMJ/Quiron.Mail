using MailKit;
using MailKit.Security;
using MimeKit;
using System.Text;

namespace Quiron.Mail
{
    public class ServerEmail : IServerEmail
    {
        protected virtual bool UserSsl { get; set; } = true;
        protected virtual bool ServerCertificateValidation { get; set; } = true;
        protected virtual SecureSocketOptions SecureSocketOptions { get; set; } = SecureSocketOptions.StartTls;
        protected virtual string? Host { get; set; } = string.Empty;
        protected virtual string? UserMail { get; set; } = string.Empty;
        protected virtual string? Password { get; set; } = string.Empty;
        protected virtual int Port { get; set; } = 0;

        protected virtual string ContainerHtml(string body)
        {
            return body;
        }

        public async virtual Task SendMailAsync(ParamEmail from, ParamEmail to, string subject
            , string message, MailAttachment[]? mailAttachments = null, MessagePriority messagePriority = MessagePriority.Normal)
        {
            await this.SendMailAsync(from, [new MailboxAddress(to.Name, to.Email)], subject, message, mailAttachments, messagePriority);
        }

        public async virtual Task SendMailAsync(ParamEmail from, InternetAddressList mailboxAddresses, string subject
            , string message, MailAttachment[]? mailAttachments = null, MessagePriority messagePriority = MessagePriority.Normal)
        {
            ArgumentException.ThrowIfNullOrEmpty(this.Host);
            ArgumentException.ThrowIfNullOrEmpty(this.UserMail);
            ArgumentException.ThrowIfNullOrEmpty(this.Password);

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(from.Name, from.Email));
            mimeMessage.To.AddRange(mailboxAddresses);
            mimeMessage.Subject = subject;
            mimeMessage.Body = new BodyBuilder { HtmlBody = this.ContainerHtml(message) }.ToMessageBody();
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
                await client.ConnectAsync(this.Host, this.Port, this.SecureSocketOptions);
                await client.AuthenticateAsync(this.UserMail, this.Password);

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