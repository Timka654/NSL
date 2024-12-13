using NSL.Generators.PacketHandleGenerator.Shared;
using NSL.SocketClient;
using NSL.SocketCore.Extensions.Buffer;
using NSL.TCP.Client;

namespace NSL.Generators.PacketHandleGenerator.Tests
{
    [NSLPHGenImplement(PacketsEnum = typeof(DevPackets), NetworkDataType = typeof(BasicNetworkClient), Direction = NSLHPDirTypeEnum.Send, Modifier = NSLAccessModifierEnum.Internal | NSLAccessModifierEnum.Static, DelegateOutputResponse = false, IsStaticNetwork = true)]
    internal partial class SendRepositoryOutputStaticNetwork
    {
        internal static TCPNetworkClient<BasicNetworkClient, ClientOptions<BasicNetworkClient>> client;
        internal static RequestProcessor requestProcessor;

        protected static BasicNetworkClient GetNetworkClient()
            => client.Data;

        protected static NSL.SocketCore.Extensions.Buffer.RequestProcessor GetRequestProcessor()
            => requestProcessor;
    }
}