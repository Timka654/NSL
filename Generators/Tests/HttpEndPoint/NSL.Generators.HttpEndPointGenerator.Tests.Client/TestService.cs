using NSL.Generators.HttpEndPointGenerator.Shared.Attributes;
using NSL.Generators.HttpEndPointGenerator.Tests.Shared;


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