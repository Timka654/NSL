using System.Collections.Generic;
using System.Collections;

namespace NSL.Generators.HttpEndPointGenerator.Shared.Fake.Interfaces
{
    /// <summary>
    /// Fake parameter class for generator success build
    /// </summary>
    public interface IFormFileCollection : IReadOnlyList<IFormFile>, IEnumerable<IFormFile>, IEnumerable, IReadOnlyCollection<IFormFile>
    {
        IFormFile this[string name] { get; }

        IFormFile GetFile(string name);

        IReadOnlyList<IFormFile> GetFiles(string name);
    }
}
