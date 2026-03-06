using NSL.HttpClient;
using NSL.HttpClient.HttpContent;
using NSL.HttpClient.Models;

namespace NSL.Generators.HttpEndPointGenerator.Tests.Client
{
    internal class Program
    {
        static Stream GetStream1()
            => File.OpenRead("devContent.file");

        static StreamDataContent GetFile1()
            => new HttpClient.HttpContent.StreamDataContent() { FileName = "devContent.file", Stream = GetStream1() };

        static Stream GetStream2()
                => File.OpenRead("devContent2.file");

        static StreamDataContent GetFile2()
            => new HttpClient.HttpContent.StreamDataContent() { FileName = "devContent2.file", Stream = GetStream2() };

        static FillTypeGenerator.Tests.Develop.WithModelName.WithModelName3 GetModelN3()
            => new FillTypeGenerator.Tests.Develop.WithModelName.WithModelName3 { Abc1 = 11, Abc2 = 22 };

        static FillTypeGenerator.Tests.Develop.WithModelName.WithModelName2 GetModelN2()
            => new FillTypeGenerator.Tests.Develop.WithModelName.WithModelName2() { Abc3 = GetModelN3() };

        static FillTypeGenerator.Tests.Develop.WithModelName.WithModelName4 GetModelN4()
         => new FillTypeGenerator.Tests.Develop.WithModelName.WithModelName4()
         {
             Abc3 = GetModelN3()
         };

        static async Task Main(string[] args)
        {
            var testService = new TestService();

            await Task.Delay(2_000);

            BaseResponse response = default;

            var options = BaseHttpRequestOptions.Create(new TestHttpIOProcessor());

            ThrowIfInvalid(response = await testService.TestTestPostPostRequest(GetModelN2(), GetFile1(), options));

            ThrowIfInvalid(response = await testService.TestTestPost2PostRequest(GetModelN3(), options));

            ThrowIfInvalid(response = await testService.TestTestPost3PostRequest(GetModelN3(), options));

            ThrowIfInvalid(response = await testService.TestTestPost4PostRequest(GetModelN3(), GetFile1(), options));

            ThrowIfInvalid(response = await testService.TestTestPost5PostRequest(GetModelN4(), [GetFile1(), GetFile2()], options));

            ThrowIfInvalid(response = await testService.TestTestPost6PostRequest(GetModelN3(), [GetFile1(), GetFile2()], options));

            ThrowIfInvalid(response = await testService.TestTestPost7PostRequest(GetModelN3(), [GetFile1(), GetFile2()], "h1Value", "abc2Value", options));
        }

        static void ThrowIfInvalid(BaseResponse response)
        {
            if (!response.IsSuccess)
                throw new Exception();
        }
    }

    public class TestHttpIOProcessor : IHttpIOProcessor
    {
        public async Task RequestBuildProcess(System.Net.Http.HttpClient client, BaseHttpRequestOptions options, HttpRequestMessage message, Func<Task<HttpContent>> defaultBuildAction, params object[] requestData)
        {
            Console.WriteLine($"{message.Method.ToString()} request to {message.RequestUri} args");

            foreach (var item in requestData)
            {
                Console.WriteLine($"{item}");
            }

            message.Content = await defaultBuildAction();
        }

        public Task ResponsePostProcess(BaseHttpRequestOptions options, HttpResponseMessage httpResponse, BaseResponse response)
        {
            if (httpResponse.RequestMessage == null)
            {
                Console.WriteLine($"request timeout");
            }

            var message = httpResponse.RequestMessage;

            Console.WriteLine($"{message.Method.ToString()} response from {message.RequestUri} - {httpResponse.StatusCode}");

            return Task.CompletedTask;
        }
    }
}
