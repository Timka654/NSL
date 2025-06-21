using System;
using System.Collections.Generic;
using System.Text;

namespace BufferAnlyzeTests.Buffers
{
    public interface IWriteBuffer
    {
        void WriteInt16(short value);
        void WriteUInt16(ushort value);

        void WriteInt32(int value);
        void WriteUInt32(uint value);

        void WriteInt64(long value);
        void WriteUInt64(ulong value);

        void WriteFloat32(float value);
        void WriteFloat64(double value);

        void WriteString16(string value);
        void WriteString32(string value);

        void WriteByte(byte value);
        void Write(byte[] value, int offs, int len);
    }
}
