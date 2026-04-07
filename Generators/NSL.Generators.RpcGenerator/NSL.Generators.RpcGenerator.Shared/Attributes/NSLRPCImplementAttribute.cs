using System;

namespace NSL.Generators.RpcGenerator.Shared.Attributes
{
    /// <summary>
    /// Instructs the RPC generator to generate client/server RPC code for the specified interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class NSLRPCImplementAttribute : Attribute
    {
        /// <summary>
        /// The RPC service interface to implement.
        /// </summary>
        public Type Interface { get; }

        /// <summary>
        /// The <see cref="NSL.SocketCore.BaseNetworkConnection"/> implementation type used as the client data handle.
        /// Required for server-side handler signatures and client-side partial stubs.
        /// </summary>
        public Type NetworkDataType { get; }

        /// <summary>
        /// Controls which side(s) of the RPC contract to generate.
        /// Defaults to <see cref="NSLRPCDirection.Both"/>.
        /// </summary>
        public NSLRPCDirection Direction { get; set; } = NSLRPCDirection.Both;

        public NSLRPCImplementAttribute(Type @interface, Type networkDataType)
        {
            Interface = @interface;
            NetworkDataType = networkDataType;
        }
    }
}
