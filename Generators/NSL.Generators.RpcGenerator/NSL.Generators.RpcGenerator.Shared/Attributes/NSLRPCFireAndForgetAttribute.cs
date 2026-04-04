using System;

namespace NSL.Generators.RpcGenerator.Shared.Attributes
{
    /// <summary>
    /// Marks an RPC method as fire-and-forget: client sends the packet and returns immediately without
    /// waiting for any acknowledgement from the server. The generated client method returns <c>void</c>.
    /// The server handler receives and processes the call but never sends a response.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NSLRPCFireAndForgetAttribute : Attribute
    {
    }
}
