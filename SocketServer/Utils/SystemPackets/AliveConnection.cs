using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketCore.Utils.SystemPackets.Enums;

namespace SocketServer.Utils.SystemPackets
{
    public class AliveConnection<T> :IPacket<T> where T: IServerNetworkClient
    {
        public void Receive(T client, InputPacketBuffer data)
        {
            client.AliveState = true;
            //client.Alive_locker.Set();
            if (client.PingCount == ulong.MaxValue)
                client.PingCount = 0;
            
            if (client.PingCount++ % 60 == 0)
                SocketServer.Utils.SystemPackets.SystemTime.Send(client);
        }

        public static void Send(INetworkClient client)
        {
            client.AliveState = false;
            client.Alive_locker.Reset();

            var packet = new OutputPacketBuffer()
            {
                PacketId = (ushort)ClientPacketEnum.AliveConnection
            };
            client.Network.Send(packet);

            //client.Alive_locker.WaitOne(client.AliveWaitTime);
            //if (!client.AliveState)
            //    client.Network.Disconnect();
        }
    }
}
