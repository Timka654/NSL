﻿using NSL.Extensions.NAT.Proxy.Data.Packets.Enums;
using NSL.SocketClient;
using NSL.SocketClient.Utils;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.Extensions.NAT.Proxy.Data.Packets
{
    internal class TransportDataPacket : IPacketMessage<NetworkProxyClient, (string, InputPacketBuffer)>
    {
        public TransportDataPacket(ClientOptions<NetworkProxyClient> options) : base(options)
        {
        }

        protected override void Receive(InputPacketBuffer data)
        {
            InvokeEvent((data.ReadString(), new InputPacketBuffer(data.Read(data.ReadInt32()))));
        }

        public void Send(byte[] data)
        {
            OutputPacketBuffer packet = new OutputPacketBuffer(data.Length + OutputPacketBuffer.DefaultHeaderLength)
            {
                PacketId = (ushort)ServerPacketsEnum.Transport
            };

            packet.WriteInt32(data.Length);

            packet.Write(data);

            Send(packet);
        }
    }
}
