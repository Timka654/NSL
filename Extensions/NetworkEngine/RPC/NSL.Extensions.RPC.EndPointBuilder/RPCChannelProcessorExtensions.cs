using NSL.BuilderExtensions.SocketCore;
using NSL.EndPointBuilder;
using NSL.SocketCore.Utils;
using System;
using System.Diagnostics;

namespace NSL.Extensions.RPC.EndPointBuilder
{
    public static class RPCChannelProcessorExtensions
    {
        public static void RegisterRPCProcessor<TClient>(this IOptionableEndPointBuilder<TClient> builder)
            where TClient : INetworkClient, new()
        {
            builder.AddConnectHandle(_client =>
            {
                var proc = new RPCChannelProcessor<TClient>(_client, RPCChannelProcessor.DefaultCallPacketId, RPCChannelProcessor.DefaultResultPacketId);

                if (_client.ObjectBag == null)
                    throw new NullReferenceException($"First you must initialize ObjectBag for use RPC");

                _client.ObjectBag.Set(RPCChannelProcessor.DefaultBagKey, proc);
            });


            builder.AddPacketHandle(RPCChannelProcessor.DefaultResultPacketId, (c, p) =>
            {
                c.ObjectBag
                .Get<RPCChannelProcessor<TClient>>(RPCChannelProcessor.DefaultBagKey)
                .ResultPacketHandle(p);
            });


            builder.AddPacketHandle(RPCChannelProcessor.DefaultCallPacketId, (c, p) =>
            {
                c.ObjectBag
                .Get<RPCChannelProcessor<TClient>>(RPCChannelProcessor.DefaultBagKey)
                .CallPacketHandle(p);
            });
        }

        public static void AddRPCContainer<TClient, TContainer>(this IOptionableEndPointBuilder<TClient> builder, Func<TClient, TContainer> container)
            where TClient : INetworkClient, new()
            where TContainer : RPCHandleContainer<TClient>
        {
            builder.AddConnectHandle(_client =>
            {
                var c = container(_client);

                _client.ObjectBag
                .Get<RPCChannelProcessor<TClient>>(RPCChannelProcessor.DefaultBagKey)
                .RegisterContainer(c);

                RPCChannelProcessor.SetContainerClient(c, _client);
            });
        }

        public static void AddRPCContainer<TClient, TContainer>(this IOptionableEndPointBuilder<TClient> builder, TContainer container)
            where TClient : INetworkClient, new()
            where TContainer : RPCHandleContainer<TClient>
        {
            builder.AddConnectHandle(_client =>
            {
                RPCChannelProcessor.SetContainerClient(container, _client);
            });
        }
    }
}