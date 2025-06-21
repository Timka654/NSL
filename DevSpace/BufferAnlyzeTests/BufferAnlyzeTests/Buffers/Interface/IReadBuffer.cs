using System;
using System.Collections.Generic;
using System.Text;

namespace BufferAnlyzeTests.Buffers
{
    public interface IReadBuffer
    {
        short ReadInt16();
        ushort ReadUInt16();
        
        int ReadInt32();
        uint ReadUInt32();
        
        long ReadInt64();
        ulong ReadUInt64();

        float ReadFloat32();
        double ReadFloat64();

        string ReadString16();
        string ReadString32();
        
        byte ReadByte();
        byte[] Read(int len);

    }
}
