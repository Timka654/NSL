using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.SocketCore.Extensions.Buffer
{
    public class WaitablePacketBuffer : OutputPacketBuffer
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

        public WaitablePacketBuffer(Guid rid, int len = 48) : base(len)
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

        public new static WaitablePacketBuffer Create<TEnum>(TEnum packetId, int len = 48)
            where TEnum : struct, Enum, IConvertible
            => Create(packetId, Guid.Empty, len);

        public static WaitablePacketBuffer Create<TEnum>(TEnum packetId, Guid rid, int len = 48)
            where TEnum : struct, Enum, IConvertible
            => new WaitablePacketBuffer(rid, len).WithPid(packetId);
    }
}
