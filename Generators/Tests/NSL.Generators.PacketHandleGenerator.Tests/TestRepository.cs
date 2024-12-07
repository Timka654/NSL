using NSL.Generators.PacketHandleGenerator.Shared;

namespace NSL.Generators.PacketHandleGenerator.Tests
{
    [NSLPHGenImplement(typeof(DevPackets), typeof(DevNetworkClient), HPDirTypeEnum.Input, AccessModifierEnum.Internal | AccessModifierEnum.Static)]
    internal partial class TestRepository
    {
        internal static partial void ReceiveWithoutTypePacketMessageHandle(DevNetworkClient client, Param1Struct item0, Param2Struct item1)
        {
            throw new NotImplementedException();
        }

        internal static partial void ReceivePTPacketMessageHandle(DevNetworkClient client)
        {
            throw new NotImplementedException();
        }
    }
}
