using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NSL.DataSource.ASPNET
{
    public static class RestoreDataSourceExtensions
    {
        public static IServiceCollection ConfigureLocalFolderRestoreDataSource(this IServiceCollection services, IConfiguration configuration, string path = "RestoreData")
            => services.Configure<LocalFolderRestoreDataConfigurationModel>(configuration.GetSection(path));

        public static IServiceCollection AddLocalFolderRestoreDataSourceSingleton(this IServiceCollection services)
            => services.AddRestoreDataSingleton<LocalFolderRestoreDataSource>();

        public static IServiceCollection AddLocalFolderRestoreDataSourceSingleton(this IServiceCollection services, IConfiguration configuration, string path = "RestoreData")
            => services.ConfigureLocalFolderRestoreDataSource(configuration, path)
            .AddLocalFolderRestoreDataSourceSingleton();

        public static IServiceCollection AddRestoreDataSingleton<TSource>(this IServiceCollection services)
            where TSource : class, IRestoreDataSource
        {
            services.AddSingleton<TSource>();
            services.AddSingleton(s => s.GetRequiredService<TSource>() as IRestoreDataSource);

            return services;
        }


    }
}
