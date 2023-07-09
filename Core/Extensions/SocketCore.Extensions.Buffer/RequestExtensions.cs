using NSL.SocketCore.Extensions.Buffer.Interface;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;

namespace NSL.SocketCore.Extensions.Buffer
{
    public static class RequestExtensions
    {
        [Obsolete("Renamed to AddResponsePacketHandle", true)]
        public static void AddReceivePacketHandle<TClient>(this CoreOptions<TClient> options, ushort packetId, Func<TClient, IResponsibleProcessor> handler)
            where TClient : INetworkClient, new()
            => AddResponsePacketHandle(options, packetId, handler);


        [Obsolete("Renamed to AddResponsePacketHandle", true)]
        public static void AddReceivePacketHandle<TClient, TEnum>(this CoreOptions<TClient> options, TEnum packetId, Func<TClient, IResponsibleProcessor> handler)
            where TClient : INetworkClient, new()
            where TEnum : struct, IConvertible
            => AddResponsePacketHandle(options, packetId, handler);

        /// <summary>
        /// Register handle for execute wait receive packet in buffer
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="packetId"></param>
        /// <param name="handler"></param>
        public static void AddResponsePacketHandle<TClient>(this CoreOptions<TClient> options, ushort packetId, Func<TClient, IResponsibleProcessor> handler)
            where TClient : INetworkClient
        {
            options.AddHandle(packetId, (client, packet) => handler(client).ProcessResponse(packet));
        }

        /// <summary>
        /// Register handle for execute wait receive packet in buffer
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="client"></param>
        /// <param name="packetId"></param>
        /// <param name="handler"></param>
        public static void AddResponsePacketHandle<TClient, TEnum>(this CoreOptions<TClient> options, TEnum packetId, Func<TClient, IResponsibleProcessor> handler)
            where TClient : INetworkClient, new()
            where TEnum : struct, IConvertible
        {
            options.AddResponsePacketHandle(packetId.ToUInt16(null), handler);
        }

        /// <summary>
        /// Create <see cref="RequestProcessor"/> with <paramref name="objectKey"/> in Client.ObjectBag and register handle for execute wait receive packet in buffer
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="options"></param>
        /// <param name="packetId"></param>
        /// <param name="objectKey"></param>
        public static void ConfigureRequestProcessor<TClient, TEnum>(this CoreOptions<TClient> options, TEnum responsePacketId, string objectKey = RequestProcessor.DefaultObjectBagKey)
            where TClient : INetworkClient, new()
            where TEnum : struct, IConvertible
        {
            options.OnClientConnectEvent += client =>
            {
                CreateRequestProcessor(client, objectKey);
            };

            options.AddResponsePacketHandle(responsePacketId, c => c.GetRequestProcessor(objectKey));
        }

        /// <summary>
        /// Create <see cref="RequestProcessor"/> with <paramref name="objectKey"/> in Client.ObjectBag and register handle for execute wait receive packet in buffer
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="options"></param>
        /// <param name="packetId"></param>
        /// <param name="objectKey"></param>
        public static void ConfigureRequestProcessor<TClient>(this CoreOptions<TClient> options, ushort responsePacketId = RequestProcessor.DefaultResponsePacketId, string objectKey = RequestProcessor.DefaultObjectBagKey)
            where TClient : INetworkClient, new()
        {
            options.OnClientConnectEvent += client =>
            {
                CreateRequestProcessor(client, objectKey);
            };

            options.AddResponsePacketHandle(responsePacketId, c => c.GetRequestProcessor(objectKey));
        }

        public static OutputPacketBuffer CreateResponse<TEnum>(this InputPacketBuffer data, TEnum packetId)
            where TEnum : struct, Enum, IConvertible
            => data.CreateWaitBufferResponse().WithPid(packetId);

        public static OutputPacketBuffer CreateResponse(this InputPacketBuffer data, ushort packetId = RequestProcessor.DefaultResponsePacketId)
        {
            var response = data.CreateWaitBufferResponse();

            response.PacketId = packetId;

            return response;
        }

        /// <summary>
        /// Create <see cref="RequestProcessor"/> with <paramref name="objectKey"/> and return this
        /// </summary>
        /// <param name="client"></param>
        /// <param name="objectKey"></param>
        /// <returns></returns>
        public static RequestProcessor CreateRequestProcessor<TClient>(this TClient client, string objectKey = RequestProcessor.DefaultObjectBagKey)
            where TClient : INetworkClient
        {
            client.ThrowIfObjectBagNull();

            var requestProcessor = new RequestProcessor(client);

            client.ObjectBag.Set(objectKey, requestProcessor);

            return requestProcessor;
        }

        public static RequestProcessor GetRequestProcessor<TClient>(this TClient client, string objectKey = RequestProcessor.DefaultObjectBagKey, bool throwIfNotExists = true)
            where TClient : INetworkClient
        {
            client.ThrowIfObjectBagNull();

            var requestProcessor = client.ObjectBag.Get<RequestProcessor>(objectKey, throwIfNotExists);

            return requestProcessor;
        }
    }
}
