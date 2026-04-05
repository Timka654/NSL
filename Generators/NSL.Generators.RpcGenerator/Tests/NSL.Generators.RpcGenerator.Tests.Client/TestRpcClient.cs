using NSL.Generators.RpcGenerator.Shared;
using NSL.Generators.RpcGenerator.Shared.Attributes;
using NSL.Generators.RpcGenerator.Tests.Shared;
using NSL.SocketClient;

namespace NSL.Generators.RpcGenerator.Tests.Client
{
    [NSLRPCImplement(typeof(ITestRpcService), typeof(BasicNetworkClient), Direction = NSLRPCDirection.Client)]
    [NSLRPCImplement(typeof(ITestChatService),  typeof(BasicNetworkClient), Direction = NSLRPCDirection.Client)]
    public partial class TestRpcClient
    {
        private readonly IRPCNetworkChannel _channel;

        public TestRpcClient(IRPCNetworkChannel channel)
        {
            _channel = channel;
        }

        protected partial IRPCNetworkChannel GetRpcChannel() => _channel;
    }
}
