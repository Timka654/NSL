using NSL.Node.BridgeServer.Shared;
using NSL.Node.Core.Models.Message;
using NSL.Node.RoomServer.Shared.Client.Core;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;
using NSL.SocketCore.Utils.Buffer;
using NSL.UDP;
using NSL.UDP.Enums;
using NSL.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static NSL.Node.RoomServer.Shared.Client.Core.IRoomInfo;

namespace NSL.Node.RoomServer.Client.Data
{
    public class RoomInfo : IServerRoomInfo, IDisposable
    {
        private ConcurrentDictionary<string, TransportNetworkClient> nodes = new ConcurrentDictionary<string, TransportNetworkClient>();

        public IEnumerable<NodeInfo> GetNodes() { return nodes.Values.Select(x => x.Node); }

        private Dictionary<ushort,
            ReciveHandleDelegate> handles = new Dictionary<ushort, ReciveHandleDelegate>();

        private event Action<byte[], int, int> broadcastDelegate = (buf, offset, len) => { };

        private IRoomSession Game;

        public NodeRoomServerEntry Entry { get; }

        public Guid SessionId { get; }

        public Guid RoomId { get; }

        public int ConnectedNodesCount => nodes.Count;

        public IEnumerable<TransportNetworkClient> Nodes => nodes.Values;

        public DateTime CreateTime { get; } = DateTime.UtcNow;

        public NodeRoomStartupInfo StartupInfo { get; private set; }

        public int RoomNodeCount { get; private set; }

        public bool RoomWaitAllReady { get; private set; }

        public bool RoomReady { get; private set; }

        public int StartupTimeout { get; private set; }

        public bool ShutdownOnMissedReady { get; private set; }

        public string LocalNodeId => Guid.Empty.ToString();

        public event OnNodeDelegate OnNodeConnect = node => Task.CompletedTask;

        public event OnNodeDisconnectDelegate OnNodeDisconnect = (node, manualDisconnect) => Task.CompletedTask;

        public event OnNodeDelegate OnNodeConnectionLost = node => Task.CompletedTask;

        public event Func<Task> OnRoomReady = () => Task.CompletedTask;

        public event Func<Task> OnRoomDisposed = () => Task.CompletedTask;

        public event Func<NodeInfo, bool> OnValidateSession = c => true;

        public event OnNodeDelegate OnRecoverySession = node => Task.CompletedTask;

        private AutoResetEvent ar = new AutoResetEvent(true);

        public RoomInfo(NodeRoomServerEntry entry, Guid sessionId, Guid roomId)
        {
            Entry = entry;
            SessionId = sessionId;
            RoomId = roomId;
        }

        public async Task<bool> AddClient(TransportNetworkClient client)
        {
            client.Node = new NodeInfo(client, client.Id);

            var current = nodes.GetOrAdd(client.Id, client);

            if (current != client)
            {
                if (current.Network?.GetState() == true)
                {
                    current.Disconnect();
                }
                else
                    await disconnectNode(current);

                current = nodes.AddOrUpdate(client.Id, client, (k, o) => client);
            }

            if (current != client)
                return false;

            broadcastDelegate += client.Send;

            BroadcastConnectNode(client);

            connectPlayer(client);

            return true;
        }

        public async void OnClientDisconnected(TransportNetworkClient client)
        {
            if (client.ManualDisconnected)
                await disconnectNode(client);
            else
                await OnNodeConnectionLost.InvokeAsync(x => x(client.Node));
            // implement with session manager
        }

        public async void DisconnectNode(TransportNetworkClient client)
        {
            await disconnectNode(client);
        }

        private async Task disconnectNode(TransportNetworkClient client)
        {
            if (nodes.TryRemove(client.Id, out var oldClient))
            {
                BroadcastConnectionLostNode(client);

                client.Room = null;

                broadcastDelegate -= client.Send;

                BroadcastDisconnectNode(client);

                await OnNodeDisconnect.InvokeAsync(x => x(client.Node, client.ManualDisconnected));

                if (StartupInfo.GetDestroyOnEmpty() && !nodes.Any())
                {
                    Dispose();
                }
            }
        }

