//using Mono.Nat;
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

        public int Backlog { get; set; }

        #region UnhandledProcess

        public bool UnhandledPlayerProcess { get; set; } = false;

        protected readonly Dictionary<byte, CommandHandle> _unhandledPlayerCommands = new Dictionary<byte, CommandHandle>();

        public void AddUnhandledPlayerPacketHandle(byte id, CommandHandle handle)
        {
            if (_unhandledPlayerCommands.ContainsKey(id))
            {
                Debug.LogError($"Node network: (AddUnhandledPlayerPacketHandle) packet {id} already exist, be removed and append new");
                _unhandledPlayerCommands.Remove(id);
            }

            _unhandledPlayerCommands.Add(id, handle);
        }

        #endregion

        public void InitializeClient(string serverIp, int port, int myPlayerId)
        {
            //base.Protocol = Protocol.Tcp;
            InitResult = true;

            MyPlayerId = myPlayerId;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _socket.Connect(new IPEndPoint(IPAddress.Parse(serverIp), port));

            TCPNodePlayer tnp = new TCPNodePlayer(this, (IPEndPoint)_socket.RemoteEndPoint);
            tnp.OnReceived += Player_OnReceived;
            tnp.SetSocket(_socket);
            if (_players.TryRemove(-1, out var host))
                host.Disconnect();
            _players.TryAdd(-1, tnp);
        }

        /// <summary>
        /// Server Initializer
        /// if model is all peers to all peers
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="myPlayerId"></param>
        /// <returns></returns>
        public override bool Initiliaze(string ip, ref int port, int myPlayerId)
        {
            //base.Protocol = Protocol.Tcp;
            InitResult = true;
            _uPnPLocker.Reset();

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));

            if (port == 0)
            {
                Port = port = (_socket.LocalEndPoint as IPEndPoint).Port;
            }

            base.Initiliaze(ip, ref port, myPlayerId);

            _socket.Listen(Backlog);
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
                TCPNodePlayer tnp = null;

                if (UnhandledPlayerProcess && WaitPlayerMap.TryGetValue(((IPEndPoint)client.RemoteEndPoint).Address.ToString(), out tnp) == false)
                {
                    tnp = new TCPNodePlayer(this, (IPEndPoint)client.RemoteEndPoint);
                    tnp.PlayerId = GetUnhandledId();
                    tnp.OnReceived += UnhandledPlayer_OnReceived;

                    _players.TryAdd(tnp.PlayerId, tnp);
                }
                else
                    while (WaitPlayerMap.TryGetValue(((IPEndPoint)client.RemoteEndPoint).Address.ToString(), out tnp) == false)
                    {
                        Thread.Sleep(30);
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
                Debug.LogError($"Handled player packet {packet.PacketId} not contains in map");

                return;
            }
            _actions.Enqueue(() => { _commands[packet.PacketId].Invoke(player, packet); });
        }

        private void UnhandledPlayer_OnReceived(TCPNodePlayer player, NodeInputPacketBuffer packet)
        {
#if DEBUG
            AppendOnReceiveData(player, packet);
#endif

            if (!_unhandledPlayerCommands.ContainsKey(packet.PacketId))
            {
                Debug.LogError($"Unhandled player packet {packet.PacketId} not contains in map");
                //Node.Utils.SystemPackets.InvalidPid.Send(player, packet.PacketId);

                return;
            }
            _actions.Enqueue(() => { _unhandledPlayerCommands[packet.PacketId].Invoke(player, packet); });
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

        /// <summary>
        /// Add wait players
        /// if model is all peers to all peers
        /// </summary>
        /// <param name="playerPoint"></param>
        /// <param name="playerId"></param>
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

        public void HandleUnhandledPlayer(TCPNodePlayer player, int playerId)
        {
            if (!UnhandledPlayerProcess)
                throw new Exception("This node not UnhandledPlayerProcess");

            if (player.PlayerId > 0)
                throw new Exception("Player is already handled");

            _players.TryRemove(player.PlayerId, out var dummy);
            _players.TryRemove(playerId, out dummy);

            player.PlayerId = playerId;

            player.OnReceived -= UnhandledPlayer_OnReceived;
            player.OnReceived += Player_OnReceived;
            _players.TryAdd(playerId, player);

            Debug.Log($"Handled player {playerId}");
        }
        
        private static System.Random rnd = new System.Random();

        public int GetUnhandledId()
        {
            int result;
            do
            {
                result = rnd.Next(int.MinValue , -2);
            } while (_players.ContainsKey(result));

            return result;
        }
    }
}