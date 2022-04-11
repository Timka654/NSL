using NSL.Extensions.NAT.Proxy.Data.Packets.Enums;
using NSL.Extensions.NAT.Proxy.Data.Packets.PacketData;
using NSL.SocketClient;
using NSL.SocketClient.Utils;
using SocketCore.Utils.Buffer;
using System.Threading.Tasks;

namespace NSL.Extensions.NAT.Proxy.Data.Packets
{
    internal class SignInPacket : IPacketReceive<NetworkProxyClient, ProxySignInPacketResultData>
    {
        public SignInPacket(ClientOptions<NetworkProxyClient> options) : base(options) { }

        protected override void Receive(InputPacketBuffer data)
        {
            Data = ProxySignInPacketResultData.ReadPacketData(data);
        }

        public async Task<ProxySignInPacketResultData> Send(ProxySignInPacketData data)
        {
            OutputPacketBuffer packet = new OutputPacketBuffer
            {
                PacketId = (ushort)ServerPacketsEnum.SignIn
            };

            ProxySignInPacketData.WritePacketData(packet, data);

            return await SendWaitAsync(packet);
        }
    }
}
