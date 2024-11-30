using Newtonsoft.Json.Linq;
using NSL.SocketCore.Utils.Exceptions;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace NSL.SocketCore.Utils.Buffer
{
    public class InputPacketBuffer : IDisposable
    {
        /// <summary>
        /// Default header part packet len
        /// </summary>
        public const int DefaultHeaderLength = 7;

        private static readonly DateTime MinDatetimeValue = new DateTime(1970, 1, 1);

        /// <summary>
        /// Min packet identity used by NSL library for implementing inner logic
        /// default value = 65335 is (ushort.MaxValue - 235), all values more or equals of this value - can be implemented in NSL
        /// </summary>
        public const ushort NSLSystemMinPID = ushort.MaxValue - 235;

        /// <summary>
        /// Dispose packet on finish async task execution
        /// </summary>
        public bool AsyncDisposing { get; set; } = true;

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

        private byte[] data;

        public byte[] Data => data;

        /// <summary>
        /// Current position in data segment
        /// </summary>
        public int DataPosition
        {
            get;
            set;
        }

        readonly int packetLength;

        bool manualDisposing = false;

        /// <summary>
        /// Full packet len
        /// </summary>
        public int PacketLength { get { return packetLength; } }

        public int DataLength => PacketLength - DefaultHeaderLength;

        /// <summary>
        /// This variable using for set "not automatic disposing", if need in custom scenarios
        /// Cannot return back, use Dispose method after need operations
        /// </summary>
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

        /// <summary>
        /// Packet identity
        /// </summary>
        public ushort PacketId { get; set; }

        public InputPacketBuffer(int packetLength, ushort packetId)
        {
            this.packetLength = packetLength;

            this.PacketId = packetId;
        }

        /// <summary>
        /// 
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
                {
                    throw new InvalidPacketHashException();
                }
            }

            DataPosition = 0;
        }

        /// <summary>
        /// Read float value (4 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloat()
        {
            DataPosition += 4;
            return Int32BitsToSingle(ReadInt32()); ;
        }

        /// <summary>
        /// Read double value (8 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble(ReadInt64());
        }

        /// <summary>
        /// Read short value (int16, 2 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16()
        {
            DataPosition += 2;
            return BinaryPrimitives.ReadInt16LittleEndian(data.AsSpan(DataPosition - 2));
        }

        /// <summary>
        /// Read ushort value (uint16, 2 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16()
        {
            DataPosition += 2;
            return BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(DataPosition - 2));
        }

        /// <summary>
        /// Read int value (int32, 4 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32()
        {
            DataPosition += 4;
            return BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(DataPosition - 4));
        }

        /// <summary>
        /// Read uint value (uint32, 4 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32()
        {
            DataPosition += 4;
            return BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(DataPosition - 4));
        }

        /// <summary>
        /// Read long value (int64, 8 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64()
        {
            DataPosition += 8;
            return BinaryPrimitives.ReadInt64LittleEndian(data.AsSpan(DataPosition - 8));
        }

        /// <summary>
        /// Read ulong value (uint64, 8 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            DataPosition += 8;
            return BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(DataPosition - 8));
        }

        /// <summary>
        /// Read byte value (1 byte)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new byte ReadByte()
        {
            DataPosition += 1;
            return data[DataPosition - 1];
        }
        [Obsolete("Use \"ReadString\"")]
        /// <summary>
        /// Read string value, with ushort(2 bytes) header, max - 32k len
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString16()
        {
            var len = ReadUInt16();

            if (len == ushort.MaxValue)
                return null;

            if (len == 0)
                return String.Empty;

            return ReadString(len);
        }

        [Obsolete("Use \"ReadString\"")]
        /// <summary>
        /// Read string value, with uint(4 bytes) header, max - 2.14kkk len
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString32()
        {
            var len = ReadUInt32();

            if (len == uint.MaxValue)
                return null;

            if (len == 0)
                return String.Empty;

            return ReadString(len);
        }

        /// <summary>
        /// Read bytes by len and encoding with coding to string
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ReadString(uint len)
        {
            if (len < 1)
                throw new ArgumentOutOfRangeException(nameof(len));
            return coding.GetString(Read((int)len).ToArray());
        }

        /// <summary>
        /// Read string value, with 1-4 bytes header, max - 2.14kkk len
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString()
        {
            var len = Read7BitEncodedUInt();

            if (len == uint.MaxValue)
                return null;

            if (len == 0)
                return String.Empty;

            return ReadString(len);
        }

        /// <summary>
        /// Read nullable type value with bool(hasValue) header
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hasValueAction"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? ReadNullable<T>(Func<T> hasValueAction)
            where T : struct
        {
            if (ReadBool())
            {
                return hasValueAction();
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe float Int32BitsToSingle(int value)
        {
            return *((float*)&value);
        }

        /// <summary>
        /// Read nullable class type value with bool(not null) header
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hasValueAction"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadNullableClass<T>(Func<T> hasValueAction)
            where T : class
        {
            if (ReadBool())
            {
                return hasValueAction();
            }

            return null;
        }

        /// <summary>
        /// Read datetime value (8 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime ReadDateTime()
        {
            return MinDatetimeValue.AddTicks(ReadInt64());
        }

        /// <summary>
        /// Read timespan value (8 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeSpan ReadTimeSpan()
        {
            return TimeSpan.FromTicks(ReadInt64());
        }

        /// <summary>
        /// Read Guid value (16 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid ReadGuid()
        {
            return new Guid(Read(16).ToArray());
        }

        /// <summary>
        /// Read byte array, with 1-4 bytes header, max - 4.294kkk len
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> ReadByteArray()
        {
            var len = Read7BitEncodedUInt();

            return Read((int)len);
        }

        /// <summary>
        /// Read collection with int32 header len
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="readAction"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> ReadCollection<T>(Func<InputPacketBuffer, T> readAction)
        {
            var len = Read7BitEncodedUInt();

            if (len == uint.MaxValue)
                return default;

            List<T> result = new List<T>((int)len);

            for (int i = 0; i < len; i++)
            {
                result.Add(readAction(this));
            }

            return result;
        }

        /// <summary>
        /// Read collection with int32 header len
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="readAction"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> ReadCollection<T>(Func<T> readAction)
        {
            var len = Read7BitEncodedUInt();

            if (len == uint.MaxValue)
                return default;

            List<T> result = new List<T>((int)len);

            for (int i = 0; i < len; i++)
            {
                result.Add(readAction());
            }

            return result;
        }

        /// <summary>
        /// Read bytes array
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> Read(int len)
        {
            if (DataLength - DataPosition < len)
                throw new OutOfMemoryException();

            DataPosition += len;

            return data.AsSpan(DataPosition - len, len);
        }

        /// <summary>
        /// Read bool value (1 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBool() => ReadByte() == 1;

        internal protected uint Read7BitEncodedUInt()
        {
            // Read out an Int32 7 bits at a time.  The high bit
            // of the byte when on means to continue reading more bytes.
            int count = 0;
            int shift = 0;
            byte b;
            do
            {
                // Check for a corrupted stream.  Read a max of 5 bytes.
                // In a future version, add a DataFormatException.
                if (shift == 5 * 7)  // 5 bytes max per Int32, shift += 7
                    throw new FormatException();

                // ReadByte handles end of stream cases for us.
                b = ReadByte();
                count |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);

            return (uint)count;
        }

        /// <summary>
        //     Sets the position within the current stream to the specified value.
        /// </summary>
        /// <param name="len"></param>
        /// <param name="seek"></param>
        /// <returns></returns>
        public int Seek(int len, SeekOrigin seek)
        {
            if (seek == SeekOrigin.Begin)
            {
                DataPosition = len;
            }
            else if (seek == SeekOrigin.Current)
            {
                DataPosition = DataPosition + len;
            }
            else if (seek == SeekOrigin.End)
            {
                DataPosition = packetLength + len;
            }

            if (DataPosition < 0)
                DataPosition = 0;

            return DataPosition;
        }

        public void SetData(byte[] data, bool replace = false)
        {
            if (!replace && this.data != null)
                throw new Exception("Data already set");

            this.data = data;
        }

        public void Dispose()
        {
            OnDispose(this);
            data = null;
        }

        public event Action<InputPacketBuffer> OnDispose = i => { };
    }
}
