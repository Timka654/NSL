using Mono.Nat;
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
    public class UDPNode : MonoBehaviour
    {
        public delegate void CommandHandle(UDPNodePlayer player, NodeInputPacketBuffer buffer);

#if DEBUG
        public event CommandHandle OnReceivePacket;
#endif

        private Socket _socket;

        public int Port { get; private set; }

        public int MyPlayerId { get; set; }

        private ManualResetEvent _uPnPLocker = new ManualResetEvent(false);

        private readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();

        private readonly Dictionary<int, UDPNodePlayer> _players = new Dictionary<int, UDPNodePlayer>();

        private readonly Dictionary<byte, CommandHandle> _commands = new Dictionary<byte, CommandHandle>();

        private bool InitResult { get; set; } = false;

        public bool Initiliaze(string ip, ref int port, int playerId)
        {
            MyPlayerId = playerId;

            InitResult = true;
            _uPnPLocker.Reset();

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));

            NatUtility.DeviceFound += DeviceFound;
            NatUtility.DeviceLost += DeviceLost;
            NatUtility.StartDiscovery();

            if (port == 0)
            {
                Port = port = (_socket.LocalEndPoint as IPEndPoint).Port;
            }

            _socket.Dispose();

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            _uPnPLocker.WaitOne();

            return InitResult;
        }

        #region NAT

        private async void DeviceFound(object sender, DeviceEventArgs args)
        {
            try
            {
                INatDevice device = args.Device;

                Mapping mapping = new Mapping(Protocol.Udp, Port, Port);
                await device.CreatePortMapAsync(mapping);
            }
            catch (Exception ex)
            {
                InitResult = false;
                _socket.Dispose();
                Debug.LogError(ex.ToString());
            }
            _uPnPLocker.Set();
        }

        private void DeviceLost(object sender, DeviceEventArgs args)
        {
            _socket.Dispose();
            InitResult = false;
            Debug.LogError("Router device is lost");
            _uPnPLocker.Set();
        }

        #endregion

        private void Player_OnReceived(UDPNodePlayer player, NodeInputPacketBuffer packet)
        {
            if (_commands.ContainsKey(packet.PacketId))
                _actions.Enqueue(() => { _commands[packet.PacketId].Invoke(player, packet); });

#if DEBUG
            _actions.Enqueue(() => { OnReceivePacket?.Invoke(player, packet); });
#endif

        }
        
        public void SendTo(UDPNodePlayer player, NodeOutputPacketBuffer packet)
        {
            packet.PlayerId = MyPlayerId;
            Send(packet.GetBuffer(++player.OutputCurrentId), 0, packet.PacketLenght, player);
        }

        public void SendTo(int playerId, NodeOutputPacketBuffer packet)
        {
            UDPNodePlayer player;

            if (_players.TryGetValue(playerId, out player))
            {
                SendTo(player, packet);
            }
        }
        
        public void BroadcastMessage(NodeOutputPacketBuffer packet)
        {
            packet.PlayerId = MyPlayerId;

            foreach (var player in _players.Values)
            {
                Send(packet.GetBuffer(++player.OutputCurrentId), 0, packet.PacketLenght, player);
            }
        }

        private void Send(byte[] buffer, int offset, int length, UDPNodePlayer player)
        {
            _socket.SendTo(buffer, offset, length, SocketFlags.None, player.IpPoint);
        }

        public void AddPlayer(IPEndPoint playerPoint, int playerId)
        {
            if (MyPlayerId == playerId)
                return;
            if (_players.ContainsKey(playerId))
                _players.Remove(playerId);
            _players.Add(playerId, new UDPNodePlayer(this, playerPoint) {PlayerId = playerId});

            _players[playerId].OnReceived += Player_OnReceived;
        }

        public int ReceiveFrom(byte[] buffer, ref EndPoint endPoint)
        {
            return _socket.ReceiveFrom(buffer, ref endPoint);
        }

        private void FixedUpdate()
        {
            Action action = null;
            while (_actions.TryDequeue(out action))
            {
                action.Invoke();
            }
        }

        private void OnDestroy()
        {
            Destroy();
        }

        public void Destroy()
        {
            if (_socket == null)
                return;
            try
            {
                _socket.Close();
                _socket.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}