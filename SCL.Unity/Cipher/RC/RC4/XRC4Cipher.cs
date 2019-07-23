using System;
using System.Collections.Generic;
using System.Text;
using SCL.SocketClient.Utils;

namespace SCL.Cipher.RC.RC4
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
            byte[] res = new byte[7];
            TryCipher(ref buff, 0, ref res, 7);
            return res;
        }

        public byte[] Peek(byte[] buff, int offset)
        {
            byte[] res = new byte[7];
            TryCipher(ref buff, offset, ref res, 7);
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

            rc4.code(ref pSource, off, ref pTarget, len);

            ss.m_nBox.CopyTo(rc4.m_state.m_nBox, 0);
            rc4.m_state.x = ss.x;
            rc4.m_state.y = ss.y;

        }

        private void DoCipher(ref byte[] pSource, int off, ref byte[] pTarget, int len)
        {
            rc4.code(ref pSource, off, ref pTarget, len);
        }

        public object Clone()
        {
            return new XRC4Cipher(rc4.key);
        }

        RC4Cipher rc4;

    }
}
