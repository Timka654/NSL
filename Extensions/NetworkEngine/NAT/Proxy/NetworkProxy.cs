using NSL.Extensions.NAT.Proxy.Data.Packets;
using NSL.Extensions.NAT.Proxy.Data.Packets.PacketData;
using System.Threading.Tasks;
using NSL.Extensions.NAT.Proxy.Data.Enums;
using NSL.Extensions.NAT.Proxy.Data.Packets.Enums;
using SocketCore.Utils.Buffer;
using SocketClient;
using NSL.TCP.Client;

namespace NSL.Extensions.NAT.Proxy
{
    public class NetworkProxy
    {
        SignInPacket signInPacketInstanse;
        TransportDataPacket transportDataPacketInstanse;

        public event TransportDataPacket.OnReceiveEventHandle ReceiveData
        {
            add => transportDataPacketInstanse.OnReceiveEvent += value;
            remove => transportDataPacketInstanse.OnReceiveEvent -= value;
        }

        ClientOptions<NetworkProxyClient> options;

        TCPNetworkClient<NetworkProxyClient, ClientOptions<NetworkProxyClient>> client;

        public NetworkProxy()
        {
            options = new ClientOptions<NetworkProxyClient>();

            options.AddPacket((ushort)ClientPacketsEnum.SignInResult, signInPacketInstanse = new SignInPacket(options));
            options.AddPacket((ushort)ClientPacketsEnum.SignInResult, transportDataPacketInstanse = new TransportDataPacket(options));

            client = new TCPNetworkClient<NetworkProxyClient, ClientOptions<NetworkProxyClient>>(options);
        }

        public async Task<ProxySignInPacketResultData> TrySignIn(
            string serverIp, 
            int serverPort, 
            ProxySignInPacketData data, 
            int connectionTimeout = TCPNetworkClient<NetworkProxyClient, ClientOptions<NetworkProxyClient>>.DefaultConnectionTimeout)
        {
            if (!await client.ConnectAsync(serverIp, serverPort, connectionTimeout))
                return new ProxySignInPacketResultData() { Result = ProxySignInResultEnum.CannotConnected };

            return await signInPacketInstanse.Send(data);
        }

        public void Transport(OutputPacketBuffer buffer) => Transport(buffer.GetBuffer());

        public void Transport(byte[] buffer) => transportDataPacketInstanse.Send(buffer);
    }
}
