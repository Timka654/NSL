using NSL.SocketCore.Utils.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NSL.SocketCore.Utils.Buffer
{
    public class InputPacketBuffer : MemoryStream
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
        /// Current position in data segment
        /// </summary>
        public int DataPosition
        {
            get { return (int)base.Position - DefaultHeaderLength; }
            set { base.Position = value + DefaultHeaderLength; }
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

        public InputPacketBuffer()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buf">input buffer</param>
        /// <param name="checkHash">validate packet header hash</param>
        public InputPacketBuffer(byte[] buf, bool checkHash = false) : base(buf, 0, buf.Length, false, true)
        {
            //установка позиции на 0 без смещения
            Position = 0;

            //чтение размера пакета
            packetLength = ReadInt32();

            //чтение идентификатора пакета
            PacketId = ReadUInt16();

            //проверка хеша шапки пакета
            if (checkHash)
            {
                if (!CheckHash())
                {
                    throw new InvalidPacketHashException();
                }
            }

            //установка позиции на текущий размер хедера, для дальнейшего чтения
            DataPosition = 0;
        }

        /// <summary>
        /// Check header hash
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckHash()
            => GetBuffer()[6] == ((packetLength) + PacketId) % 255;

        /// <summary>
        /// Read float value (4 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloat()
        {
            DataPosition += 4;
            return BitConverter.ToSingle(GetBuffer(), (int)Position - 4);
        }

        /// <summary>
        /// Read double value (8 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDouble()
        {
            DataPosition += 8;
            return BitConverter.ToDouble(GetBuffer(), (int)Position - 8);
        }

        /// <summary>
        /// Read short value (int16, 2 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16()
        {
            DataPosition += 2;
            return BitConverter.ToInt16(GetBuffer(), (int)Position - 2);
        }

        /// <summary>
        /// Read ushort value (uint16, 2 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16()
        {
            DataPosition += 2;
            return BitConverter.ToUInt16(GetBuffer(), (int)Position - 2);
        }

        /// <summary>
        /// Read int value (int32, 4 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32()
        {
            DataPosition += 4;
            return BitConverter.ToInt32(GetBuffer(), (int)Position - 4);
        }

        /// <summary>
        /// Read uint value (uint32, 4 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32()
        {
            DataPosition += 4;
            return BitConverter.ToUInt32(GetBuffer(), (int)Position - 4);
        }

        /// <summary>
        /// Read long value (int64, 8 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64()
        {
            DataPosition += 8;
            return BitConverter.ToInt64(GetBuffer(), (int)Position - 8);
        }

        /// <summary>
        /// Read ulong value (uint64, 8 bytes)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            DataPosition += 8;
            return BitConverter.ToUInt64(GetBuffer(), (int)Position - 8);
        }

        /// <summary>
        /// Read byte value (1 byte)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new byte ReadByte() => (byte)base.ReadByte();

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
            return coding.GetString(Read((int)len));
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
            return new Guid(Read(16));
        }

        /// <summary>
        /// Read byte array, with 1-4 bytes header, max - 4.294kkk len
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ReadByteArray()
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
        public byte[] Read(int len)
        {
            if (base.Length - this.Position < len)
                throw new OutOfMemoryException();

            byte[] buf = new byte[len];

            base.Read(buf, 0, len);

            return buf;
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

        /// <summary>
        /// Add Packet body segment
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="off"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendBody(byte[] buffer, int off)
        {
            int tempPos = DataPosition;

            DataPosition = 0;

            Write(buffer, off, this.packetLength - 7);

            DataPosition = tempPos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] GetBody()
        {
            byte[] buf = new byte[DataLength];
            Array.Copy(GetBuffer(), DefaultHeaderLength, buf, 0, DataLength);

            return buf;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
