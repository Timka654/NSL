using Microsoft.CodeAnalysis;

namespace NSL.Generators.RpcGenerator.Core
{
    internal class RpcMethodData
    {
        /// <summary>
        /// Original method name (used for generated method/handle names).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Packet identifier assigned via <c>[NSLRPCMethod(pid)]</c>.
        /// </summary>
        public ushort Pid { get; set; }

        /// <summary>
        /// Parameters extracted from the interface method signature.
        /// </summary>
        public RpcParamData[] Parameters { get; set; }

        /// <summary>
        /// Non-null when the method returns Task&lt;T&gt;.
        /// Null when the method returns plain Task.
        /// </summary>
        public RpcResultData Result { get; set; }

        /// <summary>
        /// True when <c>[NSLRPCFireAndForget]</c> is present.
        /// Client generates void send; server handles without sending a response.
        /// </summary>
        public bool IsFireAndForget { get; set; }

        /// <summary>
        /// True when <c>[NSLRPCExceptionHandler]</c> is present on the method or the containing interface.
        /// Server wraps the call in try/catch and writes success/error flag + exception info.
        /// Client reads the flag and throws <see cref="NSL.Generators.RpcGenerator.Shared.NSLRPCRemoteException"/> on failure.
        /// </summary>
        public bool HasExceptionHandler { get; set; }

        public IMethodSymbol MethodSymbol { get; set; }
    }
}
