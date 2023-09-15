using NSL.Extensions.Session;
using NSL.SocketCore.Utils;
using System;

namespace NSL.Extensions.Session.Server
{
    public class NSLServerSessionInfo<T> : NSLSessionInfo
        where T : INetworkClient
    {
        public DateTime? DisconnectTime { get; set; }

        public T Client { get; set; }

        public NSLServerSessionInfo(T client, string[] restoreKeys) : base(restoreKeys)
        {
            Client = client;
            RestoreKeys = restoreKeys;
        }
    }
}
