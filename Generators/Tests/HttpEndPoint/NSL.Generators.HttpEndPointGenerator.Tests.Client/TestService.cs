using NSL.Generators.HttpEndPointGenerator.Shared.Attributes;
using NSL.Generators.HttpEndPointGenerator.Tests.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using NSL.HttpClient;
using NSL.HttpClient.Models;
using NSL.HttpClient;
using NSL.HttpClient.HttpContent;
using NSL.HttpClient.Models;
using NSL.HttpClient.Validators;

using NSL.Generators.HttpEndPointGenerator.Shared.Attributes;
using NSL.Generators.HttpEndPointGenerator.Tests.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using NSL.HttpClient;
using NSL.HttpClient.Models;


namespace NSL.Generators.HttpEndPointGenerator.Tests.Client
{
    [HttpEndPointImplementGenerate(typeof(ITestController))]
    public partial class TestService
    {
        protected partial System.Net.Http.HttpClient CreateEndPointClient(string url)
        {
            return new System.Net.Http.HttpClient() { BaseAddress = new Uri("https://localhost:60673") };
        }
    }
}