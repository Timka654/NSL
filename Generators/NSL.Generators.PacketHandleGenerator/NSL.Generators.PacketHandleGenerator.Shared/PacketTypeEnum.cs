using System;

namespace NSL.Generators.PacketHandleGenerator.Shared
{
    [Flags]
    public enum PacketTypeEnum
    {
        Request = 1,
        Message = 2,
        Async = 4
    }

    public enum HPDirTypeEnum
    {
        Output,
        Input
    }
}
