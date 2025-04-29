using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;

namespace NSL.FileStorage
{
    public interface IFileSystemProvider
    {
        Task<FileSystemUploadResult> UploadTempFile(Stream stream, string relativeFilePath);

        Task<FileSystemUploadResult> UploadProdFile(Stream stream, string relativeFilePath);

        Task<FileSystemUploadResult> TryProduceTempFile(string relativeTempFilePath, string relativeProdFilePath);

        Task<bool> RemoveTempFile(string relativeTempPath);

        Task<bool> RemoveProdFile(string relativeProdPath);

        Task<string> GetTempFileUrl(string relativeTempPath);

        Task<string> GetProdFileUrl(string relativeProdPath);

        string GetBaseTempUrl();

        string GetBaseProdUrl();

        Task<string> GenerateUploadUrl(string relativeFilePath);

        Task<string> GenerateDownloadUrl(string relativeFilePath, string? fileName = default);
    }

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
