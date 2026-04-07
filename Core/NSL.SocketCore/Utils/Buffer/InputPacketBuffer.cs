using NSL.SocketCore.Utils.Exceptions;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NSL.SocketCore.Utils.Buffer
{
    public class InputPacketBuffer : PacketBodyReader
    {
        /// <summary>
        /// Default header part packet len
        /// </summary>
        public const int DefaultHeaderLength = 7;

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

        readonly int packetLength;

        /// <summary>Full packet len (header + body).</summary>
        public int PacketLength => packetLength;

        public override int DataLength => PacketLength - DefaultHeaderLength;

        /// <summary>Packet identity.</summary>
        public ushort PacketId { get; set; }

        public InputPacketBuffer(int packetLength, ushort packetId)
        {
            this.packetLength = packetLength;
            this.PacketId = packetId;
        }

        /// <summary>
        /// Constructs from a raw 7-byte header buffer.
        /// </summary>
        /// <param name="buf">header buffer</param>
        /// <param name="checkHash">validate packet header hash</param>
        public InputPacketBuffer(Span<byte> buf, bool checkHash = false)
        {
            packetLength = BinaryPrimitives.ReadInt32LittleEndian(buf);
            PacketId = BinaryPrimitives.ReadUInt16LittleEndian(buf.Slice(4));

            if (checkHash)
            {
                if (buf[6] != ((packetLength) + PacketId) % 255)
                    throw new InvalidPacketHashException();
            }

            DataPosition = 0;
        }

        // ── Typed-self overloads for backward compat with generators ─────────
        // Shadow the PacketBodyReader Func<PacketBodyReader,T> overloads so that
        // existing code compiled against InputPacketBuffer continues receiving the
        // concrete type without casts.

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new T? ReadNullable<T>(Func<InputPacketBuffer, T> hasValueAction)
            where T : struct
        {
            if (ReadBool()) return hasValueAction(this);
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new T ReadNullableClass<T>(Func<InputPacketBuffer, T> hasValueAction)
            where T : class
        {
            if (ReadBool()) return hasValueAction(this);
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new IEnumerable<T> ReadCollection<T>(Func<InputPacketBuffer, T> readAction)
        {
            var len = Read7BitEncodedUInt();
            if (len == uint.MaxValue) return default;
            var result = new List<T>((int)len);
            for (int i = 0; i < len; i++) result.Add(readAction(this));
            return result;
        }

#if NSL_LIBRARY
        /// <summary>
        /// Creates a pooled <see cref="InputPacketBuffer"/> wrapping the given body memory.
        /// The buffer's <c>DataLength</c> equals <paramref name="body"/>.Length.
        /// The caller is responsible for disposing the returned buffer (pool return is hooked on Dispose).
        /// </summary>
        public static InputPacketBuffer FromBody(System.Memory<byte> body, ushort packetId)
        {
            var pool = System.Buffers.ArrayPool<byte>.Shared;
            int len = body.Length;
            var data = pool.Rent(len);
            if (len > 0) body.CopyTo(data);
            var buf = new InputPacketBuffer(len + DefaultHeaderLength, packetId);
            buf.SetData(data);
            buf.OnDispose += b => pool.Return(b.Data);
            return buf;
        }
#endif
    }
}
