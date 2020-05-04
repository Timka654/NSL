using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BinarySerializer
{
#if !NOT_UNITY
    public class IL2CPPMemoryStream : MemoryStream
    {
        byte[] tempBuf = new byte[16];

        public IL2CPPMemoryStream(byte[] buffer) : base(buffer)
        {
        }

        public IL2CPPMemoryStream(int capacity) : base(capacity)
        {
        }

        public short ReadInt16()
        {
            Read(tempBuf, 0, 2);

            return BitConverter.ToInt16(tempBuf, 0);
        }

        public ushort ReadUInt16()
        {
            Read(tempBuf, 0, 2);

            return BitConverter.ToUInt16(tempBuf, 0);
        }

        public int ReadInt32()
        {
            Read(tempBuf, 0, 4);

            return BitConverter.ToInt32(tempBuf, 0);
        }

        public uint ReadUInt32()
        {
            Read(tempBuf, 0, 4);

            return BitConverter.ToUInt32(tempBuf, 0);
        }

        public long ReadInt64()
        {
            Read(tempBuf, 0, 8);

            return BitConverter.ToInt64(tempBuf, 0);
        }

        public ulong ReadUInt64()
        {
            Read(tempBuf, 0, 8);

            return BitConverter.ToUInt64(tempBuf, 0);
        }

        public float ReadFloat32()
        {
            Read(tempBuf, 0, 4);

            return BitConverter.ToSingle(tempBuf, 0);
        }

        public double ReadFloat64()
        {
            Read(tempBuf, 0, 8);

            return BitConverter.ToDouble(tempBuf, 0);
        }

        public string ReadString(int len)
        {
            if (len > 16)
            {
                var buf = new byte[len];
                Read(buf, 0, len);
                return Encoding.UTF8.GetString(buf).Trim();

            }
            Read(tempBuf, 0, len);

            return Encoding.UTF8.GetString(tempBuf).Trim();
        }

        public string ReadString16()
        {
            return ReadString(ReadInt16());
        }

        public string ReadString32()
        {
            return ReadString(ReadInt32());
        }

        public void WriteInt16(short value)
        {
            Write(BitConverter.GetBytes(value), 0, 2);
        }

        public void WriteUInt16(ushort value)
        {
            Write(BitConverter.GetBytes(value), 0, 2);
        }

        public void WriteInt32(int value)
        {
            Write(BitConverter.GetBytes(value), 0, 4);
        }

        public void WriteUInt32(uint value)
        {
            Write(BitConverter.GetBytes(value), 0, 4);
        }

        public void WriteInt64(long value)
        {
            Write(BitConverter.GetBytes(value), 0, 8);
        }

        public void WriteUInt64(ulong value)
        {
            Write(BitConverter.GetBytes(value), 0, 8);
        }

        public void WriteFloat32(float value)
        {
            Write(BitConverter.GetBytes(value), 0, 4);
        }

        public void WriteFloat64(double value)
        {
            Write(BitConverter.GetBytes(value), 0, 8);
        }

        public void WriteString(string value, int len)
        {
            var buf = Encoding.UTF8.GetBytes(value);

            Write(buf, 0, len);
        }

        public void WriteString16(string value)
        {
            WriteInt16((short)value.Length);

            WriteString(value, value.Length);
        }

        public void WriteString32(string value)
        {
            WriteInt32(value.Length);

            WriteString(value, value.Length);
        }
    }
#endif
}
