using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cipher
{
    public class PacketNoneCipher : IPacketCipher
    {
        public object Clone()
        {
            return new PacketNoneCipher();
        }

        public byte[] Decode(byte[] buffer, int offset, int lenght)
        {
            return buffer;
        }

        public byte[] Encode(byte[] buffer, int offset, int lenght)
        {
            return buffer;
        }

        public byte[] Peek(byte[] buffer)
        {
            return buffer;
        }
    }
}
