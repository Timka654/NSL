using System;

namespace NSL.SocketCore.Utils.Cipher
{
    public class PacketNoneCipher : IPacketCipher
    {
        public IPacketCipher CreateEntry()
        {
            return this;
        }

        public bool DecodeRef(ref byte[] buffer, int offset, int length) => true;

        public bool DecodeHeaderRef(ref byte[] buffer, int offset) => true;

        public bool EncodeRef(ref byte[] buffer, int offset, int length) => true;

        public bool EncodeHeaderRef(ref byte[] buffer, int offset) => true;

        public byte[] Decode(byte[] buffer, int offset, int length)
        {
            byte[] dest = new byte[length];

            System.Buffer.BlockCopy(buffer, offset, dest, 0, length);

            return dest;
        }

        public void Dispose()
        {
        }

        public byte[] Encode(byte[] buffer, int offset, int length)
        {
            return buffer;
        }

        public byte[] Peek(byte[] buffer)
        {
            return buffer;
        }

        public void Peek(ArraySegment<byte> buffer)
        {
        }

        public bool Sync() => false;
    }
}