        internal async Task SetStartupInfo(NodeRoomStartupInfo startupInfo)
        {
            RoomWaitAllReady = startupInfo.GetRoomWaitReady();

            RoomReady = !RoomWaitAllReady;

            RoomNodeCount = startupInfo.GetRoomNodeCount();

            StartupTimeout = startupInfo.GetRoomStartupTimeout();

            ShutdownOnMissedReady = startupInfo.GetRoomShutdownOnMissed();

            StartupInfo = startupInfo;

            if (ShutdownOnMissedReady)
                RunDestroyOnMissedTimer();

            Game = Entry.CreateRoomSession(this);

            if (RoomReady)
                await OnRoomReady.InvokeAsync(x => x());
        }

        public Dictionary<string, string> GetClientOptions()
            => new()
            {
                { "waitAll", this.RoomWaitAllReady.ToString() },
                { "nodeWaitCount", this.RoomNodeCount.ToString() }
            };

        private async void RunDestroyOnMissedTimer()
        {
            await Task.Delay(StartupTimeout);

            if (ConnectedNodesCount == RoomNodeCount)
                return;

            Dispose();
        }

        static TimeSpan perfMax = TimeSpan.Zero;


        private async void connectPlayer(TransportNetworkClient node)
        {
            if (node.Ready)
                return;

            await Task.Delay(1_000); // response wait for send

            var test = Stopwatch.StartNew();

            bool isLocked = RoomWaitAllReady && !RoomReady;

            if (isLocked)
                ar.WaitOne();

            if (nodes.TryGetValue(node.Id, out var current) && current == node) // check for player can duplicate connection and replace previous
            {
                node.Ready = true;

                await OnNodeConnect.InvokeAsync(x => x(node.Node));

                if (RoomWaitAllReady && !RoomReady)
                {
                    if (Nodes.All(x => x.Ready))
                    {
                        RoomReady = true;

                        await OnRoomReady.InvokeAsync();

                        Broadcast(CreateReadyRoomPacket());
                    }
                }
                else
                    SendTo(node, CreateReadyRoomPacket());

                Entry.Logger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Debug, $"connectPlayer {node.Id} send ready on {test.ElapsedMilliseconds}ms");
            }
            else
            {
                Entry.Logger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Debug, $"connectPlayer {node.Id} not equals");
            }

