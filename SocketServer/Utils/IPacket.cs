using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer.Utils
{
    public interface IPacket<TClient> where TClient : IServerNetworkClient
    {
        void Receive(TClient client, InputPacketBuffer data);
    }
}
