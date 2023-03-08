using System;
using System.Net;

namespace NSL.Utils
{
    public class NSLEndPoint
    {
        public enum Type
        {
            Unknown,
            TCP,
            UDP,
            WS
        }

        public Type ProtocolType { get; private set; } = Type.Unknown;

        public string EndPoint { get; private set; }

        public string Address { get; private set; }

        public int Port { get; private set; }

        private NSLEndPoint() { }

        public NSLEndPoint(string endPoint)
        {
            this.EndPoint = endPoint;

            var uri = new Uri(endPoint);

            ProtocolType = (Type)Enum.Parse(typeof(Type),uri.Scheme, true);

            Address = uri.Host;

            Port = uri.Port;
        }

        public static NSLEndPoint FromUrl(string url)
            => new NSLEndPoint(url);

        public static NSLEndPoint FromIPAddress(Type protocolType, IPAddress address, int port)
            => new NSLEndPoint($"{Enum.GetName(typeof(Type), protocolType)}://{address}:{port}");

        public static NSLEndPoint FromDomain(Type protocolType, string address, int port)
            => new NSLEndPoint($"{Enum.GetName(typeof(Type), protocolType)}://{address}:{port}");

        public static NSLEndPoint Parse(string endPoint) => new NSLEndPoint(endPoint);

        public override string ToString()
            => EndPoint;
    }
}
