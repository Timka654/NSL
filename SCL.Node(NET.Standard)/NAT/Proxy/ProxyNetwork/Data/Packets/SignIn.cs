using SCL.Unity.NAT.Proxy.ProxyNetwork.Data.Packets.Enums;
using SCL.Unity.NAT.Proxy.ProxyNetwork.Data.Packets.PacketData;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCL.Unity.NAT.Proxy.ProxyNetwork.Data.Packets
{
    internal class SignIn<T> : IPacketReceive<T, ProxySignInPacketResultData>
        where T : NetworkProxyClient
    {
        internal static SignIn<T> Instance { get; set; }

        public SignIn(SCL.ClientOptions<T> options) : base(options)
        {
            Instance = this;
        }

        protected override void Receive(InputPacketBuffer data)
        {
            Data = ProxySignInPacketResultData.ReadPacketData(data);
        }

        public static async Task<ProxySignInPacketResultData> Send(ProxySignInPacketData data)
        {
            OutputPacketBuffer packet = new OutputPacketBuffer
            {
                PacketId = (ushort)ServerPacketsEnum.SignIn
            };

            ProxySignInPacketData.WritePacketData(packet, data);

            return await Instance.SendWaitAsync(packet);
        }
    }
}
