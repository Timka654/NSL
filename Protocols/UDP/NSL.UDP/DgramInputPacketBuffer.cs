using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.UDP
{
    public class DgramInputPacketBuffer : InputPacketBuffer
    {
        public UDPChannelEnum SourceChannel { get; }
        //public DgramInputPacketBuffer(UDPChannelEnum channel)
        //{
        //    SourceChannel = channel;
        //}

        public DgramInputPacketBuffer(byte[] buf, UDPChannelEnum channel, bool checkHash = false) : base(buf, checkHash)
        {
            SourceChannel = channel;
        }
    }
}
