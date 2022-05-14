namespace SocketCore.Utils.Cipher
{
    public class PacketNoneCipher : IPacketCipher
    {
        public IPacketCipher CreateEntry()
        {
            return this;
        }

        public byte[] Decode(byte[] buffer, int offset, int lenght)
        {
            return buffer;
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
