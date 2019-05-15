using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SocketServer.Utils.Buffer;

namespace SocketServer.Utils.SystemPackets
{
    public class Version<T> :IPacket<T> where T: INetworkClient
    {
        public void Receive(T client, InputPacketBuffer data)
        {
            client.Version = data.ReadInt64();
        }
    }
}
