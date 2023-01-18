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
            if (client is IUDPClient c)
            {
                AppendHash = true;

                var buffer = CompilePacket(disposeOnSend);

                c.Send(Channel, buffer);

                return;
            }
            
            throw new Exception($"{nameof(DgramPacket)} cannot be send with no {nameof(IClient<DgramPacket>)}");
        }
    }
}
