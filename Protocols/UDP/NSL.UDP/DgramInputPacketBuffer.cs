﻿using NSL.SocketCore.Utils.Buffer;
using NSL.UDP.Enums;

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
