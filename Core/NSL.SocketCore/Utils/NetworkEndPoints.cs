using NSL.SocketCore.Utils;
using System.Net;

namespace NSL.SocketCore
{
    /// <summary>
    /// Convenience extensions on <see cref="CoreOptions"/> for network endpoints.
    /// Delegates to <see cref="NetworkEndPointObjectBagExtensions"/> on the options' ObjectBag.
    /// </summary>
    public static class CoreOptionsNetworkEndPointExtensions
    {
        private const string RemoteEndPointKey = "NSL.Network.RemoteEndPoint";
        private const string BindingEndPointKey = "NSL.Network.BindingEndPoint";

        public static TOptions WithRemoteEndPoint<TOptions>(this TOptions options, string ip, int port)
            where TOptions : CoreOptions
            => options.WithRemoteEndPoint(new NetworkRemoteEndPoint(ip, port));

        public static TOptions WithRemoteEndPoint<TOptions>(this TOptions options, NetworkRemoteEndPoint ep)
            where TOptions : CoreOptions
        {
            options.ObjectBag.Set(RemoteEndPointKey, ep);
            return options;
        }

        public static NetworkRemoteEndPoint GetRemoteEndPoint(this CoreOptions options)
            => options.ObjectBag.Get<NetworkRemoteEndPoint>(RemoteEndPointKey);

        public static TOptions WithBindingEndPoint<TOptions>(this TOptions options, string ip, int port, int backlog = 100)
            where TOptions : CoreOptions
            => options.WithBindingEndPoint(new NetworkBindingEndPoint(ip, port, backlog));

        public static TOptions WithBindingEndPoint<TOptions>(this TOptions options, NetworkBindingEndPoint ep)
            where TOptions : CoreOptions
        {
            options.ObjectBag.Set(BindingEndPointKey, ep);
            return options;
        }

        public static NetworkBindingEndPoint GetBindingEndPoint(this CoreOptions options)
            => options.ObjectBag.Get<NetworkBindingEndPoint>(BindingEndPointKey);
    }
}

namespace NSL.SocketCore.Utils
{
    /// <summary>
    /// Endpoint used by clients to identify the remote server (ip + port).
    /// </summary>
    public struct NetworkRemoteEndPoint
    {
        public string IpAddress;
        public int Port;

        public NetworkRemoteEndPoint(string ip, int port)
        {
            IpAddress = ip;
            Port = port;
        }

        public IPAddress GetIPAddress() => IPAddress.Parse(IpAddress);
        public IPEndPoint GetIPEndPoint() => new IPEndPoint(GetIPAddress(), Port);
    }

    /// <summary>
    /// Endpoint used by servers to define the local binding address (ip + port + backlog).
    /// </summary>
    public struct NetworkBindingEndPoint
    {
        public string IpAddress;
        public int Port;
        public int Backlog;

        public NetworkBindingEndPoint(string ip, int port, int backlog = 100)
        {
            IpAddress = ip;
            Port = port;
            Backlog = backlog;
        }

        public IPAddress GetIPAddress() => IPAddress.Parse(IpAddress);
        public IPEndPoint GetIPEndPoint() => new IPEndPoint(GetIPAddress(), Port);
    }
}
