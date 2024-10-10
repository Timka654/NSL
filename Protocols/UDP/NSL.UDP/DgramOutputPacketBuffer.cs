using NSL.SocketCore;
using NSL.SocketCore.Utils.Buffer;
using NSL.UDP.Enums;
using NSL.UDP.Interface;
using NSL.UDP.Packet;
using System;
using System.Runtime.CompilerServices;

namespace NSL.UDP
{
    public class DgramOutputPacketBuffer : OutputPacketBuffer
    {
        public UDPChannelEnum Channel { get; set; } = UDPChannelEnum.ReliableOrdered;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UDPChannelEnum ReadChannel(Span<byte> buffer) => UDPPacket.ReadChannel(buffer);


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

            base.Send(client, disposeOnSend);
        }

        public new static DgramOutputPacketBuffer Create(ushort packetId, int len = 32)
        {
            return new DgramOutputPacketBuffer(len) { PacketId = packetId };
        }

        public new static DgramOutputPacketBuffer Create<TEnum>(TEnum packetId, int len = 32)
            where TEnum : struct, Enum, IConvertible
        {
            return new DgramOutputPacketBuffer(len).WithPid(packetId);
        }

        public DgramOutputPacketBuffer(int len = 32) : base(len) { }
    }

    [Obsolete("Replace to DgramOutputPacketBuffer", true)]
    public class DgramPacket : DgramOutputPacketBuffer { }
}
