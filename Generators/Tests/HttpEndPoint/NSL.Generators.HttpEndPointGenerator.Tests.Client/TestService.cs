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


namespace Tests
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