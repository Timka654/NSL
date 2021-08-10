using SocketCore.Utils;

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
