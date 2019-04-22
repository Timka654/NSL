using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer.Utils.Buffer
{
    public interface IPacket<TClient> where TClient : INetworkClient
    {
        void Receive(TClient client, InputPacketBuffer data);
    }
}
