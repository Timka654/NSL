using NSL.Generators.PacketHandleGenerator.Shared;
using NSL.SocketClient;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Request;
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
        internal static TCPNetworkClient<ClientNetworkConnection, ClientOptions<ClientNetworkConnection>> client;
        internal static RequestProcessor requestProcessor;

        protected static ClientNetworkConnection GetNetworkClient()
            => client.Data;

        protected static RequestProcessor GetRequestProcessor()
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
        internal static TCPNetworkClient<ClientNetworkConnection, ClientOptions<ClientNetworkConnection>> client;
        internal static RequestProcessor requestProcessor;

        protected static ClientNetworkConnection GetNetworkClient()
            => client.Data;

        protected static RequestProcessor GetRequestProcessor()
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
        internal static TCPNetworkClient<ClientNetworkConnection, ClientOptions<ClientNetworkConnection>> client;
        internal static RequestProcessor requestProcessor;

        protected static ClientNetworkConnection GetNetworkClient()
            => client.Data;

        protected static RequestProcessor GetRequestProcessor()
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
        internal static TCPNetworkClient<ClientNetworkConnection, ClientOptions<ClientNetworkConnection>> client;
        internal static RequestProcessor requestProcessor;

        protected static ClientNetworkConnection GetNetworkClient()
            => client.Data;

        protected static RequestProcessor GetRequestProcessor()
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