namespace NSL.WebSockets.UnityClient
{
    public class WGLWSNetworkClient<T, TOptions> : WSNetworkClient<T, TOptions>
        where T : BaseSocketNetworkClient, new()
        where TOptions : WSClientOptions<T>
    {
        public WGLWSNetworkClient(TOptions options) : base(options)
        {
        }


        protected override WebSocket CreateWS()
        {
            return new WGLWebSocket();
        }

        protected override async Task ConnectAsync(Uri endPoint, CancellationToken cts)
        {
            await ((WGLWebSocket)client).ConnectAsync(endPoint, cts);
        }
    }
}
