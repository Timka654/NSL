using System;

namespace NSL.Generators.PacketHandleGenerator.Shared
{
    [Flags]
    public enum AccessModifierEnum
    {
        Private = 1,
        Internal = 2,
        Public = 4, 
        Static = 8
    }
}
