using NSL.SocketCore.Utils.Buffer;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Generators.RpcGenerator.Shared
{
    /// <summary>
    /// Abstracts the send/request transport for RPC calls.
    /// Implement this interface to plug in any network backend
    /// (e.g. plain TCP, UDP channels, local bridge, etc.).
    /// </summary>
    public interface IRPCNetworkChannel
    {
        /// <summary>Send a fire-and-forget packet. No response is expected.</summary>
        void Send(OutputPacketBuffer buffer);

        /// <summary>
        /// Send a request packet and asynchronously receive the response buffer.
        /// The generator always passes a <see cref="RequestPacketBuffer"/> created via
        /// <c>RequestPacketBuffer.Create(pid)</c>; the channel implementation is free to
        /// cast or handle it as needed (e.g. for UDP channel tracking).
        /// The caller is responsible for disposing the returned <see cref="InputPacketBuffer"/>.
        /// </summary>
        Task<InputPacketBuffer> RequestAsync(OutputPacketBuffer buffer, CancellationToken cancellationToken = default);
    }
}
