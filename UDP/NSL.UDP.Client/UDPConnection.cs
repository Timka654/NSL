using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NSL.UDP.Client
{
    public class UDPConnection<TClient> : UDPListener<TClient>
        where TClient : IServerNetworkClient, new()
    {
        UDPClient<TClient> client;

        public UDPClient<TClient> GetClient() => client;

        public UDPConnection(UDPOptions<TClient> options) : base(options)
        {
        }

        public void Connect()
        {
            StartReceive(() => {
                client = new UDPClient<TClient>(options.GetIPEndPoint(), listener, options);
            });
        }

        protected override void Args_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (!state)
                return;

            RunReceiveAsync();

            if (e.RemoteEndPoint.Equals(options.GetIPEndPoint()))
                client.Receive(e);
            else
                e.Dispose();
        }
    }
}
