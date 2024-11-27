using System;

namespace NSL.SocketCore.Utils
{
    /// <summary>
    /// Интерфейс для реализации методов криптографии 
    /// </summary>
    public interface IPacketCipher : IDisposable
    {
        byte[] Peek(byte[] buffer);

        void Peek(ArraySegment<byte> buffer);

        byte[] Encode(byte[] buffer, int offset, int length);

        byte[] Decode(byte[] buffer, int offset, int length);

        void DecodeRef(ref byte[] buffer, int offset, int length);

        bool Sync();

        IPacketCipher CreateEntry();
    }
}
