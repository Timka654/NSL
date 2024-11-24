using System;

namespace NSL.SocketCore.Utils
{
    /// <summary>
    /// Интерфейс для реализации методов криптографии 
    /// </summary>
    public interface IPacketCipher : IDisposable
    {
        /// <summary>
        /// Дешифровка заголовка пакета
        /// </summary>
        /// <param name="buffer">буффер с данными</param>
        /// <returns>Буффер с расшифрованным хедером пакета</returns>
        byte[] Peek(byte[] buffer);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer">буффер с данными</param>
        /// <param name="offset">позиция в буффере с которой нужно начинать чтение</param>
        /// <param name="length">размер данных </param>
        /// <returns>Буффер с расшифрованным хедером пакета</returns>
        byte[] Encode(byte[] buffer, int offset, int length);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer">буффер с данными</param>
        /// <param name="offset">позиция в буффере с которой нужно начинать чтение</param>
        /// <param name="length"></param>
        /// <returns>Буффер с расшифрованным хедером пакета</returns>
        byte[] Decode(byte[] buffer, int offset, int length);

        bool Sync();

        IPacketCipher CreateEntry();
    }
}
