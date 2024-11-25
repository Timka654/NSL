namespace NSL.Extensions.Session.Client
{

    public class NSLSessionClientOptions
    {
        public const string ObjectBagKey = "NSL__SESSION__CLIENTOPTIONS";

        public const string DefaultSessionBagKey = "NSL__SESSION__INFO";

        public string ClientSessionBagKey { get; set; } = DefaultSessionBagKey;
    }
}
