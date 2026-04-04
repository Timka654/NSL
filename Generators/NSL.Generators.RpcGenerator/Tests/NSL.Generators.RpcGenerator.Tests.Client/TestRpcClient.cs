using NSL.Generators.RpcGenerator.Shared;
using NSL.Generators.RpcGenerator.Shared.Attributes;
using NSL.Generators.RpcGenerator.Tests.Shared;
using NSL.SocketClient;
using NSL.SocketCore.Extensions.Buffer;

namespace NSL.Generators.RpcGenerator.Tests.Client
{
    [NSLRPCImplement(typeof(ITestRpcService), typeof(BasicNetworkClient), Direction = NSLRPCDirection.Client)]
    [NSLRPCImplement(typeof(ITestChatService),  typeof(BasicNetworkClient), Direction = NSLRPCDirection.Client)]
    public partial class TestRpcClient
    {
        private readonly BasicNetworkClient _client;
        private readonly RequestProcessor _requestProcessor;

        public TestRpcClient(BasicNetworkClient client, RequestProcessor requestProcessor)
        {
            _client = client;
            _requestProcessor = requestProcessor;
        }

        protected partial BasicNetworkClient GetNetworkClient() => _client;

        protected partial RequestProcessor GetRequestProcessor() => _requestProcessor;
    }
}
