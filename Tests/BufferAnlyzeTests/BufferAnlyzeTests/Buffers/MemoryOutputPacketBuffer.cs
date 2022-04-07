using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BufferAnlyzeTests.Buffers
{
    public class MemoryOutputPacketBuffer : MemoryStream, IWriteBuffer
    {
        /// <summary>
        /// Текущая кодировка типа String
        /// </summary>
        Encoding coding = Encoding.UTF8;

        byte[] _buffer = new byte[16];

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
            get { return (int)base.Length + headerLenght; }
        }

        /// <summary>
        /// Полный размер пакета
        /// </summary>
        public int PacketLenght
        {
            get { return Lenght - headerLenght; }
        }

        /// <summary>
        /// Индификатор пакета
        /// </summary>
        public byte PacketId { get; set; }

        public int PlayerId { get; set; }

        /// <summary>
        /// Размер шапки пакета
        /// </summary>
        public const int headerLenght = 5;

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="len">начальный размер буффера</param>
        public MemoryOutputPacketBuffer(int len = 32)
        {
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

        public void WriteVector2(Vector2 value)
        {
            WriteFloat32(value.X);
            WriteFloat32(value.Y);
        }

        public void WriteVector3(Vector3 value)
        {
            WriteFloat32(value.X);
            WriteFloat32(value.Y);
            WriteFloat32(value.Z);
        }

        public void WriteQuaternion(Quaternion value)
        {
            WriteFloat32(value.X);
            WriteFloat32(value.Y);
            WriteFloat32(value.Z);
            WriteFloat32(value.W);
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
            Write(_buffer,0,8);
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
        public unsafe void WriteInt32(int value)
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
        /// Запись значения string, с записью заголовка c размером ushort (2 байта), до 36к симв
        /// </summary>
        /// <param name="value">значение</param>
        public void WriteString16(string value)
        {
            if (value == null)
            {
                WriteUInt32(0);
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

        public void WriteTimeSpan(TimeSpan value)
        {
            WriteUInt64((ulong)value.TotalMilliseconds);
        }

        public void WriteDateTime(DateTime? value)
        {
            if (value.HasValue)
                WriteDateTime(value.Value);
            else
                WriteByte(0);
        }

        public void WriteDateTime(DateTime value)
        {
            WriteUInt64((ulong)(value - DateTime.MinValue).TotalMilliseconds);
        }

        private void Write(byte[] buf)
        {
            base.Write(buf, 0, buf.Length);
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
        /// Сборка пакета в финальный вид перед отправкой
        /// </summary>
        /// <param name="cpid">Идентификатор текущего пользователя</param>
        /// <returns></returns>
        public byte[] Finalize()
        {
            base.Seek(0, SeekOrigin.Begin);

            WriteInt32(PacketLenght);
            WriteByte(PacketId);

            return base.ToArray();
        }
    }
}
