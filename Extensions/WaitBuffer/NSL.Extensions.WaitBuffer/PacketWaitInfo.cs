using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Extensions.WaitBuffer
{
    public class PacketWaitInfo 
    {
        public PacketWaitInfo(byte[] buffer,int offset,int len)
        {
            Buffer = buffer;
            Offset = offset;
            Len = len;
        }

        public byte[] Buffer { get; }
        public int Offset { get; }
        public int Len { get; }
    }
}
