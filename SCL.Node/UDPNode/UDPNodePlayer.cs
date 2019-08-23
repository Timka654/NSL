using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SCL.Node.Utils;
using SCL.Node.Utils.SystemPackets;

namespace SCL.Node.UDPNode
{
    public class UDPNodePlayer : INodePlayer
    {
        public delegate void ReceivedHandle(UDPNodePlayer player, NodeInputPacketBuffer packet);

        public event ReceivedHandle OnReceived;

        public uint InputCurrentId { get; set; }

        public uint OutputCurrentId { get; set; }

        public EndPoint IpPoint;

        private UDPNode node;

        public int PlayerId { get; set; }

        public byte[] buffer = new byte[1024];

        public UDPNodePlayer(UDPNode node, IPEndPoint ipPoint)
        {
            this.node = node;
            IpPoint = ipPoint;

            Receive();
        }

        private async void Receive()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    int len = node.ReceiveFrom(buffer, ref IpPoint);

                    if (len < 0)
                        return;

                    NodeInputPacketBuffer packet = null;
                    try
                    {
                        packet = new NodeInputPacketBuffer(buffer, true);

                        if (packet.PlayerId != PlayerId)
                            throw new Exception();

                        if (InputCurrentId + 1 != packet.Cpid)
                        {
                            InvalidPid.Send(this,InputCurrentId);
                        }

                        InputCurrentId++;
                    }
                    catch
                    {
                        return;
                    }

                    OnReceived?.Invoke(this, packet);
                }
            });
        }
        
        public void Send(NodeOutputPacketBuffer packet)
        {
            node.SendTo(this, packet);
        }
    }
}