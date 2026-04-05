using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Threading.Tasks;

namespace NSL.SocketCore.Utils.Request
{
    public static class RequestExtensions
    {
        public static void AddResponsePacketHandle<TClient>(this CoreOptions<TClient> options, ushort packetId, Func<TClient, IResponsibleProcessor> handler)
            where TClient : INetworkClient
        {
            options.AddHandle(packetId, (client, packet) => handler(client).ProcessResponse(packet));
        }

        public static void AddResponsePacketHandle<TClient, TEnum>(this CoreOptions<TClient> options, TEnum packetId, Func<TClient, IResponsibleProcessor> handler)
            where TClient : INetworkClient, new()
            where TEnum : struct, IConvertible
        {
            options.AddResponsePacketHandle(packetId.ToUInt16(null), handler);
        }

        public delegate OutputPacketBuffer RequestPacketHandle<TClient>(TClient client, InputPacketBuffer data)
            where TClient : INetworkClient;

        public delegate bool RequestPacketHandle2<TClient>(TClient client, InputPacketBuffer data, OutputPacketBuffer response)
            where TClient : INetworkClient;

        public delegate Task<OutputPacketBuffer> RequestPacketAsyncHandle<TClient>(TClient client, InputPacketBuffer data)
            where TClient : INetworkClient;

        public delegate Task<bool> RequestPacketAsyncHandle2<TClient>(TClient client, InputPacketBuffer data, OutputPacketBuffer response)
            where TClient : INetworkClient;

        public static void AddRequestPacketHandle<TClient, TEnum>(this CoreOptions<TClient> builder, TEnum packetId, RequestPacketHandle<TClient> packet)
            where TClient : INetworkClient, new() where TEnum : struct, IConvertible
            => builder.AddRequestPacketHandle(packetId.ToUInt16(null), packet);

        public static void AddRequestPacketHandle<TClient, TEnum>(this CoreOptions<TClient> builder, TEnum packetId, RequestPacketHandle2<TClient> packet, ushort responsePacketId = 1)
            where TClient : INetworkClient, new() where TEnum : struct, IConvertible
            => builder.AddRequestPacketHandle(packetId.ToUInt16(null), packet, responsePacketId);

        public static void AddRequestPacketHandle<TClient>(this CoreOptions<TClient> builder, ushort packetId, RequestPacketHandle<TClient> packet)
            where TClient : INetworkClient, new()
        {
            builder.AddHandle(packetId, (client, data) =>
            {
                var result = packet.Invoke(client, data);
                if (result != null)
                    client.Send(result);
            });
        }

        public static void AddRequestPacketHandle<TClient>(this CoreOptions<TClient> builder, ushort packetId, RequestPacketHandle2<TClient> packet, ushort responsePacketId = 1)
            where TClient : INetworkClient
        {
            builder.AddHandle(packetId, (client, data) =>
            {
                using (var response = data.CreateResponse(responsePacketId))
                {
                    if (packet.Invoke(client, data, response))
                        client.Send(response);
                }
            });
        }

        public static void AddAsyncRequestPacketHandle<TClient, TEnum>(this CoreOptions<TClient> builder, TEnum packetId, RequestPacketAsyncHandle<TClient> packet)
            where TClient : INetworkClient, new() where TEnum : struct, IConvertible
        {
            builder.AddAsyncHandle(packetId.ToUInt16(null), async (client, data) =>
            {
                var result = await packet.Invoke(client, data);
                if (result != null)
                    client.Send(result);
            });
        }

        public static void AddAsyncRequestPacketHandle<TClient, TEnum>(this CoreOptions<TClient> builder, TEnum packetId, RequestPacketAsyncHandle2<TClient> packet, ushort responsePacketId = 1)
            where TClient : INetworkClient, new() where TEnum : struct, IConvertible
        {
            builder.AddAsyncHandle(packetId.ToUInt16(null), async (client, data) =>
            {
                using (var response = data.CreateResponse(responsePacketId))
                {
                    if (await packet.Invoke(client, data, response))
                        client.Send(response);
                }
            });
        }

        public static void ConfigureRequestProcessor<TClient, TEnum>(this CoreOptions<TClient> options, TEnum responsePacketId, string objectKey = RequestProcessor.DefaultObjectBagKey)
            where TClient : INetworkClient, new()
            where TEnum : struct, IConvertible
        {
            options.OnClientConnectEvent += client => CreateRequestProcessor(client, objectKey);
            options.AddResponsePacketHandle(responsePacketId, c => c.GetRequestProcessor(objectKey));
        }

        public static void ConfigureRequestProcessor<TClient>(this CoreOptions<TClient> options, ushort responsePacketId = RequestProcessor.DefaultResponsePacketId, string objectKey = RequestProcessor.DefaultObjectBagKey)
            where TClient : INetworkClient, new()
        {
            options.OnClientConnectEvent += client => CreateRequestProcessor(client, objectKey);
            options.AddResponsePacketHandle(responsePacketId, c => c.GetRequestProcessor(objectKey));
        }

        public static void SetDefaultResponsePID<TClient>(this CoreOptions<TClient> options, ushort responsePacketId = RequestProcessor.DefaultResponsePacketId)
            where TClient : INetworkClient, new()
        {
            options.ObjectBag[RequestProcessor.DefaultResponsePIDObjectBagKey] = responsePacketId;
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
            return client.ObjectBag.Get<RequestProcessor>(objectKey, throwIfNotExists);
        }
    }
}
