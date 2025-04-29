using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace NSL.FileStorage
{
    public class LocalFileSystemProvider(string prodUrl, string prodPath, string tempUrl, string tempPath) : BaseLocalFileSystemProvider
    {
        public override string GetBaseProdUrl()
            => prodUrl;

        public override string GetBaseProdPath()
            => prodPath;

        public override string GetBaseTempUrl()
            => tempUrl;

        public override string GetBaseTempPath()
            => tempPath;
    }

    public abstract class BaseLocalFileSystemProvider : IFileSystemProvider
    {
        public abstract string GetBaseProdUrl();

        public abstract string GetBaseProdPath();

        public abstract string GetBaseTempUrl();

        public abstract string GetBaseTempPath();

        public Task<bool> RemoveTempFile(string relativeTempPath)
        {
            var path = Path.Combine(GetBaseTempPath(), relativeTempPath);

            var fi = new FileInfo(path);

            if (fi.Exists)
                fi.Delete();

            return Task.FromResult(true);
        }

        public Task<bool> RemoveProdFile(string relativeTempPath)
        {
            var path = Path.Combine(GetBaseProdPath(), relativeTempPath);

            var fi = new FileInfo(path);

            if (fi.Exists)
                fi.Delete();

            return Task.FromResult(true);
        }

        public async Task<FileSystemUploadResult> TryProduceTempFile(string relativeTempFilePath, string relativeProdFilePath)
        {
            var tempPath = Path.Combine(GetBaseTempPath(), relativeTempFilePath.TrimStart('/', '\\'));

            var fi = new FileInfo(tempPath);

            if (!fi.Exists)
                return new FileSystemUploadResult() { Result = false };

            var destPath = Path.Combine(GetBaseProdPath(), relativeProdFilePath.TrimStart('/', '\\'));

            var destfi = new FileInfo(destPath);

            if (!destfi.Directory.Exists)
                destfi.Directory.Create();

            fi.MoveTo(destfi.FullName, true);

            var url = string.Join("/", GetBaseProdUrl().TrimStart('/'), Path.GetRelativePath(GetBaseProdPath(), destPath).Replace('\\', '/'));

            return new FileSystemUploadResult()
            {
                Result = true,
                Url = url
            };

        }

        public async Task<FileSystemUploadResult> UploadTempFile(Stream stream, string relativeFilePath)
        {
            var tempPath = Path.Combine(GetBaseTempPath(), relativeFilePath.TrimStart('/', '\\'));

            var fi = new FileInfo(tempPath);

            if (!fi.Directory.Exists)
                fi.Directory.Create();

            using var resultStream = fi.Create();

            stream.CopyTo(resultStream);

            var url = "/" + string.Join("/", GetBaseTempUrl(), Path.GetRelativePath(GetBaseTempPath(), fi.FullName).Replace('\\', '/'));

            return new FileSystemUploadResult()
            {
                Result = true,
                Url = Path.GetRelativePath(GetBaseTempPath(), fi.FullName).Replace('\\', '/')
            };
        }

        public async Task<FileSystemUploadResult> UploadProdFile(Stream stream, string relativeFilePath)
        {
            var prodPath = Path.Combine(GetBaseProdPath(), relativeFilePath.TrimStart('/', '\\'));

            var fi = new FileInfo(prodPath);

            if (!fi.Directory.Exists)
                fi.Directory.Create();

            using var resultStream = fi.Create();

            stream.CopyTo(resultStream);

            var url = "/" + string.Join("/", GetBaseProdUrl(), Path.GetRelativePath(GetBaseProdPath(), fi.FullName).Replace('\\', '/'));

            return new FileSystemUploadResult()
            {
                Result = true,
                Url = url
            };
        }

        public Task<string> GetTempFileUrl(string relativeTempPath)
            => Task.FromResult(Path.GetRelativePath(GetBaseTempPath(), relativeTempPath).Replace('\\', '/'));

        public Task<string> GetProdFileUrl(string relativeProdPath)
            => Task.FromResult(Path.GetRelativePath(GetBaseProdPath(), relativeProdPath).Replace('\\', '/'));

        public Task<string> GenerateUploadUrl(string relativeFilePath)
        {
            var tempPath = Path.Combine(GetBaseTempPath(), relativeFilePath);

            var fi = new FileInfo(tempPath);

            if (!fi.Directory.Exists)
                fi.Directory.Create();

            return Task.FromResult(Convert.ToBase64String(Encoding.UTF8.GetBytes(relativeFilePath)));
        }

        public Task<string> GenerateDownloadUrl(string relativeFilePath, string? fileName = default)
        {
            var tempPath = Path.Combine(GetBaseTempPath(), relativeFilePath);

            var url = Convert.ToBase64String(Encoding.UTF8.GetBytes(relativeFilePath));

            if (fileName != default)
                url += $"?fileName={UrlEncoder.Default.Encode(fileName)}";

            return Task.FromResult(url);
        }
    }
}
