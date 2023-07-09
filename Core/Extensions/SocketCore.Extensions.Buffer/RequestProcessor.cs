using NSL.SocketCore.Extensions.Buffer.Interface;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.SocketCore.Extensions.Buffer
{
    public class RequestProcessor : IResponsibleProcessor, IDisposable
    {
        public const string DefaultObjectBagKey = "NSL__DEFAULT__REQUEST__PROCESSOR";

        public const ushort DefaultResponsePacketId = 1;

        public RequestProcessor(INetworkClient client)
        {
            this.client = client;
        }

        private Dictionary<Guid, Action<InputPacketBuffer>> requests = new Dictionary<Guid, Action<InputPacketBuffer>>();

        private readonly INetworkClient client;

        protected bool TryAdd(Guid id, Action<InputPacketBuffer> action)
        {
            lock (this)
            {
                if (requests.ContainsKey(id))
                    return false;

                requests.Add(id, action);

                return true;
            }
        }

        /// <summary>
        /// Send request and handle response on receive
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="onResponse">Delegate must return <see langword="true"/> for dispose or input buffer must manual disposing after processing</param>
        /// <returns>request id</returns>
        public Guid SendRequest(RequestPacketBuffer buffer, Func<InputPacketBuffer, bool> onResponse, bool disposeOnSend = true)
        {
            Guid rid = default;

            do
            {
                rid = Guid.NewGuid();
            } while (!TryAdd(rid, (input) =>
            {
                if (onResponse(input))
                    input?.Dispose();
            }));

            buffer.WithRecvIdentity(rid);

            client.Network.Send(buffer, disposeOnSend);

            return rid;
        }


        public Task SendRequestAsync(RequestPacketBuffer buffer, Func<InputPacketBuffer, Task> onResponse, bool disposeOnSend = true)
            => SendRequestAsync(buffer, onResponse, CancellationToken.None, disposeOnSend);

        public async Task SendRequestAsync(RequestPacketBuffer buffer, Func<InputPacketBuffer, Task> onResult, CancellationToken cancellationToken, bool disposeOnSend = true)
        {
            InputPacketBuffer data = default;
            Guid rid = default;
            try
            {
                using (ManualResetEvent locker = new ManualResetEvent(false))
                {
                    rid = SendRequest(buffer, input =>
                    {
                        data = input;
                        return false;
                    }, disposeOnSend);

                    while (requests.ContainsKey(rid) && !await Task.Run(() => locker.WaitOne(100), cancellationToken)) { cancellationToken.ThrowIfCancellationRequested(); }
                }

                await onResult(data);
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                lock (this)
                {
                    if (rid != default)
                        requests.Remove(rid);
                }

                data?.Dispose();
            }
        }

        public void ProcessResponse(InputPacketBuffer data)
        {
            var id = data.ReadGuid();

            processResponse(id, data);
        }

        private void processResponse(Guid id, InputPacketBuffer data)
        {
            if (requests.TryGetValue(id, out var waitAction))
            {
                lock (this)
                {
                    requests.Remove(id);
                }

                if (data != null)
                    data.ManualDisposing = true;

                waitAction(data);
            }
        }

        public void Dispose()
        {
            Guid[] keys = default;

            lock (this)
            {
                keys = requests.Keys.ToArray();
            }

            foreach (var item in keys)
            {
                processResponse(item, null);
            }
        }

        [Obsolete("Replace to RequestProcessor", true)]
        public class PacketWaitBuffer : RequestProcessor
        {
            public PacketWaitBuffer(INetworkClient client) : base(client)
            {
            }
        }

    }

}
