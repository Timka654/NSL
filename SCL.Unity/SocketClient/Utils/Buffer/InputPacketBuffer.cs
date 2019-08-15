using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;
using SCL.SocketClient.Utils.Exceptions;

namespace SCL.SocketClient.Utils.Buffer
{
    public class InputPacketBuffer
    {
        private static readonly DateTime MinDatetimeValue = new DateTime(1970, 1, 1);
        /// <summary>
        /// Текущая кодировка типа String
        /// </summary>
        Encoding coding = Encoding.UTF8;

        /// <summary>
        /// Буффер с полученными данными
        /// </summary>
        readonly byte[] buffer;

        /// <summary>
        /// маскировка хедера пакета
        /// </summary>
        int offs;
        int offset { get { return offs; } set { offs = value; } }

        readonly int lenght;

        /// <summary>
        /// Текущая позиция чтения в потоке
        /// </summary>
        public int Offset { get { return offset - headerLenght; } }

        /// <summary>
        /// Размер пакета
        /// </summary>
        public int Lenght { get { return lenght; } }

        public int DataLength => Lenght - headerLenght;

        /// <summary>
        /// Индификатор пакета
        /// </summary>
        public ushort PacketId { get; set; }

        /// <summary>
        /// Размер шапки пакета
        /// </summary>
        public const int headerLenght = 7;

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="buf">входящий буффер</param>
        /// <param name="checkHash">проверка хеша пакета</param>
        public InputPacketBuffer(byte[] buf, bool checkHash = false)
        {
            //присвоение буффера
            buffer = buf;
            //установка позиции на 0 без смещения
            offset = 0;

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
            offset = headerLenght;
        }

