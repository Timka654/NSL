using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace NSL.SocketCore.Utils.Buffer
{
    /// <summary>
    /// Base write buffer containing all typed Write* methods.
    /// Position starts at 0; written data begins immediately (no header region).
    /// Used directly by the pipeline engine for body-only packets and inherited by
    /// <see cref="OutputPacketBuffer"/> which prepends a 7-byte packet header region.
    /// </summary>
    public class PacketBodyBuffer : MemoryStream
    {
        protected static readonly DateTime MinDatetimeValue = new DateTime(1970, 1, 1);

        Encoding coding = Encoding.UTF8;
        byte[] _buffer = new byte[16];

        /// <summary>Position within the data region (no header offset in this base class).</summary>
        public virtual long DataPosition
        {
            get => base.Position;
            set => base.Position = value;
        }

        /// <summary>Number of bytes written into the data region.</summary>
        public virtual int DataLength => (int)base.Length;

        public PacketBodyBuffer(int initialCapacity = 32) : base()
        {
            SetLength(initialCapacity);
            base.Position = 0;
        }

        // ── Write primitives ─────────────────────────────────────────────────

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteFloat(float value)
        {
            uint TmpValue = *(uint*)&value;
            _buffer[0] = (byte)TmpValue;
            _buffer[1] = (byte)(TmpValue >> 8);
            _buffer[2] = (byte)(TmpValue >> 16);
            _buffer[3] = (byte)(TmpValue >> 24);
            Write(_buffer, 0, 4);
        }

        [Obsolete("Use \"WriteFloat\"")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteFloat32(float value) => WriteFloat(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteDouble(double value)
        {
            ulong TmpValue = *(ulong*)&value;
            _buffer[0] = (byte)TmpValue;
            _buffer[1] = (byte)(TmpValue >> 8);
            _buffer[2] = (byte)(TmpValue >> 16);
            _buffer[3] = (byte)(TmpValue >> 24);
            _buffer[4] = (byte)(TmpValue >> 32);
            _buffer[5] = (byte)(TmpValue >> 40);
            _buffer[6] = (byte)(TmpValue >> 48);
            _buffer[7] = (byte)(TmpValue >> 56);
            Write(_buffer, 0, 8);
        }

        [Obsolete("Use \"WriteDouble\"")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteFloat64(double value) => WriteDouble(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt16(short value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            Write(_buffer, 0, 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt16(ushort value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            Write(_buffer, 0, 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt32(int value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            Write(_buffer, 0, 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt32(uint value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            Write(_buffer, 0, 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt64(long value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            _buffer[4] = (byte)(value >> 32);
            _buffer[5] = (byte)(value >> 40);
            _buffer[6] = (byte)(value >> 48);
            _buffer[7] = (byte)(value >> 56);
            Write(_buffer, 0, 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt64(ulong value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            _buffer[4] = (byte)(value >> 32);
            _buffer[5] = (byte)(value >> 40);
            _buffer[6] = (byte)(value >> 48);
            _buffer[7] = (byte)(value >> 56);
            Write(_buffer, 0, 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt128(Int128 value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            _buffer[4] = (byte)(value >> 32);
            _buffer[5] = (byte)(value >> 40);
            _buffer[6] = (byte)(value >> 48);
            _buffer[7] = (byte)(value >> 56);
            _buffer[8] = (byte)(value >> 64);
            _buffer[9] = (byte)(value >> 72);
            _buffer[10] = (byte)(value >> 80);
            _buffer[11] = (byte)(value >> 88);
            _buffer[12] = (byte)(value >> 96);
            _buffer[13] = (byte)(value >> 104);
            _buffer[14] = (byte)(value >> 112);
            _buffer[15] = (byte)(value >> 120);
            Write(_buffer, 0, 16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt128(UInt128 value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            _buffer[4] = (byte)(value >> 32);
            _buffer[5] = (byte)(value >> 40);
            _buffer[6] = (byte)(value >> 48);
            _buffer[7] = (byte)(value >> 56);
            _buffer[8] = (byte)(value >> 64);
            _buffer[9] = (byte)(value >> 72);
            _buffer[10] = (byte)(value >> 80);
            _buffer[11] = (byte)(value >> 88);
            _buffer[12] = (byte)(value >> 96);
            _buffer[13] = (byte)(value >> 104);
            _buffer[14] = (byte)(value >> 112);
            _buffer[15] = (byte)(value >> 120);
            Write(_buffer, 0, 16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBool(bool value) => WriteByte((byte)(value ? 1 : 0));

        // ── String ───────────────────────────────────────────────────────────

        [Obsolete("Use \"WriteString\"")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString16(string value)
        {
            if (value == null) { WriteUInt16(ushort.MaxValue); return; }
            byte[] buf = coding.GetBytes(value);
            WriteUInt16((ushort)buf.Length);
            if (buf.Length > 0) Write(buf, 0, buf.Length);
        }

        [Obsolete("Use \"WriteString\"")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString32(string value)
        {
            if (value == null) { WriteUInt32(uint.MaxValue); return; }
            byte[] buf = coding.GetBytes(value);
            WriteUInt32((uint)buf.Length);
            if (buf.Length > 0) Write(buf);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString(string value)
        {
            if (value == null) { Write7BitEncodedUInt32(uint.MaxValue); return; }
            byte[] buf = coding.GetBytes(value);
            Write7BitEncodedUInt32((uint)buf.Length);
            if (buf.Length > 0) Write(buf);
        }

        // ── Collections / Nullable ───────────────────────────────────────────

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteCollection<T>(IEnumerable<T> arr, Action<PacketBodyBuffer, T> writeAction)
        {
            if (arr == default) { Write7BitEncodedUInt32(uint.MaxValue); return; }
            Write7BitEncodedUInt32((uint)arr.Count());
            foreach (var item in arr) writeAction(this, item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteCollection<T>(IEnumerable<T> arr, Action<T> writeAction)
        {
            if (arr == default) { Write7BitEncodedUInt32(uint.MaxValue); return; }
            Write7BitEncodedUInt32((uint)arr.Count());
            foreach (var item in arr) writeAction(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNullable<T>(Nullable<T> value, Action hasValueAction)
            where T : struct
        {
            if (value.HasValue) { WriteBool(true); hasValueAction(); return; }
            WriteBool(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNullable<T>(T? value, Action<PacketBodyBuffer, T> writeAction)
            where T : struct
        {
            if (value.HasValue) { WriteBool(true); writeAction(this, value.Value); return; }
            WriteBool(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNullableClass<T>(T value, Action hasValueAction)
            where T : class
        {
            if (value != null) { WriteBool(true); hasValueAction(); return; }
            WriteBool(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNullableClass<T>(T value, Action<PacketBodyBuffer, T> writeAction)
            where T : class
        {
            if (value != null) { WriteBool(true); writeAction(this, value); return; }
            WriteBool(false);
        }

        // ── Typed extras ─────────────────────────────────────────────────────

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDateTime(DateTime value) => WriteInt64((value - MinDatetimeValue).Ticks);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteTimeSpan(TimeSpan value) => WriteInt64(value.Ticks);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteGuid(Guid value) { var arr = value.ToByteArray(); Write(arr, 0, 16); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] buf) => Write(buf, 0, buf.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByteArray(byte[] buf)
        {
            Write7BitEncodedUInt32((uint)buf.Length);
            Write(buf, 0, buf.Length);
        }

        // ── Internal helpers ─────────────────────────────────────────────────

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Write7BitEncodedUInt32(uint value)
        {
            uint v = value;
            while (v >= 0x80) { WriteByte((byte)(v | 0x80)); v >>= 7; }
            WriteByte((byte)v);
        }
    }
}

