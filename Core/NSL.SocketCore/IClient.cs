using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
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

        [Obsolete("Use SetClientData")]
        void ChangeUserData(INetworkClient newClientData);

        /// <summary>
        /// Set old or another instance of client data to current network, and disconnect from old
        /// </summary>
        /// <param name="from">old or another instance</param>
        void SetClientData(INetworkClient from);

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
