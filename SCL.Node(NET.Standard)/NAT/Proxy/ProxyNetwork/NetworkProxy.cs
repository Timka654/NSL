using SCL.Node.Utils;
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
        public async Task<ProxySignInPacketResultData> SignProxy(string serverIp, int serverPort, ProxySignInPacketData data)
        {
            data.Proxy = false;

            return await Sign(serverIp, serverPort, data);
        }

        private async Task<ProxySignInPacketResultData> Sign(string serverIp, int serverPort, ProxySignInPacketData data)
        {
            if (!await ConnectAsync(serverIp, serverPort))
            {
                return new ProxySignInPacketResultData() { Result = Data.Info.Enums.ProxySignInResultEnum.CannotConnected };
            }

            return await SignIn<T>.Send(data);
        }

        public async Task<ProxySignInPacketResultData> SignProxyClient(string serverIp, int serverPort, ProxySignInPacketData data)
        {
            data.Proxy = true;

            return await Sign(serverIp, serverPort, data);
        }

        public void Transport(NodeOutputPacketBuffer buffer)
        {
            Data.Packets.TransportData.Send(SocketOptions.ClientData, buffer.GetBuffer());
        }

        public void Transport(byte[] buffer)
        {
            Data.Packets.TransportData.Send(SocketOptions.ClientData, buffer);
        }
    }
}
