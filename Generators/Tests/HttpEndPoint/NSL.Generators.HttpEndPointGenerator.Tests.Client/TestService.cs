using NSL.Generators.HttpEndPointGenerator.Attributes;
using NSL.Generators.HttpEndPointGenerator.Tests.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.HttpEndPointGenerator.Tests.Client
{
    [HttpEndPointImplementGenerate(typeof(ITestController))]
    public partial class TestService
    {
        protected partial HttpClient CreateEndPointClient(string url)
        {
            return null;
        }
    }
}
