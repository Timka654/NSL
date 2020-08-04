using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketCore.Utils.SystemPackets.Enums;

namespace SocketServer.Utils.SystemPackets
{
    public class AliveConnection<T> : IPacket<T> where T : IServerNetworkClient
    {
        public override void Receive(T client, InputPacketBuffer data)
        {
            client.Alive_locker.Set();
            if (client.PingCount == ulong.MaxValue)
                client.PingCount = 0;

            if (client.PingCount++ % 60 == 0)
                SocketServer.Utils.SystemPackets.SystemTime.Send(client);
        }

        public static void Send(INetworkClient client)
        {
            client.Alive_locker.Reset();

            var packet = new OutputPacketBuffer()
            {
                PacketId = (ushort)ClientPacketEnum.AliveConnection
            };
            client.Network.Send(packet);

            client.AliveState = client.Alive_locker.WaitOne(client.AliveWaitTime);

            //client.Alive_locker.WaitOne(client.AliveWaitTime);
            //if (!client.AliveState)
            //    client.Network.Disconnect();
        }
    }
}
