using NSL.SocketCore.Utils;

namespace NSL.SocketClient.Utils.Session
{
    public class NSLSessionClientOptions
    {
        public const string ObjectBagKey       = NSLObjectBagKeys.SessionClientOptions;
        public const string DefaultSessionBagKey = NSLObjectBagKeys.SessionInfo;

        public string ClientSessionBagKey { get; set; } = DefaultSessionBagKey;
    }
}
