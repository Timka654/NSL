using NSL.Extensions.NAT.Proxy.Data.Enums;
using SocketCore.Utils.Buffer;

namespace NSL.Extensions.NAT.Proxy.Data.Packets.PacketData
{
    public class ProxySignInPacketResultData
    {
        public ProxySignInResultEnum Result { get; set; }

        public string ProxyIp { get; set; }

        public static ProxySignInPacketResultData ReadPacketData(InputPacketBuffer data)
        {
            ProxySignInPacketResultData result = new ProxySignInPacketResultData();

            result.Result = (ProxySignInResultEnum)data.ReadByte();

            if (result.Result == ProxySignInResultEnum.Ok)
                result.ProxyIp = data.ReadString16();

            return result;
        }
    }
}
