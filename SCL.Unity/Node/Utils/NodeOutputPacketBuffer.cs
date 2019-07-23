using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCL.Node.Utils
{
    public class NodeOutputPacketBuffer
    {
        /// <summary>
        /// Текущая кодировка типа String
        /// </summary>
        Encoding coding = Encoding.UTF8;

        /// <summary>
        /// Буффер с полученными данными
        /// </summary>
        byte[] buffer;

        /// <summary>
        /// Текущий размер буффера (включая пустые байты)
        /// </summary>
        int bufferLenght;

        /// <summary>
        /// маскировка хедера пакета
        /// </summary>
        int offs;

        int offset
        {
            get { return offs + headerLenght; }
            set { offs = value; }
        }

        int lenght;

        /// <summary>
        /// Текущая позиция чтения в потоке
        /// </summary>
        public int Offset
        {
            get { return offs; }
        }

        /// <summary>
        /// Размер данных пакета
        /// </summary>
        public int Lenght
        {
            get { return lenght; }
        }

        /// <summary>
        /// Полный размер пакета
        /// </summary>
        public int PacketLenght
        {
            get { return lenght + headerLenght; }
        }

        /// <summary>
        /// Индификатор пакета
        /// </summary>
        public byte PacketId { get; set; }

        public int PlayerId { get; set; }

        /// <summary>
        /// Хеш байт пакета
        /// </summary>
        public bool AppendHash { get; set; }

        /// <summary>
        /// Размер шапки пакета
        /// </summary>
        public const int headerLenght = 9;

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="len">начальный размер буффера</param>
        public NodeOutputPacketBuffer(int len = 32)
        {
            //начальный размер буффера необходим для оптимизации пакетов, в случае если пакет имеет заведомо известный размер, его не придется увеличивать что будет экономить время
            //инициализация буффера
            buffer = new byte[headerLenght + len];
            //установка размера буффера
            bufferLenght = headerLenght + len;

            offset = 0;
        }
        byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        public void WriteObject(object[] value)
        {
            for(int i = 0; i < value.Length; i++)
            {
                WriteObject(value[i]);
            }
                
        }
        public void WriteObject(object value)
        {
            dynamic d = value;
            Write(d);
        }
        /// <summary>
        /// Запись значения float (4 bytes)
        /// </summary>
        /// <param name="value">значение</param>
        public void Write(float value)
        {
            Write(BitConverter.GetBytes(value), 0, 4);
        }

        public async Task WriteAsync(float value)
        {
            await WriteAsync(BitConverter.GetBytes(value), 0, 4);
        }

        public void Write(Vector2 value)
        {
            Write(value.x);
            Write(value.y);
        }

        public void Write(Vector3 value)
        {
            Write(value.x);
            Write(value.y);
            Write(value.z);
        }


        /// <summary>
        /// Запись значения double (8 bytes)
        /// </summary>
        /// <param name="value">значение</param>
        public void Write(double value)
        {
            Write(BitConverter.GetBytes(value), 0, 8);
        }

        public async Task WriteAsync(double value)
        {
            await WriteAsync(BitConverter.GetBytes(value), 0, 8);
        }

        /// <summary>
        /// не реализовано
        /// </summary>
        /// <param name="value">значение</param>
        public void Write(decimal value)
        {
            throw new Exception();
        }

        /// <summary>
        /// Запись значения short (int16, 2 байта)
        /// </summary>
        /// <param name="value">значение</param>
        public void Write(short value)
        {
            Write(BitConverter.GetBytes(value), 0, 2);
        }

        public async Task WriteAsync(short value)
        {
            await WriteAsync(BitConverter.GetBytes(value), 0, 2);
        }

        /// <summary>
        /// Запись значения ushort (uint16, 2 байта)
        /// </summary>
        /// <param name="value">значение</param>
        public void Write(ushort value)
        {
            Write(BitConverter.GetBytes(value), 0, 2);
        }

        public async Task WriteUInt16Async(ushort value)
        {
            await WriteAsync(BitConverter.GetBytes(value), 0, 2);
        }

        /// <summary>
        /// Запись значения int (int32, 4 байта)
        /// </summary>
        /// <param name="value">значение</param>
        public void Write(int value)
        {
            Write(BitConverter.GetBytes(value), 0, 4);
        }

        public async Task WriteAsync(int value)
        {
            await WriteAsync(BitConverter.GetBytes(value), 0, 4);
        }

        /// <summary>
        /// Запись значения uint (uint32, 4 байта)
        /// </summary>
        /// <param name="value">значение</param>
        public void Write(uint value)
        {
            Write(BitConverter.GetBytes(value), 0, 4);
        }

        public async Task WriteAsync(ushort value)
        {
            await WriteAsync(BitConverter.GetBytes(value), 0, 4);
        }

        /// <summary>
        /// Запись значения long (int64, 8 байта)
        /// </summary>
        /// <param name="value">значение</param>
        public void Write(long value)
        {
            Write(BitConverter.GetBytes(value), 0, 8);
        }

        public async Task WriteAsync(long value)
        {
            await WriteAsync(BitConverter.GetBytes(value), 0, 8);
        }

        /// <summary>
        /// Запись значения ulong (uint64, 8 байта)
        /// </summary>
        /// <param name="value">значение</param>
        public void Write(ulong value)
        {
            Write(BitConverter.GetBytes(value), 0, 8);
        }

        public async Task WriteAsync(ulong value)
        {
            await WriteAsync(BitConverter.GetBytes(value), 0, 8);
        }

        /// <summary>
        /// Запись значения bool (1 байт)
        /// </summary>
        /// <param name="value">значение</param>
        public void Write(bool value)
        {
            Write((byte) (value ? 1 : 0));
        }

        public async Task WriteAsync(bool value)
        {
            await WriteAsync((byte) (value ? 1 : 0));
        }

        /// <summary>
        /// Запись значения byte (1 байт)
        /// </summary>
        /// <param name="value">значение</param>
        public void Write(byte value)
        {
            if (offset + 1 >= bufferLenght)
                SetLength(1);
            buffer[offset] = value;
            offs++;
            if (offs >= lenght)
                lenght = offs;
        }

        public async Task WriteAsync(byte value)
        {
            await Task.Run(() =>
            {
                if (offset + 1 >= bufferLenght)
                    SetLength(1);
                buffer[offset] = value;
                offs++;
                if (offs >= lenght)
                    lenght = offs;
            });
        }

        /// <summary>
        /// Запись значения string, с записью заголовка c размером ushort (2 байта), до 36к симв
        /// </summary>
        /// <param name="value">значение</param>
        public void Write(string value)
        {
            if (value == null)
                value = "";
            byte[] buf = coding.GetBytes(value);

            Write((ushort) buf.Length);
            if (buf.Length > 0)
                Write(buf, 0, buf.Length);
        }

        public async Task WriteAsync(string value)
        {
            await Task.Run(() =>
            {
                if (value == null)
                    value = "";
                byte[] buf = coding.GetBytes(value);

                Write((ushort) buf.Length);
                if (buf.Length > 0)
                    Write(buf, 0, buf.Length);
            });
        }

        /// <summary>
        /// Запись значения string, с записью заголовка c размером  (4 байта), до 1.2ккк симв
        /// </summary>
        /// <param name="value">текст</param>
        public void WriteString32(string value)
        {
            if (value == null)
                value = "";
            byte[] buf = coding.GetBytes(value);

            Write((uint) buf.Length);
            if (buf.Length > 0)
                Write(buf, 0, buf.Length);
        }

        public async Task WriteString32Async(string value)
        {
            await Task.Run(() =>
            {
                if (value == null)
                    value = "";
                byte[] buf = coding.GetBytes(value);

                Write((uint) buf.Length);
                if (buf.Length > 0)
                    Write(buf, 0, buf.Length);
            });
        }

        public async Task WriteDateTimeAsync(DateTime? value)
        {
            if (value.HasValue)
                await WriteDateTimeAsync(value.Value);
            else
                await WriteAsync(0);
        }

        public async Task WriteDateTimeAsync(DateTime value)
        {
            await WriteAsync((ulong) (value - DateTime.MinValue).TotalMilliseconds);
        }

        public void WriteTimeSpan(TimeSpan value)
        {
            Write((ulong) value.TotalMilliseconds);
        }

        public void WriteDateTime(DateTime? value)
        {
            if (value.HasValue)
                WriteDateTime(value.Value);
            else
                Write(0);
        }

        public void WriteDateTime(DateTime value)
        {
            Write((ulong) (value - DateTime.MinValue).TotalMilliseconds);
        }

        /// <summary>
        /// Запись массива байт
        /// </summary>
        /// <param name="buf">буффер</param>
        /// <param name="off">позиция в буффере</param>
        /// <param name="len">размер для записи</param>
        public void Write(byte[] buf, int off, int len)
        {
            if (offset + len >= bufferLenght)
                SetLength(len);
            for (int i = 0; i < len; i++)
            {
                buffer[offset + i] = buf[off + i];
            }

            offs += len;
            if (offs >= lenght)
                lenght = offs;
        }

        public async Task WriteAsync(byte[] buf, int off, int len)
        {
            await Task.Run(() =>
            {
                if (offset + len >= bufferLenght)
                    SetLength(len);
                for (int i = 0; i < len; i++)
                {
                    buffer[offset + i] = buf[off + i];
                }

                offs += len;
                if (offs >= lenght)
                    lenght = offs;
            });
        }

        /// <summary>
        /// Добавление размера пакета в случае недостающего размера буффера
        /// </summary>
        /// <param name="appendLen">кол-во байт добавляемых в буффер</param>
        private void SetLength(int appendLen)
        {
            while (offset + appendLen >= bufferLenght)
            {
                bufferLenght = bufferLenght * 2;
            }

            Array.Resize(ref buffer, bufferLenght);
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
                offs = len;
            }
            else if (seek == SeekOrigin.Current)
            {
                offs += len;
            }
            else if (seek == SeekOrigin.End)
            {
                offs = lenght + len;
            }

            if (offs < 0)
                offs = 0;
            return offset;
        }

        /// <summary>
        /// Сборка пакета в финальный вид перед отправкой
        /// </summary>
        /// <param name="appendHash">добавить хеш в пакет</param>
        /// <returns></returns>
        public byte[] GetBuffer(uint cpid)
        {
            int off = offs;

            offset = 0 - headerLenght;
            Write(PacketLenght);
            Write(cpid);
            Write(PacketId);

            if (AppendHash)
            {
                Write((byte) (((PlayerId - PacketId) + cpid) % 14));
            }

            offs = off;

            return buffer;
        }
    }
}