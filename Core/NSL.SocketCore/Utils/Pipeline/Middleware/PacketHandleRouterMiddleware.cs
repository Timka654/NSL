#if NSL_LIBRARY
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSL.SocketCore.Utils.Pipeline.Middleware
{
    /// <summary>
    /// Receive-only middleware acting as an in-pipeline packet dispatch table.
    /// Replaces or augments the <see cref="CoreOptions"/> PacketHandles store for pipelines.
    /// Routes by <see cref="ReceiveChannelContext.PacketId"/>; unmatched packets fall through to <c>next</c>.
    /// <para>
    /// Three route signatures are supported:
    /// <list type="bullet">
    ///   <item><description><c>PacketRouteDelegate</c> — full context, ValueTask return.</description></item>
    ///   <item><description><c>Action&lt;ReceiveChannelContext&gt;</c> — simple sync handler.</description></item>
    ///   <item><description><c>Action&lt;BaseNetworkConnection, InputPacketBuffer&gt;</c> — backward-compat with existing handles.</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public class PacketHandleRouterMiddleware : IChannelReceiveMiddleware, IPacketHandleRegistry
    {
        public int ReceiveHeaderSize => 0;

        private readonly Dictionary<ushort, PacketRouteDelegate> _routes
            = new Dictionary<ushort, PacketRouteDelegate>();

        // ── Route registration ───────────────────────────────────────────────

        public PacketHandleRouterMiddleware AddRoute(ushort pid, PacketRouteDelegate handler)
        {
            _routes[pid] = handler;
            return this;
        }

        public PacketHandleRouterMiddleware AddRoute(ushort pid, Action<ReceiveChannelContext> handler)
        {
            _routes[pid] = ctx => { handler(ctx); return default(ValueTask); };
            return this;
        }

        /// <summary>
        /// Backward-compatible overload: constructs an <see cref="InputPacketBuffer"/> from the context
        /// and invokes the classic handler. The buffer is disposed after the handler returns.
        /// </summary>
        public PacketHandleRouterMiddleware AddRoute(ushort pid, Action<BaseNetworkConnection, InputPacketBuffer> handler)
        {
            _routes[pid] = ctx =>
            {
                var buf = InputPacketBuffer.FromBody(ctx.Body, ctx.PacketId);
                try   { handler(ctx.Connection, buf); }
                finally { if (!buf.ManualDisposing) buf.Dispose(); }
                return default(ValueTask);
            };
            return this;
        }

        // ── IPacketHandleRegistry ────────────────────────────────────────────

        public bool AddHandle(ushort packetId, CoreOptions.PacketHandle handle)
        {
            AddRoute(packetId, (conn, buf) => handle(conn, buf));
            return true;
        }

        public bool AddPacket(ushort packetId, IPacket packet)
            => AddHandle(packetId, packet.Receive);

        public bool AddAsyncHandle(ushort packetId, Func<BaseNetworkConnection, InputPacketBuffer, Task> handle)
        {
            AddRoute(packetId, (conn, buf) =>{

                buf.ManualDisposing = true;
                Task.Run(async () =>
                {
                    try { await handle(conn, buf); }
                    catch (Exception ex) { conn.Options.CallExceptionEvent(ex, conn); }
                    if (buf.AsyncDisposing) buf.Dispose();
                });
            });
            return true;
        }

        // ── Dispatch ─────────────────────────────────────────────────────────

        public ValueTask InvokeAsync(ReceiveChannelContext ctx, PacketReceiveDelegate next)
        {
            if (_routes.TryGetValue(ctx.PacketId, out var handler))
                return handler(ctx);
            return next(ctx);
        }
    }

    /// <summary>Pipeline-native route handler: receives the full <see cref="ReceiveChannelContext"/>.</summary>
    public delegate ValueTask PacketRouteDelegate(ReceiveChannelContext ctx);
}
#endif
