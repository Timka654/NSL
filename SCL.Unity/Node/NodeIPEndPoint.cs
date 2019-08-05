using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SCL.Unity.Node
{
    class NodeIPEndPoint
    {
        public string Ip { get; private set; }

        public int Port { get; private set; }

        public bool Proxy { get; private set; }

        public int ProxyServerId { get; private set; }

        public string ProxyConnectionToken { get; private set; }

        public NodeIPEndPoint(string ip, int port)
        {
            if (ip.StartsWith("proxy:"))
            {
                ip = ip.Replace("proxy:", "");
                var proxySettings = ip.Split('$');

                ProxyServerId = Convert.ToInt32(proxySettings[0]);
                ProxyConnectionToken = proxySettings[1];

                Proxy = true;
            }

            Ip = ip;
            Port = port;
        }

        public IPEndPoint GetIPEP()
        {
            return new IPEndPoint(IPAddress.Parse(Ip), Port);
        }
    }
}
