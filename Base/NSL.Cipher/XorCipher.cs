using NSL.SocketCore.Utils;
using System;
using System.Text;

namespace NSL.Cipher
{
    public class XorCipher : IPacketCipher
    {
        private readonly byte[] key;

        public XorCipher(string key) : this(Encoding.UTF8.GetBytes(key))
        {
        }

        public XorCipher(byte[] key)
        {
            this.key = key;
        }

        void EncryptOrDecrypt(Span<byte> buf)
        {
            for (int c = 0; c < buf.Length; c++)
                buf[c] = (byte)((uint)buf[c] ^ (uint)key[c % key.Length]);
        }

        public IPacketCipher CreateEntry()
            => new XorCipher(key);

        public byte[] Decode(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public bool DecodeHeaderRef(ref byte[] buffer, int offset)
        {
            EncryptOrDecrypt(buffer.AsSpan(offset, 7));

            return true;
        }

        public bool DecodeRef(ref byte[] buffer, int offset, int length)
        {
            EncryptOrDecrypt(buffer.AsSpan(offset, length));

            return true;
        }

        public void Dispose()
        {
        }

        public byte[] Encode(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public bool EncodeHeaderRef(ref byte[] buffer, int offset)
        {
            EncryptOrDecrypt(buffer.AsSpan(offset, 7));

            return true;
        }

        public bool EncodeRef(ref byte[] buffer, int offset, int length)
        {
            EncryptOrDecrypt(buffer.AsSpan(offset, length));

            return true;
        }

        public byte[] Peek(byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public void Peek(ArraySegment<byte> buffer)
        {
            throw new NotImplementedException();
        }

        public bool Sync()
            => false;
    }
}
