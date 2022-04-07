using SCL.Node.NAT.Proxy.Data.Packets.Enums;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketClient;
using System.Net;
using SocketClient.Utils;
using SCL.Node.NAT.Proxy.Data.Packets.PacketData;

namespace SCL.Node.NAT.Proxy.Data.Packets
{
    internal class TransportDataPacket : IPacketMessage<NetworkProxyClient, (string, InputPacketBuffer)>
    {
        public TransportDataPacket(ClientOptions<NetworkProxyClient> options) : base(options)
        {
        }

        protected override void Receive(InputPacketBuffer data)
        {
            InvokeEvent((data.ReadString16(), new InputPacketBuffer(data.Read(data.ReadInt32()))));
        }

        public void Send(byte[] data)
        {
            OutputPacketBuffer packet = new OutputPacketBuffer(data.Length + OutputPacketBuffer.headerLenght)
            {
                PacketId = (ushort)ServerPacketsEnum.Transport
            };

            packet.WriteInt32(data.Length);

            packet.Write(data);

            Send(packet);
        }
    }
}
