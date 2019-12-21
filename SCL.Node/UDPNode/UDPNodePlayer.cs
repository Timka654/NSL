using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
//using ReliableNetcode;
using SCL.Node.Utils;
using SCL.Node.Utils.SystemPackets;

namespace SCL.Node.UDPNode
{
    public class UDPNodePlayer : INodePlayer
    {
        //ReliableNetcode.ReliableEndpoint reliable = new ReliableNetcode.ReliableEndpoint();

        public delegate void ReceivedHandle(UDPNodePlayer player, NodeInputPacketBuffer packet);

        public event ReceivedHandle OnReceived;

        public uint InputCurrentId { get; set; }

        public uint OutputCurrentId { get; set; }

        public EndPoint IpPoint;

        private UDPNode UDPNetworkNode;

        public override INetworkNode NetworkNode { get; set; }

        public override int PlayerId { get; internal set; }

        public byte[] buffer = new byte[1024];

        public UDPNodePlayer(UDPNode node, IPEndPoint ipPoint)
        {
            NetworkNode = UDPNetworkNode = node;
            IpPoint = ipPoint;

            //reliable.TransmitCallback += (buffer, len) =>
            //{
            //    node.SendTo(this, buffer, len);
            //};

            //reliable.ReceiveCallback += (buffer, len) =>
            //{
            //    NodeInputPacketBuffer packet = null;
            //    try
            //    {
            //        packet = new NodeInputPacketBuffer(buffer, true);

            //        if (packet.PlayerId != PlayerId)
            //            throw new Exception();

            //        if (InputCurrentId + 1 != packet.Cpid)
            //        {
            //            InvalidPid.Send(this, InputCurrentId);
            //        }

            //        InputCurrentId++;
            //    }
            //    catch
            //    {
            //        return;
            //    }

            //    OnReceived?.Invoke(this, packet);
            //};

            Receive();
        }

        private async void Receive()
        {
            await Task.Run(() =>
            {
                while (UDPNetworkNode != null)
                {
                    int len = UDPNetworkNode.ReceiveFrom(buffer, ref IpPoint);

                    if (len < 0)
                        return;

                    //reliable.ReceivePacket(buffer, buffer.Length);
                }
            });
        }

        //public override void Send(NodeOutputPacketBuffer packet, QosType qos)
        //{
        //    packet.PlayerId = PlayerId;
        //    reliable.SendMessage(packet.GetBuffer(), packet.PacketLenght, qos);
        //}
    }
}