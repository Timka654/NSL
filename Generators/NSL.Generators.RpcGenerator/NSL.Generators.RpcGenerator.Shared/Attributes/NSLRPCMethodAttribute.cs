using System;

namespace NSL.Generators.RpcGenerator.Shared.Attributes
{
    /// <summary>
    /// Marks an interface method as an RPC endpoint with the specified packet identifier.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NSLRPCMethodAttribute : Attribute
    {
        /// <summary>
        /// Unique packet identifier for this RPC method.
        /// </summary>
        public ushort Pid { get; }

        /// <summary>
        /// Optional binary model name for custom serialization (matches NSL BinaryGenerator model).
        /// When null, full serialization is used.
        /// </summary>
        public string BinaryModel { get; set; }

        public NSLRPCMethodAttribute(ushort pid)
        {
            Pid = pid;
        }
    }
}
