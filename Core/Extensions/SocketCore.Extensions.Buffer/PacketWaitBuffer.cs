using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.SocketCore.Extensions.Buffer
{
    public class PacketWaitBuffer : IDisposable
    {
        public PacketWaitBuffer(INetworkClient client)
        {
            this.client = client;
        }

        private ConcurrentDictionary<Guid, Action<InputPacketBuffer>> requests = new ConcurrentDictionary<Guid, Action<InputPacketBuffer>>();

        private readonly INetworkClient client;

        public async Task SendWaitRequest(WaitablePacketBuffer buffer, Func<InputPacketBuffer, Task> onResult, bool disposeOnSend = true)
        {
            InputPacketBuffer data = default;

            using (ManualResetEvent locker = new ManualResetEvent(false))
            {
                Guid rid;

                do
                {
                    rid = Guid.NewGuid();
                } while (!requests.TryAdd(rid, (input) => { data = input; locker.Set(); }));

                buffer.WithRecvIdentity(rid);

                client.Network.Send(buffer, disposeOnSend);

                while (!await Task.Run(() => locker.WaitOne(client.AliveState ? client.AliveCheckTimeOut / 2 : 1000)))
                {
                    if (client?.GetState() == false)
                    {
                        requests.TryRemove(rid, out _);
                        break;
                    }
                }

            }

            await onResult(data);

            data?.Dispose();
        }

        public void ProcessWaitResponse(InputPacketBuffer data)
        {
            if (requests.TryRemove(data.ReadGuid(), out var waitAction))
            {
                data.ManualDisposing = true;
                waitAction(data);
            }
        }

        public void Dispose()
        {
            foreach (var item in requests.Keys.ToArray())
            {
                if (requests.TryRemove(item, out var value))
                    value.Invoke(null);
            }
        }
    }

    public static class __Extensions
    {
        /// <summary>
        /// Register handle for execute wait receive packet in buffer
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="packetId"></param>
        /// <param name="handler"></param>
        public static void AddReceivePacketHandle<TClient>(this CoreOptions<TClient> options, ushort packetId, Func<TClient, PacketWaitBuffer> handler)
            where TClient : INetworkClient, new()
        {
            options.AddHandle(packetId, (client, packet) => handler(client).ProcessWaitResponse(packet));
        }

        /// <summary>
        /// Register handle for execute wait receive packet in buffer
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="client"></param>
        /// <param name="packetId"></param>
        /// <param name="handler"></param>
        public static void AddReceivePacketHandle<TClient, TEnum>(this CoreOptions<TClient> options, TEnum packetId, Func<TClient, PacketWaitBuffer> handler)
            where TClient : INetworkClient, new()
            where TEnum : struct, IConvertible
        {
            options.AddReceivePacketHandle(packetId.ToUInt16(null), handler);
        }
    }
}
