#if NSL_LIBRARY
using System.Threading.Tasks;

namespace NSL.SocketCore.Utils.Pipeline
{
    public delegate ValueTask PacketSendDelegate(SendChannelContext ctx);

    public interface IChannelSendMiddleware
    {
        /// <summary>
        /// Number of bytes this middleware reserves in the channel header region of an outgoing packet
        /// (placed between the 7-byte base header and the body). The pipeline engine slices this many
        /// bytes and exposes them as <see cref="SendChannelContext.MiddlewareHeader"/> before invoking
        /// this middleware.
        /// </summary>
        int SendHeaderSize { get; }

        ValueTask InvokeAsync(SendChannelContext ctx, PacketSendDelegate next);
    }
}
#endif
