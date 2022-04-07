using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketPhantom.AspNetCore.Network.Packets
{
    internal class InvokePacket : IPacket<PhantomHubClientProxy>
    {
        public override void Receive(PhantomHubClientProxy client, InputPacketBuffer data)
        {
            client.Hub.Invoke(client, data);
        }
    }
}
