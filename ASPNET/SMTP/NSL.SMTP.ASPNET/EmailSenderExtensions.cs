using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NSL.SMTP.ASPNET
{
    public static class EmailSenderExtensions
    {
        public static IServiceCollection ConfigureEmailSender<TConfiguration>(this IServiceCollection services, IConfiguration configuration, string path = "SMTP")
            where TConfiguration : SMTPConfigurationModel
            => services.Configure<TConfiguration>(configuration.GetSection(path));

        public static IServiceCollection ConfigureEmailSender(this IServiceCollection services, IConfiguration configuration, string path = "SMTP")
            => services.ConfigureEmailSender<SMTPConfigurationModel>(configuration, path);



        /// <summary>
        /// Register default <see cref="EmailSender"/> sender
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddEmailSender(this IServiceCollection services)
            => services.AddEmailSender<EmailSender>();

        /// <summary>
        /// Register default <see cref="EmailSender"/> sender, with configuration
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration, string path = "SMTP")
            => services.ConfigureEmailSender(configuration, path)
            .AddEmailSender<EmailSender>();

        public static IServiceCollection AddEmailSender<TSender>(this IServiceCollection services)
            where TSender : BaseEmailSender
        {
            services.AddSingleton<TSender>();
            services.AddSingleton(x => x.GetRequiredService<TSender>() as IEmailSender);

            if (typeof(TSender).IsAssignableTo(typeof(IHostedService)))
                services.AddHostedService(x => x.GetRequiredService<TSender>());

            return services;
        }
    }
}
