using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Threading.Tasks;

namespace NSL.SocketCore.Utils.Request
{
    public static class RequestExtensions
    {
        public static void AddResponsePacketHandle(this CoreOptions options, ushort packetId, Func<BaseNetworkConnection, IResponsibleProcessor> handler)
        {
            options.AddHandle(packetId, (client, packet) => handler(client).ProcessResponse(packet));
        }

        public static void AddResponsePacketHandle<TEnum>(this CoreOptions options, TEnum packetId, Func<BaseNetworkConnection, IResponsibleProcessor> handler)
            where TEnum : struct, IConvertible
        {
            options.AddResponsePacketHandle(packetId.ToUInt16(null), handler);
        }

        public delegate OutputPacketBuffer RequestPacketHandle(BaseNetworkConnection client, InputPacketBuffer data);

        public delegate bool RequestPacketHandle2(BaseNetworkConnection client, InputPacketBuffer data, OutputPacketBuffer response);

        public delegate Task<OutputPacketBuffer> RequestPacketAsyncHandle(BaseNetworkConnection client, InputPacketBuffer data);

        public delegate Task<bool> RequestPacketAsyncHandle2(BaseNetworkConnection client, InputPacketBuffer data, OutputPacketBuffer response);

        public static void AddRequestPacketHandle<TEnum>(this CoreOptions builder, TEnum packetId, RequestPacketHandle packet)
            where TEnum : struct, IConvertible
            => builder.AddRequestPacketHandle(packetId.ToUInt16(null), packet);

        public static void AddRequestPacketHandle<TEnum>(this CoreOptions builder, TEnum packetId, RequestPacketHandle2 packet, ushort responsePacketId = 1)
            where TEnum : struct, IConvertible
            => builder.AddRequestPacketHandle(packetId.ToUInt16(null), packet, responsePacketId);

        public static void AddRequestPacketHandle(this CoreOptions builder, ushort packetId, RequestPacketHandle packet)
        {
            builder.AddHandle<BaseNetworkConnection>(packetId, (client, data) =>
            {
                var result = packet.Invoke(client, data);
                if (result != null)
                    client.Send(result);
            });
        }

        public static void AddRequestPacketHandle(this CoreOptions builder, ushort packetId, RequestPacketHandle2 packet, ushort responsePacketId = 1)
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

        public static void AddAsyncRequestPacketHandle<TEnum>(this CoreOptions builder, TEnum packetId, RequestPacketAsyncHandle packet)
            where TEnum : struct, IConvertible
        {
            builder.AddAsyncHandle(packetId.ToUInt16(null), async (client, data) =>
            {
                var result = await packet.Invoke(client, data);
                if (result != null)
                    client.Send(result);
            });
        }

        public static void AddAsyncRequestPacketHandle<TEnum>(this CoreOptions builder, TEnum packetId, RequestPacketAsyncHandle2 packet, ushort responsePacketId = 1)
            where TEnum : struct, IConvertible
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

        public static void ConfigureRequestProcessor<TEnum>(this CoreOptions options, TEnum responsePacketId, string objectKey = RequestProcessor.DefaultObjectBagKey)
            where TEnum : struct, IConvertible
        {
            options.OnClientConnectEvent += client => CreateRequestProcessor(client, objectKey);
            options.AddResponsePacketHandle<TEnum>(responsePacketId, c => c.GetRequestProcessor(objectKey));
        }

        public static void ConfigureRequestProcessor(this CoreOptions options, ushort responsePacketId = RequestProcessor.DefaultResponsePacketId, string objectKey = RequestProcessor.DefaultObjectBagKey)
        {
            options.OnClientConnectEvent += client => CreateRequestProcessor(client, objectKey);
            options.AddResponsePacketHandle(responsePacketId, c => c.GetRequestProcessor(objectKey));
        }

        public static void SetDefaultResponsePID(this CoreOptions options, ushort responsePacketId = RequestProcessor.DefaultResponsePacketId)
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

        public static RequestProcessor CreateRequestProcessor(this BaseNetworkConnection client, string objectKey = RequestProcessor.DefaultObjectBagKey)
        {
            client.ThrowIfObjectBagNull();
            var requestProcessor = new RequestProcessor(client);
            client.ObjectBag.Set(objectKey, requestProcessor);
            return requestProcessor;
        }

        public static RequestProcessor GetRequestProcessor(this BaseNetworkConnection client, string objectKey = RequestProcessor.DefaultObjectBagKey, bool throwIfNotExists = true)
        {
            client.ThrowIfObjectBagNull();
            return client.ObjectBag.Get<RequestProcessor>(objectKey, throwIfNotExists);
        }
    }
}
