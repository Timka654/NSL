using System;

namespace NSL.SocketCore.Utils.Buffer
{
    /// <summary>
    /// Centralized hub that owns the request registry and routes incoming responses
    /// to the appropriate handler by request id.
    /// Multiple <see cref="RequestProcessor"/> instances can share a single hub.
    /// </summary>
    public interface IRequestHub : IResponsibleProcessor
    {
        /// <summary>
        /// Register a pending request handler and return a unique request id.
        /// </summary>
        Guid CreateRequest(Action<InputPacketBuffer> handler);

        /// <summary>
        /// Remove a pending request from the registry without invoking its handler.
        /// Used for cancellation-token-driven cleanup where the awaiting task already
        /// unblocks due to the token itself.
        /// </summary>
        bool TryRemoveRequest(Guid id);

        /// <summary>
        /// Remove a pending request from the registry and invoke its handler with
        /// <see langword="null"/> to unblock any awaiter (e.g. on processor disposal).
        /// </summary>
        bool CancelRequest(Guid id);
    }
}
