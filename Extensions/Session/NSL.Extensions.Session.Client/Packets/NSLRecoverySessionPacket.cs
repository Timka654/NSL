﻿using NSL.SocketClient;
using NSL.SocketCore.Extensions.Buffer;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Extensions.Session.Client.Packets
{
    public static class NSLRecoverySessionPacketExtension
    {
        public static async Task<NSLRecoverySessionResult> NSLSessionSendRequestAsync<TClient>(this TClient client
            , CancellationToken cancellationToken
            , string SOObjectKey = NSLSessionClientOptions.ObjectBagKey
            , string RPObjectKey = RequestProcessor.DefaultObjectBagKey)
            where TClient : BaseSocketNetworkClient
        {
            client.ThrowIfObjectBagNull();

            var options = client.GetNSLSessionOptions(SOObjectKey);

            if (options == null)
                throw new Exception($"ObjectBag not contains session options item {SOObjectKey}");

            return await client.NSLSessionSendRequestAsync(options, cancellationToken, RPObjectKey);
        }

        public static void NSLSessionSendRequest<TClient>(this TClient client
            , Action<NSLRecoverySessionResult> onResponse, CancellationToken cancellationToken
            , string SOObjectKey = NSLSessionClientOptions.ObjectBagKey
            , string RPObjectKey = RequestProcessor.DefaultObjectBagKey)
            where TClient : BaseSocketNetworkClient
        {
            client.ThrowIfObjectBagNull();

            var options = client.GetNSLSessionOptions(SOObjectKey);

            if (options == null)
                throw new Exception($"ObjectBag not contains session options item {SOObjectKey}");

            client.NSLSessionSendRequest(onResponse, options, cancellationToken, RPObjectKey);
        }

        public static async Task<NSLRecoverySessionResult> NSLSessionSendRequestAsync<TClient>(this TClient client, NSLSessionClientOptions sessionOptions, CancellationToken cancellationToken, string RPObjectKey = RequestProcessor.DefaultObjectBagKey)
            where TClient : BaseSocketNetworkClient
        {
            client.ThrowIfObjectBagNull();

            var info = client.GetNSLSessionInfo(sessionOptions);

            if (info == null)
                throw new Exception($"ObjectBag not contains session info item {sessionOptions.ClientSessionBagKey}");

            return await client.NSLSessionSendRequestAsync(info, cancellationToken, RPObjectKey);

        }

        public static void NSLSessionSendRequest<TClient>(this TClient client, Action<NSLRecoverySessionResult> onResponse, NSLSessionClientOptions sessionOptions, CancellationToken cancellationToken, string RPObjectKey = RequestProcessor.DefaultObjectBagKey)
            where TClient : BaseSocketNetworkClient
        {
            client.ThrowIfObjectBagNull();

            var info = client.GetNSLSessionInfo(sessionOptions);

            if (info == null)
                throw new Exception($"ObjectBag not contains session info item {sessionOptions.ClientSessionBagKey}");

            client.NSLSessionSendRequest(onResponse, info, cancellationToken, RPObjectKey);
        }

        public static async Task<NSLRecoverySessionResult> NSLSessionSendRequestAsync<TClient>(this TClient client, NSLSessionInfo sessionInfo, CancellationToken cancellationToken, string RPObjectKey = RequestProcessor.DefaultObjectBagKey)
            where TClient : BaseSocketNetworkClient
        {
            client.ThrowIfObjectBagNull();

            NSLRecoverySessionResult result = null;

            await client.GetRequestProcessor(RPObjectKey).SendRequestAsync(NSLRecoverySessionPacket.CreateRequest(sessionInfo), d =>
            {
                result = NSLRecoverySessionResult.ReadFullFrom(d);

                return Task.FromResult(true);
            }, cancellationToken);

            return result;
        }

        public static void NSLSessionSendRequest<TClient>(this TClient client, Action<NSLRecoverySessionResult> onResponse, NSLSessionInfo sessionInfo, CancellationToken cancellationToken, string RPObjectKey = RequestProcessor.DefaultObjectBagKey)
            where TClient : BaseSocketNetworkClient
        {
            client.ThrowIfObjectBagNull();

            NSLRecoverySessionResult result = null;

            client.GetRequestProcessor(RPObjectKey).SendRequest(NSLRecoverySessionPacket.CreateRequest(sessionInfo), d =>
            {
                onResponse(NSLRecoverySessionResult.ReadFullFrom(d));

                return true;
            }, cancellationToken);
        }

    }

    public class NSLRecoverySessionPacket
    {
        public const ushort PacketId = ushort.MaxValue - 2;

        public static RequestPacketBuffer CreateRequest(NSLSessionInfo sessionInfo)
        {
            var request = RequestPacketBuffer.Create();

            request.PacketId = PacketId;

            sessionInfo.WriteFullTo(request);

            return request;
        }
    }
}
