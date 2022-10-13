using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.SocketCore.Extensions.Buffer
{
    public class PacketWaitBuffer : IDisposable
    {
        public PacketWaitBuffer(INetworkClient client, ushort defaultPid = default)
        {
            this.client = client;
            this.defaultPid = defaultPid;
        }

        private ConcurrentDictionary<Guid, Action<InputPacketBuffer>> requests = new ConcurrentDictionary<Guid, Action<InputPacketBuffer>>();

        private readonly INetworkClient client;
        private readonly ushort defaultPid;

        public async Task CreateWaitRequest(Action<OutputPacketBuffer> packetBuildAction, Func<InputPacketBuffer, Task> onResult)
        {
            ManualResetEvent locker = new ManualResetEvent(false);

            Guid rid;

            InputPacketBuffer data = default;

            do
            {
                rid = Guid.NewGuid();
            } while (requests.TryAdd(rid, (input) => { data = input; locker.Set(); }));


            var packet = new OutputPacketBuffer();

            packet.PacketId = defaultPid;

            packet.WriteGuid(rid);

            packetBuildAction(packet);

            client.Network.Send(packet);

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
            foreach (var item in requests.Keys)
            {
                if (requests.TryRemove(item, out var value))
                    value.Invoke(null);
            }
        }
    }
}
