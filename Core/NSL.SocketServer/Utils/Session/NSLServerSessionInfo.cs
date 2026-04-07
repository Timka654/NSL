using NSL.SocketCore.Network.Session;
using NSL.SocketCore.Utils;
using NSL.SocketServer.Utils;
using System;

namespace NSL.SocketServer.Utils.Session
{
    public class NSLServerSessionInfo<T> : NSLSessionInfo where T : BaseNetworkConnection
    {
        public DateTime? DisconnectTime { get; set; }
        public T         Client         { get; set; }

        public NSLServerSessionInfo(T client, string[] restoreKeys) : base(restoreKeys)
        {
            Client      = client;
            RestoreKeys = restoreKeys;
        }
    }
}
