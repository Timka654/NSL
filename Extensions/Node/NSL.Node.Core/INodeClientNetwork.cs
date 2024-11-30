using NSL.SocketCore.Utils;
using NSL.UDP;
using NSL.UDP.Enums;
using System;

namespace NSL.Node.RoomServer.Shared.Client.Core
{
    public interface INodeClientNetwork : IDisposable
    {
        void Send(ushort code,Action<DgramOutputPacketBuffer> build,  UDPChannelEnum channel = UDPChannelEnum.ReliableOrdered);

        void Send(Action<DgramOutputPacketBuffer> build, UDPChannelEnum channel = UDPChannelEnum.ReliableOrdered);
        
        void Send(ushort code, Action<DgramOutputPacketBuffer> build);

        //void Send(byte[] packetBody);

        //void Send(byte[] packetBody, int offset, int len);

        void Send(Action<DgramOutputPacketBuffer> build);

        void Send(DgramOutputPacketBuffer packet, UDPChannelEnum channel = UDPChannelEnum.ReliableOrdered, bool disposeOnSend = true);
        void Send(DgramOutputPacketBuffer packet, bool disposeOnSend = true);

        ClientObjectBag ObjectBag { get; }
        string NodeId { get; }
        bool IsLocalNode { get; }

        void InitializeObjectBag();

        void SetObjectOwner(INodeOwneredObject _object);

        void Disconnect();

        INodeNetworkClient UDPClient { get; }
    }
}
