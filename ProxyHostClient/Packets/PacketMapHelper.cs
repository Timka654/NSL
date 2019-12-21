using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using Utils.Helper.Packet;

namespace ProxyHostClient.Packets
{
    public class ProxyHostPacketAttribute : PacketAttribute
    {
        public ProxyHostPacketAttribute(Packets.Enums.ClientPacketsEnum packetId) : base((ushort)packetId)
        {
        }
    }

    internal static class PacketMapHelper
    {
        public static void SetPacketId(this OutputPacketBuffer packet, Enums.ServerPacketsEnum packetId)
        {
            packet.PacketId = (ushort)packetId;
        }
    }
}
