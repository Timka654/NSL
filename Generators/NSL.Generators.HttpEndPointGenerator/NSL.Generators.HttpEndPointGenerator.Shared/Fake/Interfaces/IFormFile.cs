using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace NSL.Generators.HttpEndPointGenerator.Shared.Fake.Interfaces
{
    /// <summary>
    /// Fake parameter class for generator success build
    /// </summary>
    public interface IFormFile
    {
        string ContentType { get; }

        string ContentDisposition { get; }

        IHeaderDictionary Headers { get; }

        long Length { get; }

        string Name { get; }

        string FileName { get; }

        void CopyTo(Stream target);

        Task CopyToAsync(Stream target, CancellationToken cancellationToken = default);

        Stream OpenReadStream();
    }
}
