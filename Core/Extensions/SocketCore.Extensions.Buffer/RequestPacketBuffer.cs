using NSL.SocketCore.Utils.Buffer;
using System;

namespace NSL.SocketCore.Extensions.Buffer
{
    public class RequestPacketBuffer : OutputPacketBuffer
    {
        /// <summary>
        /// Размер шапки пакета
        /// </summary>
        public new const int DefaultHeaderLength = OutputPacketBuffer.DefaultHeaderLength + 16;

        /// <summary>
        /// <see cref="System.IO.MemoryStream.Position"/> without <see cref="DefaultHeaderLength"/> header offset
        /// </summary>
        public override long DataPosition
        {
            get => base.Position - DefaultHeaderLength;
            set => base.Position = value + DefaultHeaderLength;
        }

        public override int DataLength => PacketLength - DefaultHeaderLength;

        public RequestPacketBuffer(int len = 48) : this(Guid.Empty, len) { }

        public RequestPacketBuffer(Guid rid, int len = 48) : base(len)
        {
            WithRecvIdentity(rid);
        }

        public void WithRecvIdentity(Guid rid)
        {
            var offset = Position;

            Position = OutputPacketBuffer.DefaultHeaderLength;

            WriteGuid(rid);

            if (Position >= DefaultHeaderLength)
                Position = offset;
            else
                DataPosition = 0;
        }

        public new static RequestPacketBuffer Create<TEnum>(TEnum packetId, int len = 48)
            where TEnum : struct, Enum, IConvertible
            => Create(packetId, Guid.Empty, len);

        public static RequestPacketBuffer Create<TEnum>(TEnum packetId, Guid rid, int len = 48)
            where TEnum : struct, Enum, IConvertible
            => new RequestPacketBuffer(rid, len).WithPid(packetId);

        public static RequestPacketBuffer Create(ushort packetId, int len = 48)
            => new RequestPacketBuffer(len).WithPid(packetId);

        public static RequestPacketBuffer Create()
            => new RequestPacketBuffer();
    }

    [Obsolete("Replace to RequestPacketBuffer", true)]
    public class WaitablePacketBuffer : RequestPacketBuffer
    {
        public WaitablePacketBuffer(Guid rid, int len = 48) : base(rid, len)
        {
        }
    }
}
