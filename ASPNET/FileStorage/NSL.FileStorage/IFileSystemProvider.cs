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
}
