using MailKit;
using MailKit.Security;
using MimeKit;

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
            mimeMessage.Priority = messagePriority;

            var bodyBuilder = new BodyBuilder { HtmlBody = this.ContainerHtml(message) };

            if (mailAttachments is not null)
            {
                foreach (var att in mailAttachments)
                {
                    try
                    {
                        var fileBytes = Convert.FromBase64String(att.Base64);
                        var contentType = MimeTypes.GetMimeType(att.FileName);

                        bodyBuilder.Attachments.Add(att.FileName, fileBytes, ContentType.Parse(contentType));
                    }
                    catch (FormatException)
                    {
                        throw new FormatException($"The Base64 content of attachment '{att.FileName}' is invalid.");
                    }
                }
            }

            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();

            if (this.ServerCertificateValidation)
                client.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;

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