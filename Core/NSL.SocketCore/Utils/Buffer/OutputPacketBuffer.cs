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

    public class OutputPacketBuffer : MemoryStream
    {
        private static readonly DateTime MinDatetimeValue = new DateTime(1970, 1, 1);

        /// <summary>
        /// Min packet identity used by NSL library for implementing inner logic
        /// default value = 65335 is (ushort.MaxValue - 235), all values more or equals of this value - can be implemented in NSL
        /// </summary>
        public const ushort NSLSystemMinPID = ushort.MaxValue - 235;

        /// <summary>
        /// Detect if this pid used in NSL
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSystemPID(ushort pid)
            => !(pid < NSLSystemMinPID);

        /// <summary>
        /// Current encoding for type `String`
        /// </summary>
        Encoding coding = Encoding.UTF8;

        /// <summary>
        /// Temp buffer for convert data
        /// </summary>
        byte[] _buffer = new byte[16];

        /// <summary>
        /// <see cref="System.IO.MemoryStream.Position"/> without <see cref="DefaultHeaderLength"/> bytes header offset
        /// </summary>
        public virtual long DataPosition
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
        public virtual int DataLength => (int)base.Length - DefaultHeaderLength;

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
        /// 
        /// </summary>
        /// <param name="len">initial buffer len</param>
        public OutputPacketBuffer(int len = 32) : base()
        {
            //начальный размер буфера необходим для оптимизации пакетов, в случае если пакет имеет заведомо известный размер, его не придется увеличивать что будет экономить время
            //инициализация буфера
            //установка размера буфера
            SetLength(len + DefaultHeaderLength);

            DataPosition = 0;
        }

        /// <summary>
        /// Write float value (4 bytes)
        /// </summary>
        /// <param name="value">value</param>
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
        /// <summary>
        /// Write float value (4 bytes)
        /// </summary>
        /// <param name="value">value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteFloat32(float value)
            => WriteFloat(value);

        /// <summary>
        /// Write double value (8 bytes)
        /// </summary>
        /// <param name="value">value</param>
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
        /// <summary>
        /// Write double value (8 bytes)
        /// </summary>
        /// <param name="value">value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteFloat64(double value)
            => WriteDouble(value);

        /// <summary>
        /// Write short value (int16, 2 bytes)
        /// </summary>
        /// <param name="value">value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt16(short value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            Write(_buffer, 0, 2);
        }

        /// <summary>
        /// Write ushort value (uint16, 2 bytes)
        /// </summary>
        /// <param name="value">value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt16(ushort value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            Write(_buffer, 0, 2);
        }

        /// <summary>
        /// Write int value (int32, 4 bytes)
        /// </summary>
        /// <param name="value">value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt32(int value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            Write(_buffer, 0, 4);
        }

        /// <summary>
        /// Write uint value (uint32, 4 bytes)
        /// </summary>
        /// <param name="value">value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt32(uint value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            Write(_buffer, 0, 4);
        }

        /// <summary>
        /// Write long value (int64, 8 bytes)
        /// </summary>
        /// <param name="value">value</param>
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

        /// <summary>
        /// Write ulong value (uint64, 8 bytes)
        /// </summary>
        /// <param name="value">value</param>
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

        /// <summary>
        /// Write bool value (1 bytes)
        /// </summary>
        /// <param name="value">value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBool(bool value)
        {
            WriteByte((byte)(value ? 1 : 0));
        }

        [Obsolete("Use \"WriteString\"")]
        /// <summary>
        /// Write string value, with ushort(2 bytes) header, max - 32k len
        /// </summary>
        /// <param name="value">value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString16(string value)
        {
            if (value == null)
            {
                WriteUInt16(ushort.MaxValue);
                return;
            }
            byte[] buf = coding.GetBytes(value);

            WriteUInt16((ushort)buf.Length);

            if (buf.Length > 0)
                Write(buf, 0, buf.Length);
        }

        [Obsolete("Use \"WriteString\"")]
        /// <summary>
        /// Write string value, with uint(4 bytes) header, max - 2.14kkk len
        /// </summary>
        /// <param name="value">value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString32(string value)
        {
            if (value == null)
            {
                WriteUInt32(uint.MaxValue);
                return;
            }

            byte[] buf = coding.GetBytes(value);

            WriteUInt32((uint)buf.Length);
            if (buf.Length > 0)
                Write(buf);
        }

        /// <summary>
        /// Write string value, with 1-4 bytes len header, max - 2.14kkk len
        /// </summary>
        /// <param name="value">value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString(string value)
        {
            if (value == null)
            {
                Write7BitEncodedUInt32(uint.MaxValue);
                return;
            }

            byte[] buf = coding.GetBytes(value);

            Write7BitEncodedUInt32((uint)buf.Length);

            if (buf.Length > 0)
                Write(buf);
        }

        /// <summary>
        /// Write collection with len header(Int32, 4 bytes)
        /// </summary>
        /// <param name="value">value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteCollection<T>(IEnumerable<T> arr, Action<OutputPacketBuffer, T> writeAction)
        {
            if (arr == default)
            {
                Write7BitEncodedUInt32(uint.MaxValue);
                return;
            }

            Write7BitEncodedUInt32((uint)arr.Count());

            foreach (var item in arr)
            {
                writeAction(this, item);
            }
        }

        /// <summary>
        /// Write collection with int32 header len
        /// </summary>
        /// <param name="value">value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteCollection<T>(IEnumerable<T> arr, Action<T> writeAction)
        {
            if (arr == default)
            {
                Write7BitEncodedUInt32(uint.MaxValue);
                return;
            }

            Write7BitEncodedUInt32((uint)arr.Count());

            foreach (var item in arr)
            {
                writeAction(item);
            }
        }

        /// <summary>
        /// Write nullable type value with bool(hasValue) header
        /// </summary>
        /// <param name="value">value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNullable<T>(Nullable<T> value, Action hasValueAction)
            where T : struct
        {
            if (value.HasValue)
            {
                WriteBool(true);
                hasValueAction();
                return;
            }
            WriteBool(false);
        }

        /// <summary>
        /// Write nullable class type value with bool(not null) header
        /// </summary>
        /// <param name="value">value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNullableClass<T>(T value, Action hasValueAction)
            where T : class
        {
            if (value != null)
            {
                WriteBool(true);
                hasValueAction();
                return;
            }
            WriteBool(false);
        }

        /// <summary>
        /// Write datetime value (8 bytes)
        /// </summary>
        /// <param name="value">value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDateTime(DateTime value)
        {
            WriteInt64((value - MinDatetimeValue).Ticks);
        }

        /// <summary>
        /// Write timespan value (8 bytes)
        /// </summary>
        /// <param name="value">value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteTimeSpan(TimeSpan value)
        {
            WriteInt64(value.Ticks);
        }

        /// <summary>
        /// Write Guid value (16 bytes)
        /// </summary>
        /// <param name="value">value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteGuid(Guid value)
        {
            var arr = value.ToByteArray();

            Write(arr, 0, 16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] buf)
        {
            Write(buf, 0, buf.Length);
        }


        /// <summary>
        /// Write byte array, with 1-4 bytes len header, max - 4.294kkk len
        /// </summary>
        /// <param name="buf"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByteArray(byte[] buf)
        {
            Write7BitEncodedUInt32((uint)buf.Length);
            Write(buf, 0, buf.Length);
        }


        /// <summary>
        /// Сборка пакета в финальный вид перед отправкой
        /// </summary>
        /// <param name="appendHash">добавить хеш в пакет</param>
        /// <returns></returns>
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
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Write7BitEncodedUInt32(uint value)
        {
            // Write out an int 7 bits at a time.  The high bit of the byte,
            // when on, tells reader to continue reading more bytes.
            uint v = value;   // support negative numbers

            while (v >= 0x80)
            {
                WriteByte((byte)(v | 0x80));
                v >>= 7;
            }

            WriteByte((byte)v);
        }

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
