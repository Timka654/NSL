using NSL.SocketCore.Utils.Buffer;
using NSL.UDP.Channels;
using NSL.UDP.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Network.UDP.NSL.UDP
{
    internal class DgramInputPacketBuffer : InputPacketBuffer
    {
        public UDPChannelEnum SourceChannel { get; }
        public DgramInputPacketBuffer(UDPChannelEnum channel)
        {
            SourceChannel = channel;
        }

        public DgramInputPacketBuffer(byte[] buf, UDPChannelEnum channel, bool checkHash = false) : base(buf, checkHash)
        {
            SourceChannel = channel;
        }
    }
}
