using NSL.SocketCore.Utils.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NSL.SocketCore.Utils.Buffer
{
    public class InputPacketBuffer : MemoryStream
    {
        /// <summary>
        /// Размер шапки пакета
        /// </summary>
        public const int DefaultHeaderLenght = 7;

        private static readonly DateTime MinDatetimeValue = new DateTime(1970, 1, 1);

        /// <summary>
        /// Текущая кодировка типа String
        /// </summary>
        Encoding coding = Encoding.UTF8;

        /// <summary>
        /// маскировка хедера пакета
        /// </summary>
        public int DataPosition
        {
            get { return (int)base.Position - DefaultHeaderLenght; }
            set { base.Position = value + DefaultHeaderLenght; }
        }

        readonly int lenght;

        bool manualDisposing = false;

        /// <summary>
        /// Размер пакета
        /// </summary>
        public int Lenght { get { return lenght; } }

        public int DataLength => Lenght - DefaultHeaderLenght;

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
        /// Индификатор пакета
        /// </summary>
        public ushort PacketId { get; set; }

        public InputPacketBuffer()
        {

        }

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="buf">входящий буффер</param>
        /// <param name="checkHash">проверка хеша пакета</param>
        public InputPacketBuffer(byte[] buf, bool checkHash = false) : base(buf, 0, buf.Length, false, true)
        {
            //присвоение буффера
            //buffer = buf;
            //установка позиции на 0 без смещения
            Position = 0;

            //чтение размера пакета
            lenght = ReadInt32();

            //чтение инидификатора пакета
            PacketId = ReadUInt16();

            //проверка хеша пакета
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
        public bool CheckHash()
            => GetBuffer()[6] == ((lenght) + PacketId) % 255;

        /// <summary>
        /// Чтение значения float (4 bytes)
        /// </summary>
        /// <returns></returns>
        public float ReadFloat()
        {
            DataPosition += 4;
            return BitConverter.ToSingle(GetBuffer(), (int)Position - 4);
        }

        public async Task<float> ReadFloatAsync() => await Task.FromResult<float>(ReadFloat());

        /// <summary>
        /// Чтение значения double (8 bytes)
        /// </summary>
        /// <returns></returns>
        public double ReadDouble()
        {
            DataPosition += 8;
            return BitConverter.ToDouble(GetBuffer(), (int)Position - 8);
        }

        public async Task<double> ReadDoubleAsync() => await Task.FromResult<double>(ReadDouble());

        /// <summary>
        /// не реализовано
        /// </summary>
        /// <returns></returns>
        public decimal ReadDecimal()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Чтения значения short (int16, 2 байта)
        /// </summary>
        /// <returns></returns>
        public short ReadInt16()
        {
            DataPosition += 2;
            return BitConverter.ToInt16(GetBuffer(), (int)Position - 2);
        }

        public async Task<short> ReadInt16Async() => await Task.FromResult(ReadInt16());

        /// <summary>
        /// Чтения значения ushort (uint16, 2 байта)
        /// </summary>
        /// <returns></returns>
        public ushort ReadUInt16()
        {
            DataPosition += 2;
            return BitConverter.ToUInt16(GetBuffer(), (int)Position - 2);
        }

        public async Task<ushort> ReadUInt16Async() => await Task.FromResult(ReadUInt16());

        /// <summary>
        /// Чтения значения int (int32, 4 байта)
        /// </summary>
        /// <returns></returns>
        public int ReadInt32()
        {
            DataPosition += 4;
            return BitConverter.ToInt32(GetBuffer(), (int)Position - 4);
        }

        public async Task<int> ReadInt32Async() => await Task.FromResult(ReadInt32());

        /// <summary>
        /// Чтения значения uint (uint32, 4 байта)
        /// </summary>
        /// <returns></returns>
        public uint ReadUInt32()
        {
            DataPosition += 4;
            return BitConverter.ToUInt32(GetBuffer(), (int)Position - 4);
        }

        public async Task<uint> ReadUInt32Async() => await Task.FromResult(ReadUInt32());

        /// <summary>
        /// Чтения значения long (int64, 8 байта)
        /// </summary>
        /// <returns></returns>
        public long ReadInt64()
        {
            DataPosition += 8;
            return BitConverter.ToInt64(GetBuffer(), (int)Position - 8);
        }

        public async Task<long> ReadInt64Async() => await Task.FromResult(ReadInt64());

        /// <summary>
        /// Чтения значения ulong (uint64, 8 байта)
        /// </summary>
        /// <returns></returns>
        public ulong ReadUInt64()
        {
            DataPosition += 8;
            return BitConverter.ToUInt64(GetBuffer(), (int)Position - 8);
        }

        public async Task<ulong> ReadUInt64Async() => await Task.FromResult(ReadUInt64());

        /// <summary>
        /// Чтения значения byte (1 байт)
        /// </summary>
        /// <returns></returns>
        public new byte ReadByte() => (byte)base.ReadByte();

        public async Task<byte> ReadByteAsync() => await Task.FromResult<byte>(ReadByte());

        /// <summary>
        /// Чтения значения string, с чтением заголовка c размером ushort (2 байта), до 36к симв
        /// </summary>
        /// <returns></returns>
        public string ReadString16()
        {
            var len = ReadUInt16();

            if (len == ushort.MaxValue)
                return null;

            if (len == 0)
                return String.Empty;

            return ReadString16(len);
        }

        public async Task<string> ReadString16Async()
        {
            var len = await ReadUInt16Async();

            if (len == ushort.MaxValue)
                return null;

            if (len == 0)
                return String.Empty;

            return await ReadString16Async(len);
        }

        /// <summary>
        /// Чтения значения string
        /// </summary>
        /// <returns></returns>
        public string ReadString16(ushort len)
        {
            if (len < 1)
                throw new ArgumentOutOfRangeException(nameof(len));
            return coding.GetString(Read(len));
        }

        public async Task<string> ReadString16Async(ushort len)
        {
            if (len < 1)
                throw new ArgumentOutOfRangeException(nameof(len));
            return await Task.FromResult(coding.GetString(Read(len)));
        }

        /// <summary>
        /// Чтения значения string, с чтением заголовка c размером  (4 байта), до 1.2ккк симв
        /// </summary>
        /// <returns></returns>
        public string ReadString32()
        {
            var len = ReadUInt32();

            if (len == uint.MaxValue)
                return null;

            if (len == 0)
                return String.Empty;

            return ReadString32(len);
        }

        public async Task<string> ReadString32Async()
        {
            var len = await ReadUInt32Async();

            if (len == uint.MaxValue)
                return null;

            if (len == 0)
                return String.Empty;

            return await ReadString32Async(len);
        }

        /// <summary>
        /// Чтения значения string
        /// </summary>
        /// <returns></returns>
        public string ReadString32(uint len)
        {
            if (len < 1)
                throw new ArgumentOutOfRangeException(nameof(len));
            return coding.GetString(Read((int)len));
        }

        public async Task<string> ReadString32Async(uint len)
        {
            if (len < 1)
                throw new ArgumentOutOfRangeException(nameof(len));
            return await Task.FromResult(coding.GetString(Read((int)len)));
        }

        public DateTime ReadDateTime()
        {
            return MinDatetimeValue.AddTicks(ReadInt64());
        }

        public TimeSpan ReadTimeSpan()
        {
            return TimeSpan.FromTicks(ReadInt64());
        }


        public IEnumerable<T> ReadCollection<T>(Func<InputPacketBuffer, T> readAction)
        {
            int len = ReadInt32();

            if (len == -1)
                return default;

            List<T> result = new List<T>(len);

            for (int i = 0; i < len; i++)
            {
                result.Add(readAction(this));
            }

            return result;
        }

        public IEnumerable<T> ReadCollection<T>(Func<T> readAction)
        {
            int len = ReadInt32();

            if (len == -1)
                return default;

            List<T> result = new List<T>(len);

            for (int i = 0; i < len; i++)
            {
                result.Add(readAction());
            }

            return result;
        }

        public T? ReadNullable<T>(Func<T> trueAction)
            where T : struct
        {
            if (ReadBool())
            {
                return trueAction();
            }

            return null;
        }

        public T ReadNullableClass<T>(Func<T> trueAction)
            where T : class
        {
            if (ReadBool())
            {
                return trueAction();
            }

            return null;
        }

        public Guid ReadGuid()
        {
            return new Guid(Read(16));
        }

        /// <summary>
        /// Чтения массива байт
        /// </summary>
        /// <param name="len">размер</param>
        /// <returns></returns>
        public byte[] Read(int len)
        {
            byte[] buf = new byte[len];
            base.Read(buf, 0, len);
            return buf;
        }

        public async Task<byte[]> ReadAsync(int len)
        {
            byte[] buf = new byte[len];
            await base.ReadAsync(buf, 0, len);
            return buf;
        }

        /// <summary>
        /// Чтения значения bool (1 байт)
        /// </summary>
        /// <returns></returns>
        public bool ReadBool() => ReadByte() == 1;

        public async Task<bool> ReadBoolAsync() => await ReadByteAsync() == 1;

        /// <summary>
        /// Смещение положения в массиве
        /// </summary>
        /// <param name="len">размер на который нужно сместить положение</param>
        /// <param name="seek">откуда смещать</param>
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
                DataPosition = lenght + len;
            }

            if (DataPosition < 0)
                DataPosition = 0;

            return DataPosition;
        }

        /// <summary>
        /// Добавление тела пакета
        /// </summary>
        /// <param name="buffer">буффер</param>
        /// <param name="off">позиция начала копирования</param>
        public void AppendBody(byte[] buffer, int off)
        {
            int tempPos = DataPosition;

            DataPosition = 0;

            Write(buffer, off, this.lenght - 7);

            DataPosition = tempPos;
        }

        public byte[] GetBody()
        {
            byte[] buf = new byte[DataLength];
            Array.Copy(GetBuffer(), DefaultHeaderLenght, buf, 0, DataLength);

            return buf;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
