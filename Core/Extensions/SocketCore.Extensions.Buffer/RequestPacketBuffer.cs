using NSL.SocketCore.Utils.Buffer;
using System;

namespace NSL.SocketCore.Extensions.Buffer
{
    public class RequestPacketBuffer : OutputPacketBuffer
    {
        /// <summary>
        /// Размер шапки пакета
        /// </summary>
        public new const int DefaultHeaderLenght = OutputPacketBuffer.DefaultHeaderLenght + 16;

        /// <summary>
        /// <see cref="System.IO.MemoryStream.Position"/> without <see cref="DefaultHeaderLenght"/> header offset
        /// </summary>
        public override long DataPosition
        {
            get => base.Position - DefaultHeaderLenght;
            set => base.Position = value + DefaultHeaderLenght;
        }

        public override int DataLenght => PacketLenght - DefaultHeaderLenght;

        public RequestPacketBuffer(Guid rid, int len = 48) : base(len)
        {
            WithRecvIdentity(rid);
        }

        public void WithRecvIdentity(Guid rid)
        {
            var offset = Position;

            Position = OutputPacketBuffer.DefaultHeaderLenght;

            WriteGuid(rid);

            if (Position >= DefaultHeaderLenght)
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
    }

    [Obsolete("Replace to RequestPacketBuffer", true)]
    public class WaitablePacketBuffer : RequestPacketBuffer
    {
        public WaitablePacketBuffer(Guid rid, int len = 48) : base(rid, len)
        {
        }
    }
}
