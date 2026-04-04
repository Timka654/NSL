using System;

namespace NSL.Generators.RpcGenerator.Shared.Attributes
{
    /// <summary>
    /// Enables remote exception forwarding for the RPC method or all methods in the interface.
    /// When present, the server wraps the handler in try/catch and serializes the exception
    /// (Type.FullName + Message) into the response; the client reads those values and throws
    /// <see cref="NSLRPCRemoteException"/> on failure.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = false)]
    public class NSLRPCExceptionHandlerAttribute : Attribute
    {
    }
}
