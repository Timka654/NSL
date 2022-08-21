﻿using System;
using System.Net;

namespace NSL.SocketCore.Utils.Exceptions
{
    public class ConnectionLostException : Exception
    {
        public ConnectionLostException(EndPoint ipep, bool receive) : base($"Cannot {(receive ? "receive" : "send")} packet data. Connection lost ({ipep})")
        {

        }
    }
}