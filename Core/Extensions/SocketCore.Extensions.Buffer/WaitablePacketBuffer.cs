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
        public new const int headerLenght = OutputPacketBuffer.headerLenght + 16;

        public WaitablePacketBuffer(Guid rid, int len = 48) : base(len)
        {
            WithRecvIdentity(rid);
        }

        public void WithRecvIdentity(Guid rid)
        {
            var offset = Position;

            Position = headerLenght;

            WriteGuid(rid);

            if (Position >= headerLenght)
                Position = offset;
        }

        public new static WaitablePacketBuffer Create<TEnum>(TEnum packetId, int len = 48)
            where TEnum : struct, Enum, IConvertible
            => Create(packetId, Guid.Empty, len);

        public static WaitablePacketBuffer Create<TEnum>(TEnum packetId, Guid rid, int len = 48)
            where TEnum : struct, Enum, IConvertible
            => new WaitablePacketBuffer(rid, len).WithPid(packetId);
    }
}
