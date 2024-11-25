using System;
using System.Net.WebSockets;

namespace NSL.WebSockets
{
    public class WebSocketClosedException: Exception
    {
        public WebSocketClosedException(WebSocketCloseStatus? closeStatus, string closeStatusDescription)
        {
            CloseStatus = closeStatus;
            CloseStatusDescription = closeStatusDescription;
        }

        public WebSocketCloseStatus? CloseStatus { get; }
        public string? CloseStatusDescription { get; }
    }
}
