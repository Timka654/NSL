using SocketServer.Utils;
using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketServer
{

    public interface IClient
    {
#if DEBUG
        void Send(OutputPacketBuffer packet, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0);
#else
        void Send(OutputPacketBuffer packet);
#endif

#if DEBUG
        void SendSerialize(ushort packetId, object obj, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0);
#else
        void SendSerialize(ushort packetId,object obj);
#endif

        void Send(byte[] buf, int offset, int lenght);

        void Disconnect();

        bool GetState();

        IPEndPoint GetRemovePoint();

        void ChangeUserData(INetworkClient data);

        object GetUserData();

        Socket GetSocket();
    }
}
