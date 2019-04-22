using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer.Utils
{
    /// <summary>
    /// Интерфейс для реализации методов криптографии 
    /// </summary>
    public interface IPacketCipher : ICloneable
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
        /// <param name="lenght">размер данных </param>
        /// <returns>Буффер с расшифрованным хедером пакета</returns>
        byte[] Encode(byte[] buffer, int offset, int lenght);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer">буффер с данными</param>
        /// <param name="offset">позиция в буффере с которой нужно начинать чтение</param>
        /// <param name="lenght"></param>
        /// <returns>Буффер с расшифрованным хедером пакета</returns>
        byte[] Decode(byte[] buffer, int offset, int lenght);
    }
}
