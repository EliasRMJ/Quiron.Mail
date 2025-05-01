using MimeKit;
namespace Quiron.Mail
{
    public interface IServerEmail
    {
        Task SendMailAsync(ParamEmail from, ParamEmail to, string subject, string message
            , MailAttachment[] mailAttachments, MessagePriority messagePriority = MessagePriority.Normal);
        Task SendMailAsync(ParamEmail from, InternetAddressList mailboxAddresses, string subject, string message
            , MailAttachment[] mailAttachments, MessagePriority messagePriority = MessagePriority.Normal);
    }
}