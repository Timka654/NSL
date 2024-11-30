using NSL.SocketCore.Utils.Buffer;
using NSL.UDP;
using NSL.UDP.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSL.Node.RoomServer.Shared.Client.Core
{
    public interface IRoomInfo
    {

        public delegate Task OnNodeDisconnectDelegate(NodeInfo node, bool manualDisconnected);

        public delegate Task OnNodeDelegate(NodeInfo node);

        void RegisterHandle(ushort command, ReciveHandleDelegate action);


        void Broadcast(DgramOutputPacketBuffer packet, bool disposeOnSend = true); // +
        void Broadcast(DgramOutputPacketBuffer packet, UDPChannelEnum channel, bool disposeOnSend = true); // +

        bool Broadcast(ushort code, Action<DgramOutputPacketBuffer> builder);
        bool Broadcast(ushort code, UDPChannelEnum channel, Action<DgramOutputPacketBuffer> builder);

        bool Broadcast(Action<DgramOutputPacketBuffer> builder);
        bool Broadcast(UDPChannelEnum channel, Action<DgramOutputPacketBuffer> builder);


        bool SendTo(string nodeId, DgramOutputPacketBuffer packet, bool disposeOnSend = true); // +
        bool SendTo(string nodeId, UDPChannelEnum channel, DgramOutputPacketBuffer packet, bool disposeOnSend = true); // +

        bool SendTo(NodeInfo node, DgramOutputPacketBuffer packet, bool disposeOnSend = true);
        bool SendTo(NodeInfo node, byte[] buffer);
        bool SendTo(NodeInfo node, byte[] buffer, int offset, int len);
        bool SendTo(NodeInfo node, UDPChannelEnum channel, DgramOutputPacketBuffer packet, bool disposeOnSend = true);

        bool SendTo(string nodeId, ushort command, Action<DgramOutputPacketBuffer> build);
        bool SendTo(string nodeId, ushort command, UDPChannelEnum channel, Action<DgramOutputPacketBuffer> build);

        bool SendTo(NodeInfo node, ushort command, Action<DgramOutputPacketBuffer> build);
        bool SendTo(NodeInfo node, ushort command, UDPChannelEnum channel, Action<DgramOutputPacketBuffer> build);


        void SendToServer(OutputPacketBuffer packet, bool disposeOnSend = true);
        void SendToServer(ushort command, Action<OutputPacketBuffer> build);

        void Dispose();


        IEnumerable<NodeInfo> GetNodes();

        NodeInfo GetNode(string id);

        event OnNodeDelegate OnNodeConnect;

        event Func<Task> OnRoomReady;

        event OnNodeDisconnectDelegate OnNodeDisconnect;

        event OnNodeDelegate OnNodeConnectionLost;

        event OnNodeDelegate OnRecoverySession;


        Task RecoverySession(NodeInfo node);

        string LocalNodeId { get; }
    }
}
