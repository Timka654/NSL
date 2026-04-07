#if NSL_LIBRARY
using System.Threading.Tasks;

namespace NSL.SocketCore.Utils.Pipeline
{
    public delegate ValueTask PacketReceiveDelegate(ReceiveChannelContext ctx);

    public interface IChannelReceiveMiddleware
    {
        /// <summary>
        /// Number of bytes this middleware reserves in the channel header region (after the 7-byte base header),
        /// immediately before the packet body. The pipeline engine slices this many bytes and exposes them
        /// as <see cref="ReceiveChannelContext.MiddlewareHeader"/> before invoking this middleware.
        /// </summary>
        int ReceiveHeaderSize { get; }

        ValueTask InvokeAsync(ReceiveChannelContext ctx, PacketReceiveDelegate next);
    }
}
#endif
