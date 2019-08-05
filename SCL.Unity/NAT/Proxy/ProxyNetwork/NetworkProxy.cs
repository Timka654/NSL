using SCL.SocketClient;
using SCL.Unity.NAT.Proxy.ProxyNetwork.Data.Packets;
using SCL.Unity.NAT.Proxy.ProxyNetwork.Data.Packets.PacketData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCL.Unity.NAT.Proxy.ProxyNetwork
{
    public class NetworkProxy<T> : BaseNetwork<T>
        where T : NetworkProxyClient
    {
        public async Task<ProxySignInPacketResultData> GetProxy(string serverIp, int serverPort, ProxySignInPacketData data)
        {
            if (!await ConnectAsync(serverIp, serverPort))
            {
                return new ProxySignInPacketResultData() { Result = Data.Info.Enums.ProxySignInResultEnum.Ok };
            }

            return await SignIn<T>.Send(data);
        }
    }
}
