using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Net;

namespace SocketCore
{
    public delegate void ReceivePacketDebugInfo<T>(T client, ushort pid, int len) where T : IClient;
#if DEBUG
    public delegate void SendPacketDebugInfo<T>(T client, ushort pid, int len, string stacktrace) where T : IClient;
#else
    public delegate void SendPacketDebugInfo<T>(T client, ushort pid, int len) where T : IClient;
#endif
    public interface IClient
    {
        void Send(OutputPacketBuffer packet);

        void SendEmpty(ushort packetId);


        void Send(byte[] buf, int offset, int lenght);

        void Disconnect();

        bool GetState();

        IPEndPoint GetRemovePoint();

        void ChangeUserData(INetworkClient data);

        object GetUserData();

        System.Net.Sockets.Socket GetSocket();
    }
}
