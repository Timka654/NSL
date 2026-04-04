using System;

namespace NSL.Generators.RpcGenerator.Shared.Attributes
{
    /// <summary>
    /// Marks an interface as an RPC service contract.
    /// All methods intended for RPC must be annotated with <see cref="NSLRPCMethodAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class NSLRPCContainerAttribute : Attribute
    {
    }
}
