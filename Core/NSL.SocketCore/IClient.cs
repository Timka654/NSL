using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Net;

namespace NSL.SocketCore
{
    public delegate void ReceivePacketDebugInfo<T>(T client, ushort pid, int len) where T : IClient;

    public delegate void SendPacketDebugInfo<T>(T client, ushort pid, int len, string stacktrace) where T : IClient;

    public interface IClient
    {
        CoreOptions Options { get; }

        void SendEmpty(ushort packetId);

        void Send(byte[] buf, int offset, int lenght);

        void Send(OutputPacketBuffer packet);

        void Disconnect();

        bool GetState();

        IPEndPoint GetRemotePoint();

        void ChangeUserData(INetworkClient setClient);

        object GetUserData();

        System.Net.Sockets.Socket GetSocket();

        short GetTtl();
    }

    public interface IClient<TOPacket> : IClient
        where TOPacket : OutputPacketBuffer
    {
        void Send(TOPacket packet);
    }
}
