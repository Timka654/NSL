using System;

namespace NSL.Generators.RpcGenerator.Shared
{
    [Flags]
    public enum NSLRPCDirection
    {
        Client = 1,
        Server = 2,
        Both = Client | Server
    }
}
