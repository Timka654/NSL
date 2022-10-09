using NSL.BuilderExtensions.SocketCore;
using NSL.EndPointBuilder;
using NSL.SocketCore.Utils;
using System;

namespace NSL.Extensions.RPC.EndPointBuilder
{
    public static class RPCChannelProcessorExtensions
    {
        /// <summary>
        /// Previously call InitializeObjectBag method for each client in sync connection handle for normal work
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="builder"></param>
        /// <param name="callPID"></param>
        /// <param name="resultPid"></param>
        /// <exception cref="NullReferenceException"></exception>
        public static void RegisterRPCProcessor<TClient>(this IOptionableEndPointBuilder<TClient> builder, ushort callPID = RPCChannelProcessor.DefaultCallPacketId, ushort resultPid = RPCChannelProcessor.DefaultResultPacketId)
            where TClient : INetworkClient, new()
        {
            builder.AddConnectHandle(_client =>
            {
                var proc = new RPCChannelProcessor<TClient>(_client, callPID, resultPid);

                if (_client.ObjectBag == null)
                    throw new NullReferenceException($"First you must initialize ObjectBag for use RPC");

                _client.ObjectBag.Set(RPCChannelProcessor.DefaultBagKey, proc);
            });


            builder.AddPacketHandle(resultPid, (c, p) =>
            {
                c.ObjectBag
                .Get<RPCChannelProcessor<TClient>>(RPCChannelProcessor.DefaultBagKey)
                .ResultPacketHandle(p);
            });


            builder.AddPacketHandle(callPID, (c, p) =>
            {
                c.ObjectBag
                .Get<RPCChannelProcessor<TClient>>(RPCChannelProcessor.DefaultBagKey)
                .CallPacketHandle(p);
            });
        }
        /// <summary>
        /// Previously call InitializeObjectBag method for each client in sync connection handle for normal work
        /// Previously call RegisterRPCProcessor method for each client in sync connection handle for normal work
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <typeparam name="TContainer"></typeparam>
        /// <param name="builder"></param>
        /// <param name="container"></param>
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

        /// <summary>
        /// Previously call InitializeObjectBag method for each client in sync connection handle for normal work
        /// Previously call RegisterRPCProcessor method for each client in sync connection handle for normal work
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <typeparam name="TContainer"></typeparam>
        /// <param name="builder"></param>
        /// <param name="container"></param>
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