            if (isLocked)
                ar.Set();

        }

        protected OutputPacketBuffer CreateReadyRoomPacket()
        {
            var p = OutputPacketBuffer.Create(RoomPacketEnum.ReadyRoomMessage);

            p.WriteDateTime(CreateTime);

            return p;
        }

        private void BroadcastConnectNode(TransportNetworkClient node)
        {
            var buffer = OutputPacketBuffer.Create(RoomPacketEnum.NodeConnectMessage);

            new ConnectNodeMessageModel()
            {
                NodeId = node.NodeId,
                Token = node.Token,
                EndPoint = node.EndPoint
            }.WriteFullTo(buffer);

            Broadcast(buffer);
        }

        private void SendConnectNodeInformation(TransportNetworkClient to, TransportNetworkClient node)
        {
            var buffer = OutputPacketBuffer.Create(RoomPacketEnum.NodeConnectMessage);

            new ConnectNodeMessageModel()
            {
                NodeId = node.NodeId,
                Token = node.Token,
                EndPoint = node.EndPoint
            }.WriteFullTo(buffer);

            SendTo(to, buffer);
        }

        private void BroadcastRoomDestroy()
            => Broadcast(OutputPacketBuffer.Create(RoomPacketEnum.RoomDestroyMessage));

        private void SendRoomDestroy(TransportNetworkClient to)
            => SendTo(to, OutputPacketBuffer.Create(RoomPacketEnum.RoomDestroyMessage));

        private void BroadcastConnectionLostNode(TransportNetworkClient node)
        {
            var buffer = OutputPacketBuffer.Create(RoomPacketEnum.NodeConnectionLostMessage);

            buffer.WriteString(node.NodeId);

            Broadcast(buffer);
        }

        private void BroadcastChangeNodeEndPoint(TransportNetworkClient node)
        {
            var buffer = OutputPacketBuffer.Create(RoomPacketEnum.NodeChangeEndPointMessage);

            buffer.WriteString(node.NodeId);
            buffer.WriteString(node.EndPoint);

            Broadcast(buffer);
        }

        private void BroadcastDisconnectNode(TransportNetworkClient node)
        {
            var buffer = OutputPacketBuffer.Create(RoomPacketEnum.NodeDisconnectMessage);

            buffer.WriteString(node.NodeId);

            Broadcast(buffer);
        }

        #region Broadcast

        public async void Broadcast(OutputPacketBuffer packet, bool disposeOnSend = true)
        {

            await Task.Run(() =>
            {
                var buf = packet.CompilePacket();

                broadcastDelegate(buf, 0, buf.Length);

                if (disposeOnSend)
                    packet.Dispose();
            });
        }

        public void Broadcast(DgramOutputPacketBuffer packet, bool disposeOnSend = true)
        {
            Broadcast((OutputPacketBuffer)packet, disposeOnSend);
        }

        public void Broadcast(DgramOutputPacketBuffer packet, UDPChannelEnum channel, bool disposeOnSend = true)
        {
            Broadcast(packet, disposeOnSend);
        }

        public bool Broadcast(ushort code, Action<DgramOutputPacketBuffer> builder)
        {
            return Broadcast(packet =>
            {
                packet.WriteUInt16(code);
                builder(packet);
            });
        }

        public bool Broadcast(ushort code, UDPChannelEnum channel, Action<DgramOutputPacketBuffer> builder)
        {
            return Broadcast(code, builder);
        }

        public bool Broadcast(Action<DgramOutputPacketBuffer> builder)
        {
            var packet = new DgramOutputPacketBuffer();

            packet.PacketId = (ushort)RoomPacketEnum.ExecuteMessage;

            builder(packet);

            Broadcast(packet);

            return true;
        }

        public bool Broadcast(UDPChannelEnum channel, Action<DgramOutputPacketBuffer> builder)
        {
            return Broadcast(builder);
        }

        #endregion

        #region SendTo

        public bool SendTo(string nodeId, OutputPacketBuffer packet, bool disposeOnSend = true)
        {
            if (nodes.TryGetValue(nodeId, out var node))
                return SendTo(node, packet, disposeOnSend);
            else if (disposeOnSend)
                packet.Dispose();

            return false;
        }

        public bool SendTo(string nodeId, UDPChannelEnum channel, OutputPacketBuffer packet, bool disposeOnSend = true)
        {
            return SendTo(nodeId, packet, disposeOnSend);
        }

        public bool SendTo(TransportNetworkClient node, OutputPacketBuffer packet, bool disposeOnSend = true)
        {
            if (node.Network != null)
            {
                node.Network.Send(packet, disposeOnSend);
                return true;
            }
            else if (disposeOnSend)
                node.Dispose();

            return false;
        }

        public bool SendTo(TransportNetworkClient node, UDPChannelEnum channel, OutputPacketBuffer packet, bool disposeOnSend = true)
        {
            return SendTo(node, packet, disposeOnSend);
        }

        public bool SendTo(TransportNetworkClient node, DgramOutputPacketBuffer packet, bool disposeOnSend = true)
        {
            return SendTo(node, (OutputPacketBuffer)packet, disposeOnSend);
        }

        public bool SendTo(TransportNetworkClient node, UDPChannelEnum channel, DgramOutputPacketBuffer packet, bool disposeOnSend = true)
        {
            return SendTo(node, packet, disposeOnSend);
        }

        public bool SendTo(string nodeId, DgramOutputPacketBuffer packet, bool disposeOnSend = true)
        {
            if (nodes.TryGetValue(nodeId, out var node))
                return SendTo(node, packet, disposeOnSend);
            else if (disposeOnSend)
                packet.Dispose();

            return false;
        }

        public bool SendTo(string nodeId, UDPChannelEnum channel, DgramOutputPacketBuffer packet, bool disposeOnSend = true)
            => SendTo(nodeId, packet, disposeOnSend);

        public bool SendTo(NodeInfo node, DgramOutputPacketBuffer packet, bool disposeOnSend = true)
        {
            return SendTo(node.Network as TransportNetworkClient, packet, disposeOnSend);
        }

        public bool SendTo(NodeInfo node, UDPChannelEnum channel, DgramOutputPacketBuffer packet, bool disposeOnSend = true)
        {
            return SendTo(node, packet, disposeOnSend);
        }

        public bool SendTo(string nodeId, ushort command, Action<DgramOutputPacketBuffer> build)
        {
            DgramOutputPacketBuffer packet = new DgramOutputPacketBuffer();

            packet.PacketId = (ushort)RoomPacketEnum.ExecuteMessage;

            packet.WriteUInt16(command);

            build(packet);

            return SendTo(nodeId, packet);
        }


        public bool SendTo(NodeInfo node, byte[] buffer)
        {
            if (node.Network != null)
            {
                (node.Network as TransportNetworkClient).Send(buffer);
                return true;
            }
            return false;
        }
        public bool SendTo(NodeInfo node, byte[] buffer, int offset, int len)
        {
            if (node.Network != null)
            {
                (node.Network as TransportNetworkClient).Send(buffer, offset, len);
                return true;
            }
            return false;
        }

        public bool SendTo(string nodeId, ushort command, UDPChannelEnum channel, Action<DgramOutputPacketBuffer> build)
        {
            return SendTo(nodeId, command, build);
        }

        public bool SendTo(NodeInfo node, ushort command, Action<DgramOutputPacketBuffer> build)
        {
            DgramOutputPacketBuffer packet = new DgramOutputPacketBuffer();

            packet.PacketId = (ushort)RoomPacketEnum.ExecuteMessage;

            packet.WriteUInt16(command);

            build(packet);

            return SendTo(node, packet);
        }

        public bool SendTo(NodeInfo node, ushort command, UDPChannelEnum channel, Action<DgramOutputPacketBuffer> build)
            => SendTo(node, command, build);

        #endregion

        #region SendToServer

        public void SendToServer(ushort command, Action<OutputPacketBuffer> build)
        {
            throw new AlreadyOnServerException();
        }

        public void SendToServer(OutputPacketBuffer packet)
        {
            throw new AlreadyOnServerException();
        }

        public void SendToServer(OutputPacketBuffer packet, bool disposeOnSend = true)
        {
            throw new AlreadyOnServerException();
        }

        #endregion

        public void Execute(TransportNetworkClient client, InputPacketBuffer packet)
        {
            try
            {
                if (handles.TryGetValue(packet.ReadUInt16(), out var command))
                {
                    command(client.Node, packet);
                }
            }
            catch (Exception ex)
            {
                Entry.Logger.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, $"{nameof(Execute)} {nameof(client.Id)} throw error\r\n{ex}");
            }
        }

        public void Transport(TransportNetworkClient client, InputPacketBuffer packet)
        {
            var body = packet.Data;

            var to = packet.ReadString();

            OutputPacketBuffer pbuf = OutputPacketBuffer.Create(RoomPacketEnum.TransportMessage);

            pbuf.Write(body);

            SendTo(to, pbuf);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterHandle(ushort code, ReciveHandleDelegate handle)
        {
            if (!handles.TryAdd(code, handle))
                throw new Exception($"code {code} already contains in {nameof(handles)}");
        }

        public NodeInfo GetNode(string id)
        {
            if (nodes.TryGetValue(id, out var node))
                return node.Node;

            return default;
        }

        public void Dispose()
        {
            Dispose(null);
        }

        public bool Destroyed { get; private set; }

        public async void Dispose(byte[] data)
        {
            if (Destroyed)
                return;

            Destroyed = true;

            await OnRoomDisposed.InvokeAsync(x => x());

            BroadcastRoomDestroy();

            SendLobbyFinishRoom(data);
        }

        public void SendLobbyFinishRoom(byte[] data = null)
        {
            Entry.RoomFinishHandle(this, data);
        }

        public void SendLobbyRoomMessage(byte[] data)
        {
            Entry.RoomMessageHandle(this, data);
        }

        public TValue GetOption<TValue>(string key)
            where TValue : IConvertible
            => StartupInfo.GetValue<TValue>(key);

        internal bool TryRecoverySession(TransportNetworkClient transportNetworkClient)
        {
            if (Destroyed)
            {
                SendRoomDestroy(transportNetworkClient);

                return false;
            }

            if (nodes.ContainsKey(transportNetworkClient.NodeId))
            {
                nodes[transportNetworkClient.NodeId] = transportNetworkClient;

                BroadcastConnectNode(transportNetworkClient);
            }

            return true;
        }

        internal void ChangeNodeConnectionPoint(TransportNetworkClient client, string endPoint)
        {
            client.EndPoint = endPoint;

            BroadcastChangeNodeEndPoint(client);
        }

        public bool ValidateSession(NodeInfo node)
        {
            return OnValidateSession(node);
        }

        public async Task RecoverySession(NodeInfo node)
        {
            await OnRecoverySession.InvokeAsync(x => x(node));
        }
    }
}
