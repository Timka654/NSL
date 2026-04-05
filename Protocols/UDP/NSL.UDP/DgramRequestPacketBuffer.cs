using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Request;
using NSL.UDP.Interface;
using System.Runtime.CompilerServices;

namespace NSL.UDP
{
    /// <summary>
    /// A request packet buffer for UDP connections.
    /// Extends <see cref="RequestPacketBuffer"/> with a <see cref="Channel"/> property
    /// and routes the send through the appropriate UDP channel.
    /// </summary>
    public class DgramRequestPacketBuffer : RequestPacketBuffer
    {
        public UDPChannelEnum Channel { get; set; } = UDPChannelEnum.ReliableOrdered;

        public DgramRequestPacketBuffer(int len = 48) : base(len) { }

        public DgramRequestPacketBuffer(System.Guid rid, int len = 48) : base(rid, len) { }

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

        public new static DgramRequestPacketBuffer Create<TEnum>(TEnum packetId, int len = 48)
            where TEnum : struct, System.Enum, System.IConvertible
            => new DgramRequestPacketBuffer(len).WithPid(packetId);

        public new static DgramRequestPacketBuffer Create(ushort packetId, int len = 48)
            => new DgramRequestPacketBuffer(len).WithPid(packetId);
    }
}
