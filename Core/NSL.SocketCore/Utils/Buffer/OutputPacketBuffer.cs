using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace NSL.SocketCore.Utils.Buffer
{
    public class OutputPacketBuffer<TPID> : OutputPacketBuffer
        where TPID : struct, Enum, IConvertible
    {
        public OutputPacketBuffer(TPID packetId, int len = 32) : base(len)
        {
            PacketId = packetId.ToUInt16(null);
        }
    }

    public class OutputPacketBuffer : PacketBodyBuffer
    {
        /// <summary>
        /// Min packet identity used by NSL library for implementing inner logic
        /// default value = 65335 is (ushort.MaxValue - 235), all values more or equals of this value - can be implemented in NSL
        /// </summary>
        public const ushort NSLSystemMinPID = ushort.MaxValue - 235;

        /// <summary>
        /// Detect if this pid used in NSL
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSystemPID(ushort pid)
            => !(pid < NSLSystemMinPID);

        /// <summary>
        /// <see cref="System.IO.MemoryStream.Position"/> without <see cref="DefaultHeaderLength"/> bytes header offset
        /// </summary>
        public override long DataPosition
        {
            get => base.Position - DefaultHeaderLength;
            set => base.Position = value + DefaultHeaderLength;
        }

        /// <summary>
        /// Full packet len
        /// </summary>
        public int PacketLength => (int)base.Length;

        /// <summary>
        /// Data part packet len
        /// </summary>
        public override int DataLength => PacketLength - DefaultHeaderLength;

        /// <summary>
        /// Packet identity
        /// </summary>
        public ushort PacketId { get; set; }

        /// <summary>
        /// Hash for packet header
        /// </summary>
        public bool AppendHash { get; set; }

        /// <summary>
        /// Default header part packet len
        /// </summary>
        public const int DefaultHeaderLength = 7;

        public static OutputPacketBuffer Create(ushort packetId, int len = 32)
        {
            return new OutputPacketBuffer(len) { PacketId = packetId };
        }

        public static OutputPacketBuffer Create<TEnum>(TEnum packetId, int len = 32)
            where TEnum : struct, Enum, IConvertible
        {
            return new OutputPacketBuffer(len).WithPid(packetId);
        }

        /// <summary>
        /// </summary>
        /// <param name="len">initial buffer len</param>
        public OutputPacketBuffer(int len = 32) : base(len + DefaultHeaderLength)
        {
            DataPosition = 0;
        }

        /// <summary>
        /// Сборка пакета в финальный вид перед отправкой
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] CompilePacket(bool dispose = true)
        {
            base.Seek(0, SeekOrigin.Begin);

            WriteInt32(PacketLength);
            WriteUInt16(PacketId);

            if (AppendHash)
                WriteByte((byte)((PacketLength + PacketId) % 255));

            var arr = base.ToArray();

            if (dispose)
                base.Dispose();

            return arr;
        }

        /// <summary>
        /// Source - https://github.com/microsoft/referencesource/blob/master/mscorlib/system/io/binarywriter.cs
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Send(IClient client, bool disposeOnSend)
            => client.Send(CompilePacket(disposeOnSend));
    }

    public static class _Extensions
    {
        public static TBuffer WithPid<TBuffer, TEnum>(this TBuffer buffer, TEnum packetId)
            where TBuffer : OutputPacketBuffer
            where TEnum : struct, Enum, IConvertible
        => buffer.WithPid(packetId.ToUInt16(null));

        public static TBuffer WithPid<TBuffer>(this TBuffer buffer, ushort packetId)
            where TBuffer : OutputPacketBuffer
        {
            buffer.PacketId = packetId;
            return buffer;
        }
    }
}
