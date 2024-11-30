using NSL.Logger.Interface;
using NSL.Logger;
using System;
using System.Collections.Concurrent;
using NSL.Utils;
using NSL.EndPointBuilder;
using NSL.SocketServer.Utils;
using NSL.UDP;
using NSL.SocketCore.Utils.Buffer;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;
using NSL.SocketCore.Utils.Logger;
using NSL.Node.P2Proxy.Proxy.Data;
using NSL.BuilderExtensions.UDPServer;
using NSL.BuilderExtensions.SocketCore;

namespace NSL.Node.P2Proxy.Client
{
    public partial class P2ProxyServerEntry
    {
        protected INetworkListener Listener { get; private set; }

        protected NodeP2PProxyEntry Entry { get; }

        protected IBasicLogger Logger { get; }

        public static P2ProxyServerEntry Create(
            NodeP2PProxyEntry entry,
            string logPrefix = "[P2Proxy]")
            => new P2ProxyServerEntry(entry, logPrefix);

        public P2ProxyServerEntry(NodeP2PProxyEntry entry, string logPrefix = "[P2Proxy]")
        {
            Entry = entry;

            if (Entry.Logger != null)
                Logger = new PrefixableLoggerProxy(Entry.Logger, logPrefix);
        }

        public P2ProxyServerEntry Run()
        {
            //string bindingPoint = ClientBindingPoint;

            //if (bindingPoint == default)
            //    bindingPoint = $"udp://0.0.0.0:{ClientBindingPort}/";

            //var p = NSLEndPoint.FromUrl(bindingPoint);

            //Listener = Fill(UDPServerEndPointBuilder.Create()
            //    .WithClientProcessor<P2PNetworkClient>()
            //    .WithOptions<UDPClientOptions<P2PNetworkClient>>()
            //    .WithBindingPoint(p.Address, p.Port))
            //    .Build();

            //Listener.Start();

            return this;
        }

        private TBuilder Fill<TBuilder>(TBuilder builder)
            where TBuilder : IOptionableEndPointBuilder<P2PNetworkClient>, IHandleIOBuilder<P2PNetworkClient>
        {
            builder.SetLogger(Logger);

            builder.AddPacketHandle(
                RoomPacketEnum.SignSessionRequest, SignInPacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.TransportMessage, TransportPacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.BroadcastMessage, BroadcastPacketHandle);

            return builder;
        }

        private ConcurrentDictionary<string, Lazy<ProxyRoomInfo>> roomMap = new();
    }
}
