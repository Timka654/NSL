using SCL.Unity.NAT.Proxy.ProxyNetwork.Data.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCL.Unity.NAT.Proxy.ProxyNetwork
{
    public class SimpleNetworkProxy : NetworkProxy<NetworkProxyClient>
    {
        protected override void LoadPackets()
        {
            SocketOptions.AddPacket(1, new SignIn<NetworkProxyClient>(SocketOptions));
        }
    }
}
