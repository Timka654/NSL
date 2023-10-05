using System;
using System.Net;

namespace NSL.SocketCore.Utils.Exceptions
{
    public class ConnectionLostException : Exception
    {
        public bool Receive { get; }

        public ConnectionLostException(EndPoint ipep, bool receive, Exception innerException) : base($"Cannot {(receive ? "receive" : "send")} packet data. Connection lost ({ipep})", innerException)
        {
            this.Receive = receive;
        }

        public ConnectionLostException(EndPoint ipep, bool receive) : this(ipep, receive, null) { }
    }
}
