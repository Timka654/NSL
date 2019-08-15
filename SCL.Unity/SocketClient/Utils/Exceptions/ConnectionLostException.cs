using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SCL.SocketClient.Utils.Exceptions
{
    /// <summary>
    ///Ошибка при чтении/записи данных
    /// </summary>
    public class ConnectionLostException : Exception
    {
        public ConnectionLostException(EndPoint ipep, bool receive) : base($"Cannot {(receive ? "receive" : "send")} packet data. Connection lost ({ipep})")
        {

        }
    }
}
