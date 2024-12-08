using NSL.Generators.PacketHandleGenerator.Shared;
using NSL.SocketCore;
using NSL.SocketServer.Utils;

namespace NSL.Generators.PacketHandleGenerator.Tests
{
    [NSLPHGenImplement(typeof(DevPackets), typeof(BaseServerNetworkClient), HPDirTypeEnum.Input, AccessModifierEnum.Internal | AccessModifierEnum.Static)]
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
}
