//using Mono.Nat;
//using ReliableNetcode;
using SCL.Node.Utils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SCL.Node.UDPNode
{
    public class UDPNode : INetworkNode<UDPNodePlayer>
    {
        public override bool Initiliaze(string ip, ref int port, int myPlayerId)
        {
            //base.Protocol = Protocol.Udp;
            InitResult = true;
            _uPnPLocker.Reset();

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));

            base.Initiliaze(ip, ref port, myPlayerId);

            if (port == 0)
            {
                Port = port = (_socket.LocalEndPoint as IPEndPoint).Port;
            }

            _socket.Dispose();

            _socket = null;

            base.Initiliaze(ip, ref port, myPlayerId);

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            _socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));

            _uPnPLocker.WaitOne();

            return InitResult;
        }


        private void Player_OnReceived(UDPNodePlayer player, NodeInputPacketBuffer packet)
        {
            if (_commands.ContainsKey(packet.PacketId))
                _actions.Enqueue(() => { _commands[packet.PacketId].Invoke(player, packet); });

#if DEBUG
            AppendOnReceiveData(player, packet);
#endif

        }

        internal void SendTo(UDPNodePlayer player, byte[] packet, int len)
        {
            Send(packet, 0, len, player);
        }

        public void SendTo(UDPNodePlayer player, NodeOutputPacketBuffer packet)
        {
            packet.PlayerId = MyPlayerId;
            Send(packet.GetBuffer(++player.OutputCurrentId), 0, packet.PacketLenght, player);
        }

        //public void SendTo(int playerId, NodeOutputPacketBuffer packet, QosType qos)
        //{
        //    UDPNodePlayer player;

        //    if (_players.TryGetValue(playerId, out player))
        //    {
        //        player.Send(packet, qos);
        //    }
        //}

        //public void BroadcastMessage(NodeOutputPacketBuffer packet, QosType qos)
        //{
        //    packet.PlayerId = MyPlayerId;

        //    foreach (UDPNodePlayer player in _players.Values)
        //    {
        //        player.Send(packet, qos);
        //    }
        //}

        private void Send(byte[] buffer, int offset, int length, UDPNodePlayer player)
        {
            //byte[] buf = buffer;
            //if (offset > 0)
            //{
            //    buf = new byte[length - offset];

            //    Array.Copy(buffer, offset, buf, 0, length);
            //}
            _socket.SendTo(buffer, offset, length, SocketFlags.None, player.IpPoint);
        }

        public void AddPlayer(IPEndPoint playerPoint, int playerId)
        {
            if (MyPlayerId == playerId)
                return;
            if (_players.ContainsKey(playerId))
                _players.TryRemove(playerId, out var dummyPlayer);

            var player = new UDPNodePlayer(this, playerPoint) { PlayerId = playerId };

            _players.TryAdd(playerId, player);

            player.OnReceived += Player_OnReceived;
        }

        public int ReceiveFrom(byte[] buffer, ref EndPoint endPoint)
        {
            return _socket.ReceiveFrom(buffer, ref endPoint);
        }
    }
}