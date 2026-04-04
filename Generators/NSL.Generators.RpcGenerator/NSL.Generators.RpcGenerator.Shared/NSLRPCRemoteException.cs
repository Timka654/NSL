using System;

namespace NSL.Generators.RpcGenerator.Shared
{
    /// <summary>
    /// Thrown on the client side when the server catches and forwards an exception via
    /// <see cref="Attributes.NSLRPCExceptionHandlerAttribute"/>.
    /// </summary>
    public class NSLRPCRemoteException : Exception
    {
        /// <summary>
        /// The <see cref="Type.FullName"/> of the exception that was thrown on the server.
        /// </summary>
        public string RemoteTypeName { get; }

        public NSLRPCRemoteException(string remoteTypeName, string remoteMessage)
            : base($"[Remote:{remoteTypeName}] {remoteMessage}")
        {
            RemoteTypeName = remoteTypeName;
        }
    }
}
