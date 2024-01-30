using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Net;

namespace NSL.SocketCore
{
    public delegate void ReceivePacketDebugInfo<T>(T client, ushort pid, int len) where T : IClient;

    public delegate void SendPacketDebugInfo<T>(T client, ushort pid, int len, string stackTrace) where T : IClient;

    public interface IClient : INetworkNode
    {
        CoreOptions Options { get; }

        bool GetState();

        IPEndPoint GetRemotePoint();

        void ChangeUserData(INetworkClient newClientData);

        object GetUserData();

        System.Net.Sockets.Socket GetSocket();

        short GetTtl();
    }

    public interface IClient<TOPacket> : IClient
        where TOPacket : OutputPacketBuffer
    {
        void Send(TOPacket packet, bool disposeOnSend = true);
    }
}
