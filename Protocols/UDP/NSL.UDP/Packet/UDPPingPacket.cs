using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.SystemPackets;

namespace NSL.UDP.Packet
{
    public static class UDPPingPacketExtensions
    {
        /// <summary>
        /// Registers a combined ping/pong handler on <see cref="AliveConnectionPacket.PacketId"/>.
        /// Both sides of a UDP connection can call this — the handler automatically distinguishes
        /// between an incoming ping request (echoes back) and an incoming pong response (records RTT).
        /// Enable ping loop on the initiating side via <see cref="NSL.SocketCore.Utils.BaseNetworkConnection.PingPongEnabled"/>.
        /// </summary>
        public static void RegisterUDPPingHandle(this CoreOptions options)
        {
            options.AddPacketHandle(AliveConnectionPacket.PacketId, (client, data) =>
            {
                if (client.IsPingPending)
                    client.PongProcess();
                else
                    client.Network.SendEmpty(AliveConnectionPacket.PacketId);
            });
        }
    }
}
