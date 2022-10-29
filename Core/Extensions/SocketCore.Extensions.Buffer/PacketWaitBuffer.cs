using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
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
            buffer.Position = OutputPacketBuffer.headerLenght;

            ManualResetEvent locker = new ManualResetEvent(false);

            Guid rid;

            InputPacketBuffer data = default;

            do
            {
                rid = Guid.NewGuid();
            } while (!requests.TryAdd(rid, (input) => { data = input; locker.Set(); }));

            buffer.WithRecvIdentity(rid);

            client.Network.Send(buffer, disposeOnSend);

            await Task.Run(() => locker.WaitOne());

            locker.Dispose();

            await onResult(data);

            data.Dispose();

        }

        public void ProcessWaitResponse(InputPacketBuffer data)
        {
            data.ManualDisposing = true;

            if (requests.TryRemove(data.ReadGuid(), out var waitAction))
                waitAction(data);
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
