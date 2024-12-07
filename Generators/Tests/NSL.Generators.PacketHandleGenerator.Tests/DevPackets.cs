using NSL.Generators.PacketHandleGenerator.Shared;

namespace NSL.Generators.PacketHandleGenerator.Tests
{
    public enum DevPackets
    {
        Response,

        [NSLPHGen]
        WithoutTypePacketRequest,

        [NSLPHGen]
        [NSLPHGenParam(typeof(Param1Struct)), NSLPHGenParam(typeof(Param2Struct))]
        WithoutTypePacketMessage,

        [NSLPHGen(PacketTypeEnum.Request)]
        [NSLPHGenParam(typeof(Param1Struct)), NSLPHGenParam(typeof(Param2Struct))]
        PTPacketRequest,

        [NSLPHGen(PacketTypeEnum.Message)]
        PTPacketMessage,
    }
}
