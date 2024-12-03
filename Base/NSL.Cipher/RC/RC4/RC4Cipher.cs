using System;
using System.Text;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.Cipher.RC.RC4
{
    public class RC4Cipher : IPacketCipher
    {
        public string key;
        public RC4Cipher()
        {
        }

        public bool init(String pKey)
        {
            key = pKey;
            return prepareKey(pKey);
        }

        public struct State
        {
            public byte[] m_nBox;
            public int x, y;
        };

        public void saveStateTo(ref State outState)
        {
            outState = m_state;
        }

        public void loadStateFrom(State aState)
        {
            m_state = aState;
        }

        bool prepareKey(String pKey)
        {
            if (String.IsNullOrEmpty(pKey))
                return false;

            byte[] asciiBytes = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, Encoding.Unicode.GetBytes(pKey));

            long KeyLen = pKey.Length;

            m_state.m_nBox = new byte[256];
            for (long count = 0; count < 256; count++)
            {
                m_state.m_nBox[count] = (byte)count;
            }

            long index2 = 0;

            for (long count = 0; count < 256; count++)
            {
                index2 = (index2 + m_state.m_nBox[index2] + asciiBytes[index2 % KeyLen]) % 256;
                byte temp = m_state.m_nBox[count];
                m_state.m_nBox[count] = m_state.m_nBox[index2];
                m_state.m_nBox[index2] = temp;
            }

            m_state.x = m_state.y = 0;

            skipFor(1013);

            return true;
        }

        public void skipFor(int len)
        {
            long i = m_state.x;
            long j = m_state.y;

            for (long offset = 0; offset < len; offset++)
            {
                i = (i + 1) % 256;
                j = (j + m_state.m_nBox[i]) % 256;
                byte temp = m_state.m_nBox[i];
                m_state.m_nBox[i] = m_state.m_nBox[j];
                m_state.m_nBox[j] = temp;
            }

            m_state.x = (int)i;
            m_state.y = (int)j;
        }

        public void codeBlock(Span<byte> pSrc, ArraySegment<byte> pDst)
        {
            long i = m_state.x;
            long j = m_state.y;

            for (int offset = 0; offset < pSrc.Length; offset++)
            {
                i = (i + 1) % 256;
                j = (j + m_state.m_nBox[i]) % 256;
                byte temp = m_state.m_nBox[i];
                m_state.m_nBox[i] = m_state.m_nBox[j];
                m_state.m_nBox[j] = temp;
                byte a = pSrc[offset];
                byte b = m_state.m_nBox[(m_state.m_nBox[i] + m_state.m_nBox[j]) % 256];
                pDst[offset] = (byte)(a ^ b);
            }

            m_state.x = (int)i;
            m_state.y = (int)j;
        }

        public State m_state;

        public bool DecodeRef(ref byte[] buffer, int offset, int length)
        {
            codeBlock(buffer.AsSpan(offset, length)
            , new ArraySegment<byte>(buffer, offset, length));

            return true;
        }

        public bool DecodeHeaderRef(ref byte[] buffer, int offset)
        {
            codeBlock(buffer.AsSpan(offset, InputPacketBuffer.DefaultHeaderLength)
            , new ArraySegment<byte>(buffer, offset, InputPacketBuffer.DefaultHeaderLength));

            return true;
        }

        public bool EncodeRef(ref byte[] buffer, int offset, int length)
        {
            codeBlock(buffer.AsSpan(offset, length)
            , new ArraySegment<byte>(buffer, offset, length));

            return true;
        }

        public bool EncodeHeaderRef(ref byte[] buffer, int offset)
        {
            codeBlock(buffer.AsSpan(offset, InputPacketBuffer.DefaultHeaderLength)
            , new ArraySegment<byte>(buffer, offset, InputPacketBuffer.DefaultHeaderLength)); 
            
            return true;
        }

        public byte[] Decode(byte[] buffer, int offset, int length)
        {
            byte[] dest = new byte[length];
            codeBlock(buffer.AsSpan(offset, length), dest);

            return dest;
        }

        public byte[] Encode(byte[] buffer, int offset, int length)
        {
            byte[] dest = new byte[length];
            codeBlock(buffer.AsSpan(offset, length), dest);

            return dest;
        }

        public byte[] Peek(byte[] buffer)
        {
            byte[] dest = new byte[InputPacketBuffer.DefaultHeaderLength];
            codeBlock(buffer.AsSpan(0, InputPacketBuffer.DefaultHeaderLength), dest);

            return dest;
        }

        public void Peek(ArraySegment<byte> buffer)
        {
            throw new NotImplementedException();
        }

        private RC4Cipher(string key)
        {
            init(key);
        }

        public IPacketCipher CreateEntry()
        {
            return new RC4Cipher(key);
        }

        public bool Sync() => true;

        public void Dispose()
        {
        }
    }
}
