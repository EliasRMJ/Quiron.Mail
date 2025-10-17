## What is the Quiron.Mail?

Package that allows you to configure and send emails easily in your project

## Give a Star! ⭐

If you find this project useful, please give it a star! It helps us grow and improve the community.

## Namespaces and Dependencies

- ✅ Quiron.Mail
- ✅ MimeKit
- ✅ Quiron.Mail
- ✅ System.Text

## Methods

- ✅ SendMailAsync

## Basic Examples

### Program Configuration
```csharp
builder.Services.AddScoped(typeof(IServerEmail), typeof(ServerEmail));
```

### Appsettings
```csharp
"SMTP": {
    "Host": "mail.seudominio.com",
    "Username": "auth_email@seudominio.com",
    "Password": "Ant@123456",
    "Port": "0"
}
```
⚠️ Add in 'appsettings.json' your project!

### Send Email
```csharp
public class MyClass(IServerEmail serverEmail)
{
    protected override bool UserSsl => true;
    protected override bool ServerCertificateValidation => false;
    protected override SecureSocketOptions SecureSocketOptions => (SecureSocketOptions)int.Parse(configuration["SMTP:SecureSocketOptions"]!);
    protected override string? Host => configuration["SMTP:Host"]!;
    protected override string? UserMail => configuration["SMTP:Usermail"]!;
    protected override string? Password => configuration["SMTP:Password"]!;
    protected override int Port => int.Parse(configuration["SMTP:Port"]!);

    protected override string ContainerHtml(string body)
    {
        return base.ContainerHtml(body);
    }

    public async Task SendMailAsync(ParamEmail from, MailboxAddress to, string subject
        , string message, MailAttachment[] mailAttachments, MessagePriority messagePriority = MessagePriority.Normal)
    {
        await serverEmail.SendMailAsync(from, to, subject, message, mailAttachments, messagePriority);
    }
}
```


## Usage Reference

For more details, access the test project that has the practical implementation of the package's use.

https://github.com/EliasRMJ/Quiron.EntityFrameworkCore.Test

Supports:

- ✅ .NET Standard 2.1  
- ✅ .NET 9 through 9 (including latest versions)  
- ⚠️ Legacy support for .NET Core 3.1 and older (with limitations)
  
## About
Quiron.Mail was developed by [EliasRMJ](https://www.linkedin.com/in/elias-medeiros-98232066/) under the [MIT license](LICENSE).
