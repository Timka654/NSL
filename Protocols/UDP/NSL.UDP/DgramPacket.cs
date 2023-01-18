using NSL.SocketCore;
using NSL.SocketCore.Utils.Buffer;
using NSL.UDP.Enums;
using NSL.UDP.Interface;
using System;
using System.Runtime.CompilerServices;

namespace NSL.UDP
{
    public class DgramPacket : OutputPacketBuffer
    {
        public UDPChannelEnum Channel { get; set; } = UDPChannelEnum.ReliableOrdered;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Send(IClient client, bool disposeOnSend)
        {
            if (client is not IUDPClient c)
                throw new Exception($"{nameof(DgramPacket)} cannot be send with no {nameof(IClient<DgramPacket>)}");

            AppendHash = true;

            var buffer = CompilePacket(disposeOnSend);

            c.Send(Channel, buffer);

        }
    }
}
