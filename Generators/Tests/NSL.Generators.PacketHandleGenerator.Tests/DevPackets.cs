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

        [NSLPHGen(NSLPacketTypeEnum.Message | NSLPacketTypeEnum.Async)]
        [NSLPHGenParam(typeof(Param1Struct), Name = "p1s"), NSLPHGenParam(typeof(Param2Struct))]
        WithoutType2PacketMessage,

        [NSLPHGen(NSLPacketTypeEnum.Request | NSLPacketTypeEnum.Async)]
        [NSLPHGenParam(typeof(Param1Struct), Name = "p1s"), NSLPHGenParam(typeof(Param2Struct)), NSLPHGenResult(typeof(Param2Struct))]
        WithoutType2PacketRequest,

        [NSLPHGen(NSLPacketTypeEnum.Request)]
        [NSLPHGenParam(typeof(Param3Struct)), NSLPHGenParam(typeof(Param2Struct)), NSLPHGenParam(typeof(int)), NSLPHGenResult(typeof(int))]
        PTPacketRequest,

        [NSLPHGen(NSLPacketTypeEnum.Request)]
        [NSLPHGenParam(typeof(Param3Struct)), NSLPHGenParam(typeof(Param2Struct)), NSLPHGenParam(typeof(int)), NSLPHGenResult(typeof(Param3Struct))]
        PT2PacketRequest,

        [NSLPHGen(NSLPacketTypeEnum.Message)]
        PTPacketMessage,
    }
}
