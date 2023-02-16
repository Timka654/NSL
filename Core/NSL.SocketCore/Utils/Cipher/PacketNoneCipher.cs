using System;

namespace NSL.SocketCore.Utils.Cipher
{
    public class PacketNoneCipher : IPacketCipher
    {
        public IPacketCipher CreateEntry()
        {
            return this;
        }

        public byte[] Decode(byte[] buffer, int offset, int lenght)
        {
            byte[] dest = new byte[lenght];

            System.Buffer.BlockCopy(buffer, offset, dest, 0, lenght);

            return dest;
        }

        public void Dispose()
        {
        }

        public byte[] Encode(byte[] buffer, int offset, int lenght)
        {
            return buffer;
        }

        public byte[] Peek(byte[] buffer)
        {
            return buffer;
        }

        public bool Sync() => false;
    }
}
