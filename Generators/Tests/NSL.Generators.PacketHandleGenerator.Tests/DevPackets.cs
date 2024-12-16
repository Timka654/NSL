using NSL.Generators.PacketHandleGenerator.Shared;

namespace NSL.Generators.PacketHandleGenerator.Tests
{
    public enum DevPackets
    {
        Response,

        [NSLPHGen]
        WithoutTypePacketRequest,

        [NSLPHGen]
        [NSLPHGenArg(typeof(Param1Struct), Name = "p1s"), NSLPHGenArg(typeof(Param2Struct))]
        WithoutTypePacketMessage,

        [NSLPHGen(NSLPacketTypeEnum.Message)]
        [NSLPHGenArg(typeof(Param1Struct), Name = "p1s"), NSLPHGenArg(typeof(Param2Struct))]
        WithoutType2PacketMessage,

        [NSLPHGen(NSLPacketTypeEnum.Request)]
        [NSLPHGenArg(typeof(Param1Struct), Name = "p1s"), NSLPHGenArg(typeof(Param2Struct)), NSLPHGenResult(typeof(Param2Struct))]
        WithoutType2PacketRequest,

        [NSLPHGen(NSLPacketTypeEnum.Request)]
        [NSLPHGenArg(typeof(Param3Struct)), NSLPHGenArg(typeof(Param2Struct)), NSLPHGenArg(typeof(int)), NSLPHGenResult(typeof(int))]
        PTPacketRequest,

        [NSLPHGen(NSLPacketTypeEnum.Request)]
        [NSLPHGenArg(typeof(Param3Struct)), NSLPHGenArg(typeof(Param2Struct)), NSLPHGenArg(typeof(int)), NSLPHGenResult(typeof(Param3Struct))]
        PT2PacketRequest,

        [NSLPHGen(NSLPacketTypeEnum.Message)]
        PTPacketMessage,
    }
}
