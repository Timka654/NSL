﻿using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.UDP.Channels;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace NSL.UDP.Client
{
    public class UDPClient<T> : BaseUDPClient<T, UDPClient<T>>
        where T : INetworkClient, new()
    {
        private T data;

        public override T Data => data;

        public UDPClient(IPEndPoint receivePoint, Socket listenerSocket, CoreOptions<T> options) : base(receivePoint, listenerSocket)
        {
            this.options = options;

            Initialize();
        }

        protected override void Initialize()
        {
            base.Initialize();

            PacketHandles = options.GetHandleMap();

            data = new T();

            //обзятельная переменная в NetworkClient, для отправки данных, можно использовать привидения типов (Client)NetworkClient но это никому не поможет
            Data.Network = this;
            //Data.ServerOptions = options;

            //установка криптографии для дешифровки входящих данных, указана в общих настройках сервера
            inputCipher = options.InputCipher.CreateEntry();
            //установка криптографии для шифровки исходящих данных, указана в общих настройках сервера
            outputCipher = options.OutputCipher.CreateEntry();

            disconnected = false;
            //Начало приема пакетов от клиента
            options.RunClientConnect(Data);
        }

        public override void ChangeUserData(INetworkClient old_client_data)
        {
            if (old_client_data == null)
            {
                data = null;
                return;
            }

            old_client_data.ChangeOwner(Data);
        }

        protected override UDPClient<T> GetParent() => this;

        protected override void AddWaitPacket(byte[] buffer, int offset, int length) => Data?.AddWaitPacket(buffer, offset, length);

        public void Receive(SocketAsyncEventArgs result)
        {
            using (result)
            {
                var channel = BaseChannel<T, UDPClient<T>>.ReadChannel(result.MemoryBuffer);

                if (channels.TryGetValue(channel & ~Enums.UDPChannelEnum.Ordered & ~Enums.UDPChannelEnum.Unordered, out var ch))
                    ch.Receive(channel, result);

                ArrayPool<byte>.Shared.Return(result.MemoryBuffer.ToArray());
            }
        }

        protected override void OnReceive(ushort pid, int len)
        {
            Data.LastReceiveMessage = DateTime.UtcNow;

            base.OnReceive(pid, len);
        }

        protected override void RunDisconnect() => base.options.RunClientDisconnect(Data);

        protected override void RunException(Exception ex) => base.options.RunException(ex, Data);
    }
}
