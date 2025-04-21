using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Runtime.Intrinsics.Arm;

namespace NSL.SMTP.ASPNET
{
    public class EmailSender(IOptions<SMTPConfigurationModel> options, ILogger<EmailSender> logger, IServiceProvider serviceProvider) : BaseEmailSender(options, logger, serviceProvider) { }
}
