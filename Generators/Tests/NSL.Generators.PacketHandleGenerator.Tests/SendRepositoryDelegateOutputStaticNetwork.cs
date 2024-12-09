using NSL.Generators.PacketHandleGenerator.Shared;
using NSL.SocketClient;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketServer.Utils;
using NSL.TCP.Client;

namespace NSL.Generators.PacketHandleGenerator.Tests
{
    [NSLPHGenImplement(PacketsEnum = typeof(DevPackets), NetworkDataType = typeof(BasicNetworkClient), Direction = NSLHPDirTypeEnum.Send, Modifier = NSLAccessModifierEnum.Internal | NSLAccessModifierEnum.Static, DelegateOutputResponse = true, IsStaticNetwork = true)]
    internal partial class SendRepositoryDelegateOutputStaticNetwork
    {
        internal static TCPNetworkClient<BasicNetworkClient, ClientOptions<BasicNetworkClient>> client;
        internal static RequestProcessor requestProcessor;

        protected static BasicNetworkClient GetNetworkClient()
            => client.Data;

        protected static NSL.SocketCore.Extensions.Buffer.RequestProcessor GetRequestProcessor()
            => requestProcessor;
    }
}