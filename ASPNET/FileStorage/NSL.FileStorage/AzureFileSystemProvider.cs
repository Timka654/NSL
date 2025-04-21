
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NSL.FileStorage
{
    public class AzureFileSystemProvider(BlobServiceClient blobServiceClient
        , ILogger<AzureFileSystemProvider> logger) : BaseAzureFileSystemProvider(logger)
    {
        protected override BlobServiceClient GetServiceClient() => blobServiceClient;
    }

    public abstract class BaseAzureFileSystemProvider(ILogger logger) : IFileSystemProvider
    {
        private const string tempContainerName = "temp";
        private const string prodContainerName = "public";

        private BlobContainerClient? _tempContainer = null;
        private BlobContainerClient? _prodContainer = null;

        protected abstract BlobServiceClient GetServiceClient();

        private BlobContainerClient GetTempBlobClient()
            => _tempContainer ??= GetServiceClient().GetBlobContainerClient(tempContainerName);

        private BlobContainerClient GetProdBlobClient()
            => _prodContainer ??= GetServiceClient().GetBlobContainerClient(prodContainerName);

        private BlobClient GetBlobClient(BlobContainerClient client, string relativePath)
            => client
                 .GetBlobClient(relativePath);

        public string GetBaseTempUrl()
            => GetTempBlobClient().Uri.ToString();

        public string GetBaseProdUrl()
            => GetProdBlobClient().Uri.ToString();

        public async Task<bool> RemoveProdFile(string relativeProdPath)
        {
            var response = await GetBlobClient(GetProdBlobClient(), relativeProdPath)
                 .DeleteIfExistsAsync();

            return response.GetRawResponse().Status == 200;
        }

        public async Task<bool> RemoveTempFile(string relativeTempPath)
        {
            var response = await GetBlobClient(GetTempBlobClient(), relativeTempPath)
                 .DeleteIfExistsAsync();

            return response.GetRawResponse().Status == 200;
        }

        public async Task<FileSystemUploadResult> TryProduceTempFile(string relativeTempFilePath, string relativeProdFilePath)
        {
            var tempBlob = GetBlobClient(GetTempBlobClient(), relativeTempFilePath);

            if (!await tempBlob.ExistsAsync())
            {
                logger.LogError("Temp file not found: {0}", relativeTempFilePath);
                return new FileSystemUploadResult() { Result = false };
            }

            var releaseBlob = GetBlobClient(GetProdBlobClient(), relativeProdFilePath);

            var responseStart = await releaseBlob
                 .StartCopyFromUriAsync(tempBlob.Uri);

            var response = await responseStart.WaitForCompletionAsync();

            var _response = response.GetRawResponse();

            if (_response.Status != 200)
            {
                logger.LogError("Error copying temp file to prod: {0} - {1}", relativeTempFilePath, _response.Status);
                return new FileSystemUploadResult() { Result = false };
            }

            await tempBlob.DeleteIfExistsAsync();

            return new FileSystemUploadResult()
            {
                Result = true,
                Url = releaseBlob.Uri.ToString()
            };
        }

        public async Task<FileSystemUploadResult> UploadTempFile(Stream stream, string relativeFilePath)
        {
            var blob = GetBlobClient(GetTempBlobClient(), relativeFilePath);

            var response = await blob
                 .UploadAsync(stream, true);

            if (response.GetRawResponse().Status != 201)
                return new FileSystemUploadResult() { Result = false };

            return new FileSystemUploadResult()
            {
                Result = true,
                Url = blob.Uri.ToString()
            };
        }

        public async Task<string> GenerateUploadUrl(string relativeFilePath)
        {
            var blob = GetBlobClient(GetTempBlobClient(), relativeFilePath);

            return blob.GenerateSasUri(BlobSasPermissions.Write | BlobSasPermissions.Create, DateTimeOffset.UtcNow.AddHours(1)).ToString();
        }

        public async Task<string> GenerateDownloadUrl(string relativeFilePath, string? fileName = default)
        {
            var blob = GetBlobClient(GetProdBlobClient(), relativeFilePath);

            var builder = new BlobSasBuilder(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));

            if(fileName != default)
                builder.ContentDisposition = $"attachment; filename*=UTF-8''{Uri.EscapeDataString(fileName)}";
                //builder.ContentDisposition = $"attachment; filename=\"{fileName}\"";

            var url = blob.GenerateSasUri(builder).ToString();

            return url;
        }

        public async Task<FileSystemUploadResult> UploadProdFile(Stream stream, string relativeFilePath)
        {
            var blob = GetBlobClient(GetProdBlobClient(), relativeFilePath);

            var response = await blob
                 .UploadAsync(stream, true);

            if (response.GetRawResponse().Status != 201)
                return new FileSystemUploadResult() { Result = false };

            return new FileSystemUploadResult()
            {
                Result = true,
                Url = blob.Uri.ToString()
            };
        }

        public Task<string> GetTempFileUrl(string relativeTempPath)
            => Task.FromResult(GetBlobClient(GetTempBlobClient(), relativeTempPath).Uri.ToString());

        public Task<string> GetProdFileUrl(string relativeProdPath)
            => Task.FromResult(GetBlobClient(GetProdBlobClient(), relativeProdPath).Uri.ToString());
    }
}
