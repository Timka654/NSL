using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketServer.Utils
{
    public abstract class IDataReceivePacket<T, TData> : IPacket<T>
           where T : IServerNetworkClient
    {
        protected ConcurrentDictionary<IServerNetworkClient, (AutoResetEvent Locker, TData Data)> Lockers = new ConcurrentDictionary<IServerNetworkClient, (AutoResetEvent, TData)>();

        protected void SetReceiveData(T client, TData data)
        {
            if (Lockers.TryGetValue(client, out var entry))
            {
                entry.Data = data;
                entry.Locker.Set();
            }
        }

        public override abstract void Receive(T client, InputPacketBuffer data);

        protected TData Send(T client, OutputPacketBuffer packet, int timeout = 2000)
        {
            if (!Lockers.TryGetValue(client, out var entry))
            {
                entry = (new AutoResetEvent(true), default);
                Lockers.TryAdd(client, entry);
            }

            entry.Locker.WaitOne(timeout);
            client.Send(packet);

            if (!entry.Locker.WaitOne(timeout))
            {
                return default;
            }

            return entry.Data;
        }
    }
}
