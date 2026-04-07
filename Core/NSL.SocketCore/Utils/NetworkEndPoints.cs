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
        public static TOptions WithRemoteEndPoint<TOptions>(this TOptions options, string ip, int port)
            where TOptions : CoreOptions
        {
            options.ObjectBag.SetRemoteEndPoint(ip, port);
            return options;
        }

        public static NetworkRemoteEndPoint GetRemoteEndPoint(this CoreOptions options)
            => options.ObjectBag.GetRemoteEndPoint();

        public static TOptions WithBindingEndPoint<TOptions>(this TOptions options, string ip, int port, int backlog = 100)
            where TOptions : CoreOptions
        {
            options.ObjectBag.SetBindingEndPoint(ip, port, backlog);
            return options;
        }

        public static NetworkBindingEndPoint GetBindingEndPoint(this CoreOptions options)
            => options.ObjectBag.GetBindingEndPoint();
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

    /// <summary>
    /// Extension methods for storing/retrieving <see cref="NetworkRemoteEndPoint"/> and
    /// <see cref="NetworkBindingEndPoint"/> in an <see cref="ObjectBag"/>.
    /// </summary>
    public static class NetworkEndPointObjectBagExtensions
    {
        private const string RemoteEndPointKey = "NSL.Network.RemoteEndPoint";
        private const string BindingEndPointKey = "NSL.Network.BindingEndPoint";

        public static ObjectBag SetRemoteEndPoint(this ObjectBag bag, string ip, int port)
            => bag.SetRemoteEndPoint(new NetworkRemoteEndPoint(ip, port));

        public static ObjectBag SetRemoteEndPoint(this ObjectBag bag, NetworkRemoteEndPoint ep)
        {
            bag.Set(RemoteEndPointKey, ep);
            return bag;
        }

        public static NetworkRemoteEndPoint GetRemoteEndPoint(this ObjectBag bag)
            => bag.Get<NetworkRemoteEndPoint>(RemoteEndPointKey);

        public static ObjectBag SetBindingEndPoint(this ObjectBag bag, string ip, int port, int backlog = 100)
            => bag.SetBindingEndPoint(new NetworkBindingEndPoint(ip, port, backlog));

        public static ObjectBag SetBindingEndPoint(this ObjectBag bag, NetworkBindingEndPoint ep)
        {
            bag.Set(BindingEndPointKey, ep);
            return bag;
        }

        public static NetworkBindingEndPoint GetBindingEndPoint(this ObjectBag bag)
            => bag.Get<NetworkBindingEndPoint>(BindingEndPointKey);
    }
}
