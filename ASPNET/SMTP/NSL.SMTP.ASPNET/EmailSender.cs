using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace NSL.SMTP.ASPNET
{
    public class EmailSender(IOptions<SMTPConfigurationModel> options, ILogger<EmailSender> logger, IServiceProvider serviceProvider) : BaseEmailSender<object?>(options, logger, serviceProvider) { }
}
