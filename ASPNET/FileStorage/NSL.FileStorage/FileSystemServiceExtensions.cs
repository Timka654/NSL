using Microsoft.Extensions.DependencyInjection;

namespace NSL.FileStorage
{
    public static class FileSystemServiceExtensions
    {
        public static IServiceCollection AddAzureServiceClient(this IServiceCollection services, string connectionString)
            => services.AddSingleton(s => new Azure.Storage.Blobs.BlobServiceClient(connectionString));

        public static IServiceCollection AddAzureFileSystem(this IServiceCollection services)
            => services.AddSingleton<AzureFileSystemProvider>()
            .AddSingleton(x => x.GetRequiredService<AzureFileSystemProvider>() as IFileSystemProvider);

        public static IServiceCollection AddLocalFileSystem(this IServiceCollection services, string prodUrl = "", string prodPath = "wwwroot/", string tempUrl = "Temp", string tempPath = "wwwroot/Temp")
            => services.AddSingleton(x => new LocalFileSystemProvider(prodUrl, prodPath, tempUrl, tempPath))
            .AddSingleton(x => x.GetRequiredService<LocalFileSystemProvider>() as IFileSystemProvider);

    }
}
