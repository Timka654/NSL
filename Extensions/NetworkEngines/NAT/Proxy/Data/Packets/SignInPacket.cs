using SCL.Node.NAT.Proxy.Data.Packets.Enums;
using SCL.Node.NAT.Proxy.Data.Packets.PacketData;
using SocketClient.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCL.Node.NAT.Proxy.Data.Packets
{
    internal class SignInPacket : IPacketReceive<NetworkProxyClient, ProxySignInPacketResultData>
    {
        public SignInPacket(SocketClient.ClientOptions<NetworkProxyClient> options) : base(options) { }

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
