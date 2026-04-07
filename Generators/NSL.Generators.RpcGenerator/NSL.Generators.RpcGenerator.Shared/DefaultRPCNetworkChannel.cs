using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils;

using System.Threading;
using System.Threading.Tasks;
using NSL.SocketCore.Utils.Request;

namespace NSL.Generators.RpcGenerator.Shared
{
    /// <summary>
    /// Default <see cref="IRPCNetworkChannel"/> implementation for TCP/WebSocket connections
    /// that use <see cref="RequestProcessor"/> for request tracking.
    /// </summary>
    public class DefaultRPCNetworkChannel : IRPCNetworkChannel
    {
        private readonly BaseNetworkConnection _client;
        private readonly RequestProcessor _requestProcessor;

        public DefaultRPCNetworkChannel(BaseNetworkConnection client, RequestProcessor requestProcessor)
        {
            _client = client;
            _requestProcessor = requestProcessor;
        }

        /// <inheritdoc/>
        public void Send(OutputPacketBuffer buffer) => _client.Send(buffer);

        /// <inheritdoc/>
        public async Task<InputPacketBuffer> RequestAsync(OutputPacketBuffer buffer, CancellationToken cancellationToken = default)
        {
            InputPacketBuffer result = default;

            await _requestProcessor.SendRequestAsync((RequestPacketBuffer)buffer, async response =>
            {
                result = response;
                return false; // caller disposes via 'using var'
            }, cancellationToken);

            return result;
        }
    }
}
