﻿namespace NSL.UDP.Enums
{
    public enum UDPChannelEnum : byte
    {
        Reliable = 1,
        Unreliable = 2,
        Ordered = 4,
        Unordered = 8,
        ReliableOrdered = Reliable | Ordered,
        ReliableUnordered = Reliable | Unordered,
        UnreliableOrdered = Unreliable | Ordered,
        UnreliableUnordered = Unreliable | Unordered,
    }
}
