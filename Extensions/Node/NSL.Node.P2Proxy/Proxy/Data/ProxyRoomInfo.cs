using NSL.Node.RoomServer.Shared.Client.Core.Enums;
using NSL.SocketCore.Utils.Buffer;
using NSL.UDP;
using System;
using System.Collections.Concurrent;

namespace NSL.Node.P2Proxy.Proxy.Data
{
    public class ProxyRoomInfo
    {
        public string ID { get; set; }

        public ProxyRoomInfo(string id)
        {
            ID = id;
        }

        event Action<DgramOutputPacketBuffer> BroadcastDelegate = data => { };

        private ConcurrentDictionary<Guid, P2PNetworkClient> clients = new ConcurrentDictionary<Guid, P2PNetworkClient>();

        public bool ExistsClient(Guid nodeId)
            => clients.ContainsKey(nodeId);

        internal void SignIn(P2PNetworkClient client)
        {
            if (clients.TryGetValue(client.Id, out var _c))
            {
                if (client == _c)
                    return;

                clients.TryRemove(client.Id, out _);
            }

            if (clients.TryAdd(client.Id, client))
            {
                client.Room = this;

                BroadcastDelegate += (data) =>
                {
                    data.Send(client.Network, false);
                };
            }
        }

        internal void Broadcast(P2PNetworkClient client, InputPacketBuffer buffer)
        {
            if (buffer is DgramInputPacketBuffer dgram)
            {
                var output = new DgramOutputPacketBuffer();

                output.PacketId = (ushort)RoomPacketEnum.BroadcastMessage;

                output.Channel = dgram.SourceChannel;

                output.Write(buffer.Data);

                BroadcastDelegate(output);
            }
        }

        internal void Transport(P2PNetworkClient client, InputPacketBuffer buffer)
        {
            if (buffer is DgramInputPacketBuffer dgram)
            {
                var output = new DgramOutputPacketBuffer();

                output.PacketId = (ushort)RoomPacketEnum.TransportMessage;

                output.Channel = dgram.SourceChannel;

                var body = buffer.Read(buffer.DataLength - 16);

                var to = buffer.ReadGuid();

                output.Write(body);

                if (clients.TryGetValue(to, out var _to))
                    output.Send(_to.Network, true);
            }
        }
    }
}