        /// <summary>
        /// Проверка хеша
        /// </summary>
        /// <returns></returns>
        public bool CheckHash()
        {
            //проверяем хеш по спец формуле
            if (buffer[6] != ((lenght - headerLenght) + PacketId) % 14)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Чтение значения float (4 bytes)
        /// </summary>
        /// <returns></returns>
        public float ReadFloat()
        {
            offset += 4;
            return BitConverter.ToSingle(buffer, offset - 4);
        }

        public async Task<float> ReadFloatAsync()
        {
            return await Task.FromResult<float>(BitConverter.ToSingle(buffer, offset - 4));
        }

        /// <summary>
        /// Чтение значения double (8 bytes)
        /// </summary>
        /// <returns></returns>
        public double ReadDouble()
        {
            offset += 8;
            return BitConverter.ToDouble(buffer, offset - 8);
        }

        public async Task<double> ReadDoubleAsync()
        {
            offset += 8;
            return await Task.FromResult<double>(BitConverter.ToDouble(buffer, offset - 8));
        }

        /// <summary>
        /// не реализовано
        /// </summary>
        /// <returns></returns>
        public decimal ReadDecimal()
        {
            throw new Exception();
        }

        /// <summary>
        /// Чтения значения short (int16, 2 байта)
        /// </summary>
        /// <returns></returns>
        public short ReadInt16()
        {
            offset += 2;
            return BitConverter.ToInt16(buffer, offset - 2);
        }

        public async Task<short> ReadInt16Async()
        {
            offset += 2;
            return await Task.FromResult(BitConverter.ToInt16(buffer, offset - 2));
        }

        /// <summary>
        /// Чтения значения ushort (uint16, 2 байта)
        /// </summary>
        /// <returns></returns>
        public ushort ReadUInt16()
        {
            offset += 2;
            return BitConverter.ToUInt16(buffer, offset - 2);
        }

        public async Task<ushort> ReadUInt16Async()
        {
            offset += 2;
            return await Task.FromResult(BitConverter.ToUInt16(buffer, offset - 2));
        }

        /// <summary>
        /// Чтения значения int (int32, 4 байта)
        /// </summary>
        /// <returns></returns>
        public int ReadInt32()
        {
            offset += 4;
            return BitConverter.ToInt32(buffer, offset - 4);
        }

        public async Task<int> ReadInt32Async()
        {
            offset += 4;
            return await Task.FromResult(BitConverter.ToInt32(buffer, offset - 4));
        }

        /// <summary>
        /// Чтения значения uint (uint32, 4 байта)
        /// </summary>
        /// <returns></returns>
        public uint ReadUInt32()
        {
            offset += 4;
            return BitConverter.ToUInt32(buffer, offset - 4);
        }

        public async Task<uint> ReadUInt32Async()
        {
            offset += 4;
            return await Task.FromResult(BitConverter.ToUInt32(buffer, offset - 4));
        }

        /// <summary>
        /// Чтения значения long (int64, 8 байта)
        /// </summary>
        /// <returns></returns>
        public long ReadInt64()
        {
            offset += 8;
            return BitConverter.ToInt64(buffer, offset - 8);
        }

        public async Task<long> ReadInt64Async()
        {
            offset += 8;
            return await Task.FromResult(BitConverter.ToInt64(buffer, offset - 8));
        }

        /// <summary>
        /// Чтения значения ulong (uint64, 8 байта)
        /// </summary>
        /// <returns></returns>
        public ulong ReadUInt64()
        {
            offset += 8;
            return BitConverter.ToUInt64(buffer, offset - 8);
        }

        public async Task<ulong> ReadUInt64Async()
        {
            offset += 8;
            return await Task.FromResult(BitConverter.ToUInt64(buffer, offset - 8));
        }

        /// <summary>
        /// Чтения значения byte (1 байт)
        /// </summary>
        /// <returns></returns>
        public byte ReadByte()
        {
            return buffer[offset++];
        }

        public async Task<byte> ReadByteAsync()
        {
            return await Task.FromResult<byte>(buffer[offset++]);
        }

        /// <summary>
        /// Чтения значения string, с чтением заголовка c размером ushort (2 байта), до 36к симв
        /// </summary>
        /// <returns></returns>
        public string ReadString16()
        {
            return ReadString16(ReadUInt16());
        }

        public async Task<string> ReadString16Async()
        {
            return await ReadString16Async(await ReadUInt16Async());
        }

        /// <summary>
        /// Чтения значения string
        /// </summary>
        /// <returns></returns>
        public string ReadString16(ushort len)
        {
            if (len == 0)
                return "";
            return coding.GetString(Read(len));
        }

        public async Task<string> ReadString16Async(ushort len)
        {
            if (len == 0)
                return "";
            return await Task.FromResult(coding.GetString(Read(len)));
        }

        /// <summary>
        /// Чтения значения string, с чтением заголовка c размером  (4 байта), до 1.2ккк симв
        /// </summary>
        /// <returns></returns>
        public string ReadString32()
        {
            return ReadString32(ReadUInt32());
        }

        public async Task<string> ReadString32Async()
        {
            return await ReadString32Async(await ReadUInt32Async());
        }

        /// <summary>
        /// Чтения значения string
        /// </summary>
        /// <returns></returns>
        public string ReadString32(uint len)
        {
            if (len == 0)
                return "";
            return coding.GetString(Read((int)len));
        }

        public async Task<string> ReadString32Async(uint len)
        {
            if (len == 0)
                return "";
            return await Task.FromResult(coding.GetString(Read((int)len)));
        }

        public DateTime? ReadDateTime()
        {
            var r = ReadDouble();
            if (r == 0)
                return null;
            return MinDatetimeValue.AddMilliseconds(r);
        }

        /// <summary>
        /// Чтения массива байт
        /// </summary>
        /// <param name="len">размер</param>
        /// <returns></returns>
        public byte[] Read(int len)
        {
            byte[] buf = new byte[len];
            for (int i = 0; i < len; i++)
            {
                buf[i] = buffer[offset + i];
            }
            offset += len;
            return buf;
        }

        public async Task<byte[]> ReadAsync(int len)
        {
            return await Task.Run(() =>
            {
                byte[] buf = new byte[len];
                for (int i = 0; i < len; i++)
                {
                    buf[i] = buffer[offset + i];
                }
                offset += len;
                return buf;
            });
        }

        /// <summary>
        /// Чтения значения bool (1 байт)
        /// </summary>
        /// <returns></returns>
        public bool ReadBool()
        {
            return ReadByte() == 1;
        }

        public async Task<bool> ReadBoolAsync()
        {
            return await ReadByteAsync() == 1;
        }

        public T Deserialize<T>(string schemeName)
        {
            BinarySerializer.BinarySerializer bs = new BinarySerializer.BinarySerializer(TypeStorage.Instance);
            try
            {
                var result = bs.Deserialize<T>(schemeName, buffer, ref offs);

                //offset = bs.Length;

                return (T)result;
            }
            catch (Exception ex)
            {
                throw ex; //new Exception($"Serializer Stack \r\n{bs.Stack}", ex);
            }
        }

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
                offset = len + headerLenght;
            }
            else if (seek == SeekOrigin.Current)
            {
                offset = offset + len;
            }
            else if (seek == SeekOrigin.End)
            {
                offset = lenght + len;
            }
            if (offset < headerLenght)
                offset = headerLenght;
            return offset;
        }

        /// <summary>
        /// Добавление тела пакета
        /// </summary>
        /// <param name="buffer">буффер</param>
        /// <param name="off">позиция начала копирования</param>
        public void AppendBody(byte[] buffer, int off)
        {
            Array.Copy(buffer, off, this.buffer, 7, this.lenght - 7);
        }
    }
}
