﻿using NSL.SocketCore.Extensions.Buffer.Interface;
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
        public const string DefaultResponsePIDObjectBagKey = "NSL__DEFAULT__RESPONSE_PID";

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
        public Guid SendRequest(RequestPacketBuffer buffer, Func<InputPacketBuffer, bool> onResponse, CancellationToken cancellationToken, bool disposeOnSend = true)
        {
            Guid rid = default;
            
            cancellationToken.Register(() =>
            {
                lock (this)
                {
                    requests.Remove(rid);
                }
            });

            Action<InputPacketBuffer> action = (input) =>
            {
                if (onResponse(input))
                    input?.Dispose();

                lock (this)
                {
                    requests.Remove(rid);
                }
            };

            do
            {
                rid = Guid.NewGuid();
            } while (!TryAdd(rid, action));

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
                        catch (Exception ex)
                        {

                            throw;
                        }

                        return false;
                    }, cancellationToken, disposeOnSend);

                    using (var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken))
                        await Task.Delay(-1, linkedTokenSource.Token);
                }
                //data = await result.Task;
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
