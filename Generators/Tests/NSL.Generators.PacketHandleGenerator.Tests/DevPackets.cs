using NSL.Generators.PacketHandleGenerator.Shared;

namespace NSL.Generators.PacketHandleGenerator.Tests
{
    public enum DevPackets
    {
        Response,

        [NSLPHGen]
        WithoutTypePacketRequest,

        [NSLPHGen]
        [NSLPHGenParam(typeof(Param1Struct), Name = "p1s"), NSLPHGenParam(typeof(Param2Struct))]
        WithoutTypePacketMessage,

        [NSLPHGen(PacketTypeEnum.Message | PacketTypeEnum.Async)]
        [NSLPHGenParam(typeof(Param1Struct), Name = "p1s"), NSLPHGenParam(typeof(Param2Struct))]
        WithoutType2PacketMessage,

        [NSLPHGen(PacketTypeEnum.Request | PacketTypeEnum.Async)]
        [NSLPHGenParam(typeof(Param1Struct), Name = "p1s"), NSLPHGenParam(typeof(Param2Struct)), NSLPHGenResult(typeof(Param2Struct))]
        WithoutType2PacketRequest,

        [NSLPHGen(PacketTypeEnum.Request)]
        [NSLPHGenParam(typeof(Param3Struct)), NSLPHGenParam(typeof(Param2Struct)), NSLPHGenParam(typeof(int)), NSLPHGenResult(typeof(int))]
        PTPacketRequest,

        [NSLPHGen(PacketTypeEnum.Request)]
        [NSLPHGenParam(typeof(Param3Struct)), NSLPHGenParam(typeof(Param2Struct)), NSLPHGenParam(typeof(int)), NSLPHGenResult(typeof(Param3Struct))]
        PT2PacketRequest,

        [NSLPHGen(PacketTypeEnum.Message)]
        PTPacketMessage,
    }
}
