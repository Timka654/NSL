using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BufferAnlyzeTests.Buffers
{
    public class MemoryInputPacketBuffer : MemoryStream, IReadBuffer
    {
        /// <summary>
        /// Размер шапки пакета
        /// </summary>
        public const int headerLenght = 5;

        private byte[] m_buffer = new byte[16];

        /// <summary>
        /// Текущая кодировка типа String
        /// </summary>
        Encoding coding = Encoding.UTF8;

        int offset
        {
            get { return (int)base.Position - headerLenght; }
            set { base.Position = value + headerLenght; }
        }

        /// <summary>
        /// Текущая позиция чтения в потоке
        /// </summary>
        public int Offset
        {
            get { return offset + headerLenght; }
        }

        /// <summary>
        /// Размер пакета
        /// </summary>
        public int Lenght
        {
            get { return (int)base.Length; }
        }

        internal Quaternion ReadQuaternion()
        {
            return new Quaternion(
            ReadFloat32(),
            ReadFloat32(),
            ReadFloat32(),
            ReadFloat32());
        }

        /// <summary>
        /// Индификатор пакета
        /// </summary>
        public byte PacketId { get; set; }

        public int PlayerId { get; set; }

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="buf">входящий буффер</param>
        /// <param name="checkHash">проверка хеша пакета</param>
        public MemoryInputPacketBuffer(byte[] buf, int playerId) : base(buf)
        {
            //установка позиции на 0 без смещения
            base.Position = 0;

            //чтение размера пакета
            ReadInt32();

            //чтение инидификатора пакета
            PacketId = ReadByte();

            PlayerId = playerId;

            //установка позиции на текущий размер хедера, для дальнейшего чтения
            offset = 0;
        }

        private void FillBuffer(int len)
        {
            this.Read(m_buffer, 0, len);
        }

        /// <summary>
        /// Чтение значения float (4 bytes)
        /// </summary>
        /// <returns></returns>
        public float ReadFloat32()
        {
            FillBuffer(4);
            return BitConverter.ToSingle(m_buffer, 0);
        }

        public Vector2 ReadVector2()
        {
            return new Vector2(ReadFloat32(), ReadFloat32());
        }

        public Vector3 ReadVector3()
        {
            return new Vector3(ReadFloat32(), ReadFloat32(), ReadFloat32());
        }

        /// <summary>
        /// Чтение значения double (8 bytes)
        /// </summary>
        /// <returns></returns>
        public double ReadFloat64()
        {
            FillBuffer(8);
            return BitConverter.ToDouble(m_buffer, 0);
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
            FillBuffer(2);
            return BitConverter.ToInt16(m_buffer, 0);
        }

        /// <summary>
        /// Чтения значения ushort (uint16, 2 байта)
        /// </summary>
        /// <returns></returns>
        public ushort ReadUInt16()
        {
            FillBuffer(2);
            return BitConverter.ToUInt16(m_buffer, 0);
        }

        /// <summary>
        /// Чтения значения int (int32, 4 байта)
        /// </summary>
        /// <returns></returns>
        public int ReadInt32()
        {
            FillBuffer(4);
            return BitConverter.ToInt32(m_buffer, 0);
        }

        /// <summary>
        /// Чтения значения uint (uint32, 4 байта)
        /// </summary>
        /// <returns></returns>
        public uint ReadUInt32()
        {
            FillBuffer(4);
            return BitConverter.ToUInt32(m_buffer, 0);
        }

        /// <summary>
        /// Чтения значения long (int64, 8 байта)
        /// </summary>
        /// <returns></returns>
        public long ReadInt64()
        {
            FillBuffer(8);
            return BitConverter.ToInt64(m_buffer, 0);
        }

        /// <summary>
        /// Чтения значения ulong (uint64, 8 байта)
        /// </summary>
        /// <returns></returns>
        public ulong ReadUInt64()
        {
            FillBuffer(8);
            return BitConverter.ToUInt64(m_buffer, 0);
        }

        /// <summary>
        /// Чтения значения string, с чтением заголовка c размером ushort (2 байта), до 36к симв
        /// </summary>
        /// <returns></returns>
        public string ReadString16()
        {
            return ReadString16(ReadUInt16());
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

        /// <summary>
        /// Чтения значения string, с чтением заголовка c размером  (4 байта), до 1.2ккк симв
        /// </summary>
        /// <returns></returns>
        public string ReadString32()
        {
            return ReadString32(ReadUInt32());
        }

        /// <summary>
        /// Чтения значения string
        /// </summary>
        /// <returns></returns>
        public string ReadString32(uint len)
        {
            if (len == 0)
                return "";
            return coding.GetString(Read((int) len));
        }

        public DateTime? ReadDateTime()
        {
            var r = ReadUInt64();
            if (r == 0)
                return null;
            return DateTime.MinValue.AddMilliseconds(r);
        }

        public TimeSpan ReadTimeSpan()
        {
            return TimeSpan.FromMilliseconds(ReadUInt64());
        }

        /// <summary>
        /// Чтения массива байт
        /// </summary>
        /// <param name="len">размер</param>
        /// <returns></returns>
        public byte[] Read(int len)
        {
            byte[] buf = new byte[len];
            Read(buf, 0, len);
            return buf;
        }

        /// <summary>
        /// Чтения значения bool (1 байт)
        /// </summary>
        /// <returns></returns>
        public bool ReadBool()
        {
            return ReadByte() == 1;
        }

        public new byte ReadByte()
        {
            return (byte)base.ReadByte();
        }

        /// <summary>
        /// Смещение положения в массиве
        /// </summary>
        /// <param name="len">размер на который нужно сместить положение</param>
        /// <param name="seek">откуда смещать</param>
        /// <returns></returns>
        public new int Seek(long offs, SeekOrigin loc)
        {
            base.Seek(offs + headerLenght, loc);
            if (offset < 0)
                offset = 0;
            return offset;
        }

        /// <summary>
        /// Добавление тела пакета
        /// </summary>
        /// <param name="buffer">буффер</param>
        /// <param name="off">позиция начала копирования</param>
        public void AppendBody(byte[] buffer, int off)
        {
            this.Position = headerLenght;
            this.Write(buffer, off, Lenght - offset);
            this.Position = headerLenght;
        }
    }
}