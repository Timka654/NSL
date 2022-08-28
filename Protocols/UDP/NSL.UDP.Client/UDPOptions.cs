using NSL.SocketClient;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System.Net;

namespace NSL.UDP.Client
{
    //public class UDPOptions<TClient> : ClientOptions<TClient>
    //    where TClient : BaseSocketNetworkClient
    //{ 

    //}

    public interface IBindingUDPOptions
    {
        string BindingIP { get; set; }
        int BindingPort { get; set; }

        IPAddress GetBindingIPAddress();
        IPEndPoint GetBindingIPEndPoint();
    }


    public class UDPClientOptions<TClient> : ClientOptions<TClient>, IBindingUDPOptions
        where TClient : BaseSocketNetworkClient
    {
        public string BindingIP { get; set; }

        public int BindingPort { get; set; }

        public IPAddress GetBindingIPAddress() => IPAddress.Parse(BindingIP);

        public IPEndPoint GetBindingIPEndPoint() => new IPEndPoint(GetBindingIPAddress(), BindingPort);
    }

    public class UDPServerOptions<TClient> : ServerOptions<TClient>, IBindingUDPOptions
        where TClient : IServerNetworkClient
    {
        public string BindingIP { get; set; }

        public int BindingPort { get; set; }

        public IPAddress GetBindingIPAddress() => IPAddress.Parse(BindingIP);

        public IPEndPoint GetBindingIPEndPoint() => new IPEndPoint(GetBindingIPAddress(), BindingPort);
    }
}
