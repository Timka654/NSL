using System;

namespace NSL.SocketCore.Utils.Exceptions
{
    /// <summary>
    /// Ошибка хеша пакета
    /// </summary>
    public class InvalidPacketHashException : Exception
    {
        public InvalidPacketHashException() : base("Ошибка проверки хеша")
        {

        }
    }
}
