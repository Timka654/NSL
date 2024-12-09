using System.Collections.Generic;
using System.Collections;
using Microsoft.Extensions.Primitives;

namespace NSL.Generators.HttpEndPointGenerator.Shared.Fake.Interfaces
{
    /// <summary>
    /// Fake class for generator success build
    /// </summary>
    public interface IHeaderDictionary : IDictionary<string, StringValues>, ICollection<KeyValuePair<string, StringValues>>, IEnumerable<KeyValuePair<string, StringValues>>, IEnumerable
    {

    }
}
