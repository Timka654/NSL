using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Buffers.Binary;
using System.Text;

namespace NSL.SocketCore.Utils.Buffer
{
    /// <summary>
    /// Base read-only packet body reader containing all typed Read* methods.
    /// Does not carry any header metadata (no PacketId, no DefaultHeaderLength).
    /// Use directly for pipeline body-only contexts; or extended by <see cref="InputPacketBuffer"/>
    /// which adds header metadata.
    /// </summary>
    public class PacketBodyReader : IDisposable
    {
        protected static readonly DateTime MinDatetimeValue = new DateTime(1970, 1, 1);

        Encoding coding = Encoding.UTF8;

        protected byte[] data;
        protected int dataLength;

        public byte[] Data => data;

        public int DataPosition { get; set; }

        public virtual int DataLength => dataLength;

        public bool AsyncDisposing { get; set; } = true;

        bool manualDisposing = false;

        public bool ManualDisposing
        {
            get => manualDisposing;
            set
            {
                if (manualDisposing && !value)
                    throw new InvalidOperationException($"[Security] Cannot change {nameof(ManualDisposing)} back");
                manualDisposing = value;
            }
        }

        // ── Read primitives ──────────────────────────────────────────────────

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloat()
        {
            DataPosition += 4;
            return Int32BitsToSingle(ReadInt32());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble(ReadInt64());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16()
        {
            DataPosition += 2;
            return BinaryPrimitives.ReadInt16LittleEndian(data.AsSpan(DataPosition - 2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16()
        {
            DataPosition += 2;
            return BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(DataPosition - 2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32()
        {
            DataPosition += 4;
            return BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(DataPosition - 4));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32()
        {
            DataPosition += 4;
            return BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(DataPosition - 4));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64()
        {
            DataPosition += 8;
            return BinaryPrimitives.ReadInt64LittleEndian(data.AsSpan(DataPosition - 8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Int128 ReadInt128()
        {
            DataPosition += 16;
            return MemoryMarshal.Read<Int128>(data.AsSpan(DataPosition - 16));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            DataPosition += 8;
            return BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(DataPosition - 8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UInt128 ReadUInt128()
        {
            DataPosition += 16;
            return MemoryMarshal.Read<UInt128>(data.AsSpan(DataPosition - 16));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            DataPosition += 1;
            return data[DataPosition - 1];
        }

        // ── String ───────────────────────────────────────────────────────────

        [Obsolete("Use \"ReadString\"")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString16()
        {
            var len = ReadUInt16();
            if (len == ushort.MaxValue) return null;
            if (len == 0) return String.Empty;
            return ReadString(len);
        }

        [Obsolete("Use \"ReadString\"")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString32()
        {
            var len = ReadUInt32();
            if (len == uint.MaxValue) return null;
            if (len == 0) return String.Empty;
            return ReadString(len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString()
        {
            var len = Read7BitEncodedUInt();
            if (len == uint.MaxValue) return null;
            if (len == 0) return String.Empty;
            return ReadString(len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ReadString(uint len)
        {
            if (len < 1) throw new ArgumentOutOfRangeException(nameof(len));
            return coding.GetString(Read((int)len).ToArray());
        }

        // ── Nullable / Collection ────────────────────────────────────────────

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? ReadNullable<T>(Func<T> hasValueAction)
            where T : struct
        {
            if (ReadBool()) return hasValueAction();
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? ReadNullable<T>(Func<PacketBodyReader, T> hasValueAction)
            where T : struct
        {
            if (ReadBool()) return hasValueAction(this);
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadNullableClass<T>(Func<T> hasValueAction)
            where T : class
        {
            if (ReadBool()) return hasValueAction();
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadNullableClass<T>(Func<PacketBodyReader, T> hasValueAction)
            where T : class
        {
            if (ReadBool()) return hasValueAction(this);
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> ReadCollection<T>(Func<PacketBodyReader, T> readAction)
        {
            var len = Read7BitEncodedUInt();
            if (len == uint.MaxValue) return default;
            var result = new List<T>((int)len);
            for (int i = 0; i < len; i++) result.Add(readAction(this));
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> ReadCollection<T>(Func<T> readAction)
        {
            var len = Read7BitEncodedUInt();
            if (len == uint.MaxValue) return default;
            var result = new List<T>((int)len);
            for (int i = 0; i < len; i++) result.Add(readAction());
            return result;
        }

        // ── Bytes / Bool ─────────────────────────────────────────────────────

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> ReadByteArray()
        {
            var len = Read7BitEncodedUInt();
            return Read((int)len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> Read(int len)
        {
            if (DataLength - DataPosition < len)
                throw new OutOfMemoryException();
            DataPosition += len;
            return data.AsSpan(DataPosition - len, len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBool() => ReadByte() == 1;

        // ── DateTime / TimeSpan / Guid ───────────────────────────────────────

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime ReadDateTime() => MinDatetimeValue.AddTicks(ReadInt64());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeSpan ReadTimeSpan() => TimeSpan.FromTicks(ReadInt64());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid ReadGuid() => new Guid(Read(16).ToArray());

        // ── Helpers ──────────────────────────────────────────────────────────

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe float Int32BitsToSingle(int value) => *((float*)&value);

        protected internal uint Read7BitEncodedUInt()
        {
            int count = 0;
            int shift = 0;
            byte b;
            do
            {
                if (shift == 5 * 7) throw new FormatException();
                b = ReadByte();
                count |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);
            return (uint)count;
        }

        public int Seek(int len, SeekOrigin seek)
        {
            if (seek == SeekOrigin.Begin)
                DataPosition = len;
            else if (seek == SeekOrigin.Current)
                DataPosition += len;
            else if (seek == SeekOrigin.End)
                DataPosition = DataLength + len;

            if (DataPosition < 0) DataPosition = 0;
            return DataPosition;
        }

        // ── Data management ──────────────────────────────────────────────────

        public void SetData(byte[] data, bool replace = false)
        {
            if (!replace && this.data != null)
                throw new Exception("Data already set");
            this.data = data;
            this.dataLength = data.Length;
        }

        public void SetData(byte[] data, int length, bool replace = false)
        {
            if (!replace && this.data != null)
                throw new Exception("Data already set");
            this.data = data;
            this.dataLength = length;
        }

        public void Dispose()
        {
            OnDispose(this);
            data = null;
        }

        public event Action<PacketBodyReader> OnDispose = _ => { };
    }
}
