using NSL.Generators.PacketHandleGenerator.Shared;
using NSL.SocketCore;
using NSL.SocketServer.Utils;

namespace NSL.Generators.PacketHandleGenerator.Tests
{
    [NSLPHGenImplement(PacketsEnum = typeof(DevPackets), NetworkDataType = typeof(BaseServerNetworkClient), Direction = NSLHPDirTypeEnum.Input, Modifier = NSLAccessModifierEnum.Internal | NSLAccessModifierEnum.Static, IsStaticNetwork = true)]
    internal partial class TestRepository
    {
        internal static void ReceiveWithoutTypePacketRequestHandle(BaseServerNetworkClient client)
        {
            throw new NotImplementedException();
        }

        internal static void ReceiveWithoutTypePacketMessageHandle(BaseServerNetworkClient client, Param1Struct p1s, Param2Struct item1)
        {
            throw new NotImplementedException();
        }

        internal static Task ReceiveWithoutType2PacketMessageHandle(BaseServerNetworkClient client, Param1Struct p1s, Param2Struct item1)
        {
            throw new NotImplementedException();
        }

        internal static Task<Param2Struct> ReceiveWithoutType2PacketRequestHandle(BaseServerNetworkClient client, Param1Struct p1s, Param2Struct item1)
        {
            throw new NotImplementedException();
        }

        internal static Int32 ReceivePTPacketRequestHandle(BaseServerNetworkClient client, Param3Struct item0, Param2Struct item1, Int32 item2)
        {
            throw new NotImplementedException();
        }

        internal static Param3Struct ReceivePT2PacketRequestHandle(BaseServerNetworkClient client, Param3Struct item0, Param2Struct item1, Int32 item2)
        {
            throw new NotImplementedException();
        }

        internal static void ReceivePTPacketMessageHandle(BaseServerNetworkClient client)
        {
            throw new NotImplementedException();
        }


        public static void _ConfigurePacketHandles(CoreOptions<BaseServerNetworkClient> options)
        {
            ConfigurePacketHandles(options);
        }
    }

    [NSLPHGenImplement(PacketsEnum = typeof(DevPackets), NetworkDataType = typeof(BaseServerNetworkClient), Direction = NSLHPDirTypeEnum.Output, Modifier = NSLAccessModifierEnum.Internal | NSLAccessModifierEnum.Static)]
    internal partial class TestRepository2
    { 
    
    }
}
