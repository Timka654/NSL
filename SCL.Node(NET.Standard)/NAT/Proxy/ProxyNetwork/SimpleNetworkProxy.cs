using SCL.Unity.NAT.Proxy.ProxyNetwork.Data.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCL.Unity.NAT.Proxy.ProxyNetwork
{
    public class SimpleNetworkProxy : NetworkProxy<NetworkProxyClient, ClientOptions<NetworkProxyClient>>
    {
        public SimpleNetworkProxy(ClientOptions<NetworkProxyClient> options) : base(options)
        {
        }

        protected virtual void LoadPackets()
        {
            clientOptions.AddPacket((ushort)Data.Packets.Enums.ClientPacketsEnum.SignInResult, new SignIn<NetworkProxyClient>(clientOptions));
            clientOptions.AddPacket(1, new SignIn<NetworkProxyClient>(clientOptions));
        }
    }
}
