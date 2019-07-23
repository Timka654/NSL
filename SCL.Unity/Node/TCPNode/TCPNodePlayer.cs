using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SCL.Node.Utils;
using SCL.Node.Utils.SystemPackets;
using UnityEngine;

namespace SCL.Node.TCPNode
{
    public class TCPNodePlayer : INodePlayer
    {
        public delegate void ReceivedHandle(TCPNodePlayer player, NodeInputPacketBuffer packet);

        public event ReceivedHandle OnReceived;

        public uint InputCurrentId { get; set; }

        public uint OutputCurrentId { get; set; }

        public EndPoint IpPoint;

        private TCPNode node;

        private Socket socket;

        public Socket Socket => socket;

        public int PlayerId { get; set; }

        public byte[] buffer = new byte[2048];

        public int offs = 0;

        public int len = 4;

        public bool header = true;

        public TCPNodePlayer(TCPNode node, IPEndPoint ipPoint)
        {
            this.node = node;
            IpPoint = ipPoint;
        }

        public void Connect()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IAsyncResult result = socket.BeginConnect(IpPoint, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(5000, true);

            Debug.LogError($"Connection {success} {IpPoint}");


            if (success)
            {
                socket.BeginReceive(buffer, offs, len - offs, SocketFlags.None, Receive, null);
            }
        }

        public void SetSocket(Socket socket)
        {
            this.socket = socket;
            this.socket.BeginReceive(buffer, offs, len - offs, SocketFlags.None, Receive, null);
        }

        private void Receive(IAsyncResult result)
        {
            try
            {
                int rlen = socket.EndReceive(result);

                if (rlen < 0)
                    throw new Exception();

                offs += rlen;

                if (offs < len)
                    socket.BeginReceive(buffer, offs, len - offs, SocketFlags.None, Receive, null);

                if(header)
                {
                    len = BitConverter.ToInt32(buffer, 0);
                }
                else
                {
                    NodeInputPacketBuffer packet = new NodeInputPacketBuffer(buffer.Take(len).ToArray());


                    Debug.Log($"Receive to {packet.PlayerId} id:{packet.PacketId}");
                    OnReceived?.Invoke(this, packet);

                    len = 4;
                    offs = 0;
                }

                header = !header;
                socket.BeginReceive(buffer, offs, len - offs, SocketFlags.None, Receive, null);

            }
            catch (Exception ex)
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            node.RemovePlayer(this);
            try
            {
            socket.Disconnect(false);

            }
            catch (Exception)
            {

                socket?.Dispose();
                socket = null;
            }
        }
        
        public void Send(NodeOutputPacketBuffer packet)
        {
            node.SendTo(this, packet);
        }
    }
}