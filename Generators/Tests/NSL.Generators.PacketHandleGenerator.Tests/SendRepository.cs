using NSL.Generators.PacketHandleGenerator.Shared;
using NSL.SocketClient;
using NSL.SocketCore;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketServer.Utils;
using NSL.TCP.Client;

namespace NSL.Generators.PacketHandleGenerator.Tests
{
    [NSLPHGenImplDefaults(PacketsEnum = typeof(DevPackets)
        , NetworkDataType = typeof(BaseServerNetworkClient)
        , Modifier = NSLAccessModifierEnum.Internal | NSLAccessModifierEnum.Static
        , IsStaticNetwork = true)]
    [NSLPHGenImpl(Direction = NSLHPDirTypeEnum.Send)]
    internal partial class StaticSendRepository
    {
        internal static TCPNetworkClient<BasicNetworkClient, ClientOptions<BasicNetworkClient>> client;
        internal static RequestProcessor requestProcessor;

        protected static BasicNetworkClient GetNetworkClient()
            => client.Data;

        protected static NSL.SocketCore.Extensions.Buffer.RequestProcessor GetRequestProcessor()
            => requestProcessor;
    }

    [NSLPHGenImplDefaults(PacketsEnum = typeof(DevPackets)
        , NetworkDataType = typeof(BaseServerNetworkClient)
        , Modifier = NSLAccessModifierEnum.Internal | NSLAccessModifierEnum.Static
        , IsAsync = true
        , IsStaticNetwork = true)]
    [NSLPHGenImpl(Direction = NSLHPDirTypeEnum.Send)]
    internal partial class StaticAsyncSendRepository
    {
        internal static TCPNetworkClient<BasicNetworkClient, ClientOptions<BasicNetworkClient>> client;
        internal static RequestProcessor requestProcessor;

        protected static BasicNetworkClient GetNetworkClient()
            => client.Data;

        protected static NSL.SocketCore.Extensions.Buffer.RequestProcessor GetRequestProcessor()
            => requestProcessor;
    }

    [NSLPHGenImplDefaults(PacketsEnum = typeof(DevPackets)
        , NetworkDataType = typeof(BaseServerNetworkClient)
        , Modifier = NSLAccessModifierEnum.Internal | NSLAccessModifierEnum.Static
        , IsAsync = true)]
    [NSLPHGenImpl(Direction = NSLHPDirTypeEnum.Send)]
    internal partial class AsyncSendRepository
    {
    }


    [NSLPHGenImplDefaults(PacketsEnum = typeof(DevPackets)
        , NetworkDataType = typeof(BaseServerNetworkClient)
        , Modifier = NSLAccessModifierEnum.Internal | NSLAccessModifierEnum.Static)]
    [NSLPHGenImpl(Direction = NSLHPDirTypeEnum.Send)]
    internal partial class SendRepository
    {

    }

    [NSLPHGenImplDefaults(PacketsEnum = typeof(DevPackets)
        , NetworkDataType = typeof(BaseServerNetworkClient)
        , Modifier = NSLAccessModifierEnum.Internal | NSLAccessModifierEnum.Static
        , IsStaticNetwork = true
        , DelegateOutputResponse = true)]
    [NSLPHGenImpl(Direction = NSLHPDirTypeEnum.Send)]
    internal partial class StaticDelegateSendRepository
    {
        internal static TCPNetworkClient<BasicNetworkClient, ClientOptions<BasicNetworkClient>> client;
        internal static RequestProcessor requestProcessor;

        protected static BasicNetworkClient GetNetworkClient()
            => client.Data;

        protected static NSL.SocketCore.Extensions.Buffer.RequestProcessor GetRequestProcessor()
            => requestProcessor;
    }

    [NSLPHGenImplDefaults(PacketsEnum = typeof(DevPackets)
        , NetworkDataType = typeof(BaseServerNetworkClient)
        , Modifier = NSLAccessModifierEnum.Internal | NSLAccessModifierEnum.Static
        , IsAsync = true
        , IsStaticNetwork = true
        , DelegateOutputResponse = true)]
    [NSLPHGenImpl(Direction = NSLHPDirTypeEnum.Send)]
    internal partial class StaticAsyncDelegateSendRepository
    {
        internal static TCPNetworkClient<BasicNetworkClient, ClientOptions<BasicNetworkClient>> client;
        internal static RequestProcessor requestProcessor;

        protected static BasicNetworkClient GetNetworkClient()
            => client.Data;

        protected static NSL.SocketCore.Extensions.Buffer.RequestProcessor GetRequestProcessor()
            => requestProcessor;
    }

    [NSLPHGenImplDefaults(PacketsEnum = typeof(DevPackets)
        , NetworkDataType = typeof(BaseServerNetworkClient)
        , Modifier = NSLAccessModifierEnum.Internal | NSLAccessModifierEnum.Static
        , IsAsync = true
        , DelegateOutputResponse = true)]
    [NSLPHGenImpl(Direction = NSLHPDirTypeEnum.Send)]
    internal partial class AsyncDelegateSendRepository
    {
    }


    [NSLPHGenImplDefaults(PacketsEnum = typeof(DevPackets)
        , NetworkDataType = typeof(BaseServerNetworkClient)
        , Modifier = NSLAccessModifierEnum.Internal | NSLAccessModifierEnum.Static
        , DelegateOutputResponse = true)]
    [NSLPHGenImpl(Direction = NSLHPDirTypeEnum.Send)]
    internal partial class DelegateSendRepository
    {

    }

}