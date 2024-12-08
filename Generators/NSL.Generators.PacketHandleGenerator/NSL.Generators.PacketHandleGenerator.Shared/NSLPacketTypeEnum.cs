using System;

namespace NSL.Generators.PacketHandleGenerator.Shared
{
    [Flags]
    public enum NSLPacketTypeEnum
    {
        Request = 1,
        Message = 2,
        Async = 4,
        Sync = 8
    }
}
