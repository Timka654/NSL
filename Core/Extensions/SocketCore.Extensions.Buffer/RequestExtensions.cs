using NSL.SocketCore.Extensions.Buffer.Interface;
using NSL.SocketCore.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.SocketCore.Extensions.Buffer
{
    public static class RequestExtensions
    {
        /// <summary>
        /// Register handle for execute wait receive packet in buffer
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="packetId"></param>
        /// <param name="handler"></param>
        public static void AddReceivePacketHandle<TClient>(this CoreOptions<TClient> options, ushort packetId, Func<TClient, IResponsibleProcessor> handler)
            where TClient : INetworkClient, new()
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
        public static void AddReceivePacketHandle<TClient, TEnum>(this CoreOptions<TClient> options, TEnum packetId, Func<TClient, IResponsibleProcessor> handler)
            where TClient : INetworkClient, new()
            where TEnum : struct, IConvertible
        {
            options.AddReceivePacketHandle(packetId.ToUInt16(null), handler);
        }
    }
}
