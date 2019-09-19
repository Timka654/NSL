using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ProxyHostClient.Packets.Auth
{
    public delegate void GetProxyServerHandle(IPEndPoint ip, int roomId);

    [ProxyHostPacket(Enums.ClientPacketsEnum.GetProxyServerResult)]
    public class GetProxyServer : IPacket<ProxyHostClientData>
    {
        public event GetProxyServerHandle OnReceive;
        public void Receive(ProxyHostClientData client, InputPacketBuffer data)
        {
            bool result = data.ReadBool();
            int roomId = data.ReadInt32();
            IPEndPoint ipep = null;
            if (result)
            {
                var ip = data.ReadString16();
                var port = data.ReadInt32();
                ipep = new IPEndPoint(IPAddress.Parse(ip), port);
            }

            OnReceive?.Invoke(ipep, roomId);
        }

        public static void Send(ProxyHostClientData client, int roomId)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(Enums.ServerPacketsEnum.GetProxyServer);

            packet.WriteInt32(roomId);;

            client.Network.Send(packet);
        }
    }
}
