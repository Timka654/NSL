using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NSL.SocketCore.Utils.Buffer
{
    public class OutputPacketBuffer : MemoryStream
    {
        private static readonly DateTime MinDatetimeValue = new DateTime(1970, 1, 1);

        /// <summary>
        /// Текущая кодировка типа String
        /// </summary>
        Encoding coding = Encoding.UTF8;

        /// <summary>
        /// Буффер с полученными данными
        /// </summary>
        byte[] _buffer = new byte[16];

        /// <summary>
        /// маскировка хедера пакета
        /// </summary>

        int offset
        {
            get { return (int)base.Position - headerLenght; }
            set { base.Position = value + headerLenght; }
        }

        /// <summary>
        /// Размер данных пакета
        /// </summary>
        public int Lenght
        {
            get;
            private set;
        }

        /// <summary>
        /// Полный размер пакета
        /// </summary>
        public int PacketLenght => Lenght + headerLenght;

        /// <summary>
        /// Индификатор пакета
        /// </summary>
        public ushort PacketId { get; set; }

        /// <summary>
        /// Хеш байт пакета
        /// </summary>
        public bool AppendHash { get; set; }

        /// <summary>
        /// Размер шапки пакета
        /// </summary>
        public const int headerLenght = 7;

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="len">начальный размер буффера</param>
        public OutputPacketBuffer(int len = 32) : base()
        {
            //начальный размер буффера необходим для оптимизации пакетов, в случае если пакет имеет заведомо известный размер, его не придется увеличивать что будет экономить время
            //инициализация буффера
            //установка размера буффера
            SetLength(len + headerLenght);

            offset = 0;
        }

        /// <summary>
        /// Запись значения float (4 bytes)
        /// </summary>
        /// <param name="value">значение</param>
        public unsafe void WriteFloat32(float value)
        {
            uint TmpValue = *(uint*)&value;
            _buffer[0] = (byte)TmpValue;
            _buffer[1] = (byte)(TmpValue >> 8);
            _buffer[2] = (byte)(TmpValue >> 16);
            _buffer[3] = (byte)(TmpValue >> 24);
            Write(_buffer, 0, 4);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            base.Write(buffer, offset, count);
            if (this.offset > Lenght)
                Lenght = this.offset;
        }

        /// <summary>
        /// Запись значения double (8 bytes)
        /// </summary>
        /// <param name="value">значение</param>
        public unsafe void WriteFloat64(double value)
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

        /// <summary>
        /// не реализовано
        /// </summary>
        /// <param name="value">значение</param>
        public void WriteDecimal(decimal value)
        {
            throw new Exception();
        }

        /// <summary>
        /// Запись значения short (int16, 2 байта)
        /// </summary>
        /// <param name="value">значение</param>
        public void WriteInt16(short value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            Write(_buffer, 0, 2);
        }

        /// <summary>
        /// Запись значения ushort (uint16, 2 байта)
        /// </summary>
        /// <param name="value">значение</param>
        public void WriteUInt16(ushort value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            Write(_buffer, 0, 2);
        }

        /// <summary>
        /// Запись значения int (int32, 4 байта)
        /// </summary>
        /// <param name="value">значение</param>
        public void WriteInt32(int value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            Write(_buffer, 0, 4);
        }

        /// <summary>
        /// Запись значения uint (uint32, 4 байта)
        /// </summary>
        /// <param name="value">значение</param>
        public void WriteUInt32(uint value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            Write(_buffer, 0, 4);
        }

        /// <summary>
        /// Запись значения long (int64, 8 байта)
        /// </summary>
        /// <param name="value">значение</param>
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
        /// Запись значения ulong (uint64, 8 байта)
        /// </summary>
        /// <param name="value">значение</param>
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
        /// Запись значения bool (1 байт)
        /// </summary>
        /// <param name="value">значение</param>
        public void WriteBool(bool value)
        {
            WriteByte((byte)(value ? 1 : 0));
        }

        /// <summary>
        /// Запись значения byte (1 байт)
        /// </summary>
        /// <param name="value">значение</param>
        public override void WriteByte(byte value)
        {
            base.WriteByte(value);
            if (this.offset > Lenght)
                Lenght = this.offset;

        }

        /// <summary>
        /// Запись значения string, с записью заголовка c размером ushort (2 байта), до 36к симв
        /// </summary>
        /// <param name="value">значение</param>
        public void WriteString16(string value)
        {
            if (value == null)
            {
                WriteUInt16(0);
                return;
            }
            byte[] buf = coding.GetBytes(value);

            WriteUInt16((ushort)buf.Length);
            if (buf.Length > 0)
                Write(buf, 0, buf.Length);
        }

        /// <summary>
        /// Запись значения string, с записью заголовка c размером  (4 байта), до 1.2ккк симв
        /// </summary>
        /// <param name="value">текст</param>
        public void WriteString32(string value)
        {
            if (value == null)
            {
                WriteUInt32(0);
                return;
            }

            byte[] buf = coding.GetBytes(value);

            WriteUInt32((uint)buf.Length);
            if (buf.Length > 0)
                Write(buf);
        }

        public void WriteCollection<T>(IEnumerable<T> arr, Action<OutputPacketBuffer, T> writeAction)
        {
            WriteInt32(arr.Count());

            foreach (var item in arr)
            {
                writeAction(this, item);
            }
        }

        public void WriteNullable<T>(Nullable<T> value, Action trueAction)
            where T : struct
        {
            if (value.HasValue)
            {
                WriteBool(true);
                trueAction();
                return;
            }
            WriteBool(false);
        }

        public void WriteNullableClass<T>(T value, Action trueAction)
            where T : class
        {
            if (value != null)
            {
                WriteBool(true);
                trueAction();
                return;
            }
            WriteBool(false);
        }

        public void WriteDateTime(DateTime value)
        {
            WriteInt64((value - MinDatetimeValue).Ticks);
        }

        public void WriteTimeSpan(TimeSpan value)
        {
            WriteInt64(value.Ticks);
        }

        public void WriteGuid(Guid value)
        {
            var arr = value.ToByteArray();

            WriteByte((byte)arr.Length);

            Write(arr, 0, arr.Length);
        }

        public void Write(byte[] buf)
        {
            Write(buf, 0, buf.Length);
        }


        /// <summary>
        /// Сборка пакета в финальный вид перед отправкой
        /// </summary>
        /// <param name="appendHash">добавить хеш в пакет</param>
        /// <returns></returns>
        public byte[] CompilePacket(bool dispose = true)
        {
            base.Seek(0, SeekOrigin.Begin);

            WriteInt32(PacketLenght);
            WriteUInt16(PacketId);

            if (AppendHash)
            {
                WriteByte((byte)((Lenght + PacketId) % 14));
            }

            var arr = base.ToArray();

            if (dispose)
                base.Dispose();

            return arr;
        }

        public virtual void Send(IClient client, bool disposeOnSend)
        {
            var pktData = CompilePacket(disposeOnSend);

            client.Send(pktData, 0, PacketLenght);
        }
    }
}
