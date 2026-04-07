using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.SocketCore.Utils.Request
{
    public class RequestProcessor : IResponsibleProcessor, IDisposable
    {
        public const string DefaultObjectBagKey          = NSLObjectBagKeys.RequestProcessor;
        public const string DefaultResponsePIDObjectBagKey = NSLObjectBagKeys.ResponsePID;

        public const ushort DefaultResponsePacketId = (ushort)NSLSystemPacketEnum.DefaultRequestResponse;

        /// <summary>
        /// Create a processor that uses a shared <paramref name="hub"/> for request routing.
        /// Multiple processors may share the same hub over one connection.
        /// </summary>
        public RequestProcessor(BaseNetworkConnection client, IRequestHub hub)
        {
            this.client = client;
            this.hub = hub;
        }

        /// <summary>
        /// Create a processor with its own private <see cref="RequestProcessorHub"/>.
        /// </summary>
        public RequestProcessor(BaseNetworkConnection client) : this(client, new RequestProcessorHub())
        {
            ownsHub = true;
        }

        private readonly BaseNetworkConnection client;
        private readonly IRequestHub hub;
        private readonly bool ownsHub;

        // Tracks request ids owned by this processor so they can be cancelled on disposal.
        private readonly HashSet<Guid> ownedRequests = new HashSet<Guid>();

        private Guid RegisterRequest(Action<InputPacketBuffer> handler)
        {
            var id = hub.CreateRequest(handler);
            lock (ownedRequests)
                ownedRequests.Add(id);
            return id;
        }

        private void UntrackRequest(Guid id)
        {
            lock (ownedRequests)
                ownedRequests.Remove(id);
        }

        /// <summary>
        /// Send request and handle response on receive.
        /// </summary>
        /// <param name="onResponse">Return <see langword="true"/> to auto-dispose the input buffer, or dispose it manually.</param>
        /// <returns>request id</returns>
        public Guid SendRequest(RequestPacketBuffer buffer, Func<InputPacketBuffer, bool> onResponse, CancellationToken cancellationToken, bool disposeOnSend = true)
        {
            Guid rid = default;

            cancellationToken.Register(() =>
            {
                UntrackRequest(rid);
                hub.TryRemoveRequest(rid);
            });

            Action<InputPacketBuffer> action = input =>
            {
                UntrackRequest(rid);
                if (onResponse(input))
                    input?.Dispose();
            };

            rid = RegisterRequest(action);

            buffer.WithRecvIdentity(rid);
            client.Send(buffer, disposeOnSend);

            return rid;
        }

        public Task SendRequestAsync(RequestPacketBuffer buffer, Func<InputPacketBuffer, Task<bool>> onResponse, bool disposeOnSend = true)
            => SendRequestAsync(buffer, onResponse, CancellationToken.None, disposeOnSend);

        public async Task SendRequestAsync(RequestPacketBuffer buffer, Func<InputPacketBuffer, Task<bool>> onResult, CancellationToken cancellationToken, bool disposeOnSend = true)
        {
            InputPacketBuffer data = default;
            Guid rid = default;
            try
            {
                using (CancellationTokenSource cts = new CancellationTokenSource())
                {
                    rid = SendRequest(buffer, input =>
                    {
                        try
                        {
                            data = input;
                            cts.Cancel();
                        }
                        catch (Exception)
                        {
                            throw;
                        }

                        return false;
                    }, cancellationToken, disposeOnSend);

                    using (var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken))
                        await Task.Delay(-1, linkedTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                if (data != null)
                    if (await onResult(data))
                    {
                        data?.Dispose();
                    }

                data = null;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                UntrackRequest(rid);
                hub.TryRemoveRequest(rid);

                data?.Dispose();
            }
        }

        /// <summary>
        /// Delegates to the hub's <see cref="IRequestHub.ProcessResponse"/>.
        /// Register the hub directly on the connection when multiple processors share it.
        /// </summary>
        public void ProcessResponse(InputPacketBuffer data)
            => hub.ProcessResponse(data);

        public void Dispose()
        {
            Guid[] keys;
            lock (ownedRequests)
            {
                keys = ownedRequests.ToArray();
                ownedRequests.Clear();
            }

            foreach (var id in keys)
                hub.CancelRequest(id);

            if (ownsHub && hub is IDisposable disposable)
                disposable.Dispose();
        }

        [Obsolete("Replace to RequestProcessor", true)]
        public class PacketWaitBuffer : RequestProcessor
        {
            public PacketWaitBuffer(BaseNetworkConnection client) : base(client)
            {
            }
        }
    }
}
