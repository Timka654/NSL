using SCL.SocketClient;
using SCL.SocketClient.Utils;
using SCL.SocketClient.Utils.Buffer;
using SCL.Unity.NAT.Proxy.ProxyNetwork.Data.Packets.PacketData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCL.Unity.NAT.Proxy.ProxyNetwork.Data.Packets
{
    public class SignIn<T> : IPacketReceive<T, ProxySignInPacketResultData>
        where T : NetworkProxyClient
    {
        internal static SignIn<T> Instance { get; set; }

        public SignIn(ClientOptions<T> options) : base(options)
        {
            Instance = this;
        }

        protected override void Receive(InputPacketBuffer data)
        {
            Data = ProxySignInPacketResultData.ReadPacketData(data);
        }

        internal static async Task<ProxySignInPacketResultData> Send(ProxySignInPacketData data)
        {
            OutputPacketBuffer packet = new OutputPacketBuffer();

            packet.PacketId = 1;

            ProxySignInPacketData.WritePacketData(packet, data);

            return await Instance.SendWaitAsync(packet);
        }
    }
}
