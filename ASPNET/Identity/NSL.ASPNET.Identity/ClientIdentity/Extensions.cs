using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NSL.ASPNET.Identity.ClientIdentity.MessageHandlers;
using NSL.ASPNET.Identity.ClientIdentity.Providers;
using NSL.ASPNET.Identity.ClientIdentity.Providers.Options;

namespace NSL.ASPNET.Identity.ClientIdentity
{
    public static class Extensions
    {
        public static IServiceCollection AddIdentityStateProvider<TProvider>(this IServiceCollection services)
            where TProvider : IdentityStateProvider
        {
            services.AddSingleton<TProvider>();

            if (typeof(TProvider) != typeof(IdentityStateProvider))
                services.AddSingleton<IdentityStateProvider>(s => s.GetRequiredService<TProvider>());

            return services.AddSingleton<AuthenticationStateProvider>(s => s.GetRequiredService<TProvider>());
        }

        public static IServiceCollection AddIdentityPolicyProvider<TProvider>(this IServiceCollection services)
            where TProvider : IdentityPolicyProvider
        {
            services.AddSingleton<TProvider>();

            if (typeof(TProvider) != typeof(IdentityPolicyProvider))
                services.AddSingleton<IdentityPolicyProvider>(s => s.GetRequiredService<TProvider>());

            return services.AddSingleton<IAuthorizationPolicyProvider>(s => s.GetRequiredService<TProvider>());
        }

        public static IServiceCollection AddIdentityPolicyProvider<TProvider>(this IServiceCollection services, Action<IdentityPolicyOptions> buildPolicy)
            where TProvider : IdentityPolicyProvider
        {
            IdentityPolicyOptions p = IdentityPolicyOptions.Create();

            buildPolicy(p);

            return services.AddIdentityPolicyProvider<TProvider>(p);
        }

        public static IServiceCollection AddIdentityPolicyProvider<TProvider>(this IServiceCollection services, IdentityPolicyOptions policy)
            where TProvider : IdentityPolicyProvider
        {
            services.AddSingleton(policy);

            return services.AddIdentityPolicyProvider<TProvider>();
        }

        /// <summary>
        /// call <see cref="AddIdentityStateProvider"/> before register identity service
        /// call <see cref="AddIdentityPolicyProvider"/> before register identity service
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddIdentityService<TService>(this IServiceCollection services)
            where TService : IdentityService
        {
            services.AddSingleton<TService>();

            if (typeof(TService) != typeof(IdentityService))
                services.AddSingleton<IdentityService>(s => s.GetRequiredService<TService>());

            return services;
        }

        /// <summary>
        /// call <see cref="AddIdentityPolicyProvider"/> before register identity authorization service
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddIdentityAuthorizationService<TService>(this IServiceCollection services)
            where TService : IdentityAuthorizationService
        {
            services.AddScoped<TService>();

            if (typeof(TService) != typeof(IdentityAuthorizationService))
                services.AddScoped<IdentityAuthorizationService>(s => s.GetRequiredService<TService>());

            return services.AddScoped<IAuthorizationService>(x => x.GetRequiredService<TService>());
        }

        public static HttpClient FillHttpClientIdentity(this HttpClient client, IServiceProvider sp)
            => sp.GetRequiredService<IdentityService>().FillHttpClient(client);

        /// <summary>
        /// Make sure you register this handler in services with <see cref="AddHttpMessageIdentityHandler"/> method
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddHttpMessageIdentityHandler(this IHttpClientBuilder builder)
            => builder.AddHttpMessageHandler<HttpMessageIdentityHandler>();

        public static IServiceCollection AddHttpMessageIdentityHandler(this IServiceCollection services)
            => services.AddTransient<HttpMessageIdentityHandler>();

        public static async Task LoadIdentityAsync(this IServiceProvider services)
            => await services.LoadIdentityAsync<IdentityService>();

        public static async Task LoadIdentityAsync<TIService>(this IServiceProvider services)
            where TIService : IdentityService
            => await services.GetRequiredService<TIService>().LoadIdentity();
    }
}
