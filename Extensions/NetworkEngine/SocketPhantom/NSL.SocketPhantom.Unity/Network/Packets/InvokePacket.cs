using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketPhantom.Unity.Network.Packets
{
    internal class InvokePacket : IPacket<PhantomSocketNetworkClient>
    {
        public override void Receive(PhantomSocketNetworkClient client, InputPacketBuffer data)
        {
            client.connection.Invoke(data);
        }
    }
}
