using Mono.Nat;
using SCL.Node.Utils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SCL.Node.TCPNode
{
    public class TCPNode : INetworkNode<TCPNodePlayer>
    {
        public delegate void AppendClientHandle(TCPNodePlayer player);

        public event AppendClientHandle OnAppendClientEvent;

        public ConcurrentDictionary<string, TCPNodePlayer> WaitPlayerMap = new ConcurrentDictionary<string, TCPNodePlayer>();

        public override bool Initiliaze(string ip, ref int port, int myPlayerId)
        {
            base.Protocol = Protocol.Tcp;
            InitResult = true;
            _uPnPLocker.Reset();

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));

            base.Initiliaze(ip, ref port, myPlayerId);

            if (port == 0)
            {
                Port = port = (_socket.LocalEndPoint as IPEndPoint).Port;
            }

            _socket.Dispose();

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));

            _socket.Listen(20);
            _socket.BeginAccept(AcceptClient, _socket);

            _uPnPLocker.WaitOne();

            return InitResult;
        }

        private void AcceptClient(IAsyncResult result)
        {
            try
            {
                var client = _socket?.EndAccept(result);

                if (client == null)
                    return;
                TCPNodePlayer tnp;

                while (WaitPlayerMap.TryGetValue(((IPEndPoint)client.RemoteEndPoint).Address.ToString(), out tnp) == false)
                {
                    Thread.Sleep(10);
                }

                Debug.Log($"AcceptClient player: {tnp} IPEP:{client.RemoteEndPoint.ToString()}");
                tnp.SetSocket(client);

                OnAppendClientEvent?.Invoke(tnp);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }

            _socket?.BeginAccept(AcceptClient, _socket);
        }

        private void Player_OnReceived(TCPNodePlayer player, NodeInputPacketBuffer packet)
        {
#if DEBUG
            AppendOnReceiveData(player, packet);
#endif

            if (!_commands.ContainsKey(packet.PacketId))
            {
                Node.Utils.SystemPackets.InvalidPid.Send(player, packet.PacketId);

                return;
            }
            _actions.Enqueue(() => { _commands[packet.PacketId].Invoke(player, packet); });
        }

        #region Send

        public void SendTo(TCPNodePlayer player, NodeOutputPacketBuffer packet)
        {
            packet.PlayerId = MyPlayerId;
            Send(packet.GetBuffer(++player.OutputCurrentId), 0, packet.PacketLenght, player);
        }

        public void SendTo(int playerId, NodeOutputPacketBuffer packet)
        {
            if (_players.TryGetValue(playerId, out TCPNodePlayer player))
            {
                SendTo(player, packet);
            }
        }

        public void BroadcastMessage(NodeOutputPacketBuffer packet)
        {
            packet.PlayerId = MyPlayerId;

            foreach (TCPNodePlayer player in _players.Values)
            {
                Debug.Log($"BroadcastMessage to {player.PlayerId} id:{packet.PacketId}");
                Send(packet.GetBuffer(++player.OutputCurrentId), 0, packet.PacketLenght, player);
            }
        }

        private void Send(byte[] buffer, int offset, int length, TCPNodePlayer player)
        {
            player.Socket.Send(buffer, offset, length, SocketFlags.None);
        }

        #endregion

        private AutoResetEvent _addPlayerLocker = new AutoResetEvent(true);

        public void AddPlayer(IPEndPoint playerPoint, int playerId)
        {
            if (MyPlayerId == playerId)
                return;

            _addPlayerLocker.WaitOne(1000);

            if (!_players.ContainsKey(playerId))
            {

                Debug.Log($"AddPlayer id: {playerId} IPEP:{playerPoint.ToString()}");

                var tnp = new TCPNodePlayer(this, playerPoint) { PlayerId = playerId };

                tnp.OnReceived += Player_OnReceived;

                if (playerId > MyPlayerId)
                {
                    tnp.Connect();
                    Debug.Log($"AddPlayer connect to: {playerId} IPEP:{playerPoint.ToString()}");

                    OnAppendClientEvent?.Invoke(tnp);
                }
                else
                {
                    Debug.Log($"AddPlayer wait: {playerId} IPEP:{playerPoint.ToString()}");
                    WaitPlayerMap.TryAdd(playerPoint.Address.ToString(), tnp);
                }
                _players.TryAdd(playerId, tnp);
            }

            _addPlayerLocker.Set();
        }

        public void RemovePlayer(TCPNodePlayer player)
        {
            TCPNodePlayer old;
            if (_players.ContainsKey(player.PlayerId))
                _players.TryRemove(player.PlayerId, out old);
        }

        public void AddPacketHandle(byte id, CommandHandle handle)
        {
            if (_commands.ContainsKey(id))
                _commands.Remove(id);

            _commands.Add(id, handle);
        }
    }
}