#if NSL_LIBRARY
using System;
using System.Threading.Tasks;

namespace NSL.SocketCore.Utils.Pipeline.Middleware
{
    /// <summary>
    /// Channel middleware that logs every received and sent packet.
    /// Reserves 0 bytes in the channel header region.
    /// <para>
    /// Note: NSL already exposes <c>CoreOptions.OnReceivePacket</c> / <c>OnSendPacket</c> events.
    /// This middleware is the pipeline-native alternative; both can coexist or one can be removed later.
    /// </para>
    /// </summary>
    public class LoggingChannelMiddleware : IChannelReceiveMiddleware, IChannelSendMiddleware
    {
        public int ReceiveHeaderSize => 0;
        public int SendHeaderSize    => 0;

        private readonly Action<string> _log;

        /// <param name="log">Log sink, e.g. <c>Console.WriteLine</c> or a logger wrapper.</param>
        public LoggingChannelMiddleware(Action<string> log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public ValueTask InvokeAsync(ReceiveChannelContext ctx, PacketReceiveDelegate next)
        {
            _log($"[RECV] pid={ctx.PacketId} bodyLen={ctx.Body.Length} from={ctx.Connection.Network?.GetRemotePoint()}");
            return next(ctx);
        }

        public ValueTask InvokeAsync(SendChannelContext ctx, PacketSendDelegate next)
        {
            _log($"[SEND] pid={ctx.PacketId} bodyLen={ctx.Body.Length} to={ctx.Connection.Network?.GetRemotePoint()}");
            return next(ctx);
        }
    }
}
#endif
