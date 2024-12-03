using System;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.Cipher.RC.RC4
{
    public class XRC4Cipher : IPacketCipher
    {

        public XRC4Cipher(string key) : this()
        {
            SetKey(key);
        }

        public XRC4Cipher()
        {
            rc4 = new RC4Cipher();
            Clear();
        }

        public void SetKey(String pKey)
        {
            rc4.init(pKey);
        }

        public byte[] Peek(byte[] buff)
        {
            byte[] res = new byte[InputPacketBuffer.DefaultHeaderLength];
            TryCipher(ref buff, 0, ref res, InputPacketBuffer.DefaultHeaderLength);
            return res;
        }

        public void Peek(ArraySegment<byte> buffer)
        {
            throw new NotImplementedException();
        }

        public byte[] Peek(byte[] buff, int offset)
        {
            byte[] res = new byte[InputPacketBuffer.DefaultHeaderLength];
            TryCipher(ref buff, offset, ref res, InputPacketBuffer.DefaultHeaderLength);
            return res;
        }

        public byte[] Encode(byte[] buff)
        {
            return Encode(buff, 0, buff.Length);
        }

        public byte[] Encode(byte[] buff, int offset, int len)
        {
            byte[] res = new byte[len];

            DoCipher(ref buff, offset, ref res, len);
            return res;
        }

        public bool DecodeRef(ref byte[] buffer, int offset, int length)
        {
            rc4.codeBlock(buffer.AsSpan(offset, length)
                , new ArraySegment<byte>(buffer, offset, length));

            return true;
        }

        public bool DecodeHeaderRef(ref byte[] buffer, int offset)
        {
            DoCipher(ref buffer, offset, ref buffer, InputPacketBuffer.DefaultHeaderLength);

            return true;
        }

        public bool EncodeRef(ref byte[] buffer, int offset, int length)
        {
            DoCipher(ref buffer, offset, ref buffer, length);

            return true;
        }

        public bool EncodeHeaderRef(ref byte[] buffer, int offset)
        {
            rc4.codeBlock(buffer.AsSpan(offset, InputPacketBuffer.DefaultHeaderLength)
                , new ArraySegment<byte>(buffer, offset, InputPacketBuffer.DefaultHeaderLength));

            return true;
        }

        public byte[] Decode(byte[] buff)
        {
            return Decode(buff, 0, buff.Length);
        }

        public byte[] Decode(byte[] buff, int offset, int len)
        {
            byte[] res = new byte[len];
            DoCipher(ref buff, offset, ref res, len);
            return res;
        }


        public void Skip(int len)
        {
            this.SkipCipher(len);
        }

        private void SkipCipher(int len)
        {
            this.rc4.skipFor(len);
        }

        public void Clear()
        {
            rc4.init("RC4Sample");
        }

        private void TryCipher(ref byte[] pSource, int off, ref byte[] pTarget, int len)
        {
            RC4Cipher.State ss = new RC4Cipher.State
            {
                m_nBox = new byte[256]
            };
            rc4.m_state.m_nBox.CopyTo(ss.m_nBox, 0);
            ss.x = rc4.m_state.x;
            ss.y = rc4.m_state.y;

            rc4.codeBlock(pSource.AsSpan(off, len)
                , new ArraySegment<byte>(pTarget, off, len));

            ss.m_nBox.CopyTo(rc4.m_state.m_nBox, 0);
            rc4.m_state.x = ss.x;
            rc4.m_state.y = ss.y;

        }

        private void DoCipher(ref byte[] pSource, int off, ref byte[] pTarget, int len)
        {
            rc4.codeBlock(pSource.AsSpan(off, len)
                , new ArraySegment<byte>(pTarget, off, len));
        }

        public IPacketCipher CreateEntry()
        {
            return new XRC4Cipher(rc4.key);
        }

        RC4Cipher rc4;


        public bool Sync() => true;

        public void Dispose()
        {
            rc4?.Dispose();
        }
    }
}
