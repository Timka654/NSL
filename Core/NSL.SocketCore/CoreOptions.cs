using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Cipher;
using NSL.SocketCore.Utils.Logger;
using NSL.Utils;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NSL.SocketCore
{
    public class CoreOptions
    {
        public ObjectBag ObjectBag { get; } = new ObjectBag();

        public IBasicLogger HelperLogger { get; set; }

        /// <summary>
        /// Тип ип адресса, InterNetwork - IPv4, InterNetworkV6 - IPv6б по умолчанию AddressFamily.Unspecified - определяется автоматически
        /// </summary>
        public virtual AddressFamily AddressFamily { get; set; } = AddressFamily.Unspecified;

        /// <summary>
        /// Протокол для передачи данных, по умолчанию ProtocolType.Unspecified - определяется автоматически
        /// </summary>
        public virtual ProtocolType ProtocolType { get; set; } = ProtocolType.Unspecified;

        /// <summary>
        /// Размер буффера приходящих данных, если пакет больше этого значения то данные по реализованному алгоритму принять не получиться, значение по умолчанию - 1024
        /// </summary>
        public int ReceiveBufferSize { get; set; } = 1024;

        private IPacketCipher inputCipher = new PacketNoneCipher();

        /// <summary>
        /// Алгоритм шифрования входящих пакетов
        /// </summary>
        public IPacketCipher InputCipher
        {
            get => inputCipher;
            set
            {
                if (inputCipher != null && inputCipher != value)
                    inputCipher.Dispose();

                inputCipher = value ?? new PacketNoneCipher();
            }
        }


        private IPacketCipher outputCipher = new PacketNoneCipher();

        /// <summary>
        /// Алгоритм шифрования исходящих пакетов
        /// </summary>
        public IPacketCipher OutputCipher
        {
            get => outputCipher;
            set
            {
                if (outputCipher != null && outputCipher != value)
                    outputCipher.Dispose();

                outputCipher = value ?? new PacketNoneCipher();
            }
        }
    }

    /// <summary>
    /// Содержит функции для которых необходимо явное указывание типа клиента наследуется <see cref="CoreOptions"/>
    /// </summary>
    /// <typeparam name="TClient">INetworkClient</typeparam>
    public abstract class CoreOptions<TClient> : CoreOptions
        where TClient : INetworkClient
    {
        /// <summary>
        /// Пакеты которые будет принимать и обрабатывать текущий узел
        /// </summary>
        protected Dictionary<ushort, IPacket<TClient>> Packets = new Dictionary<ushort, IPacket<TClient>>();

        protected Dictionary<ushort, PacketHandle> PacketHandles = new Dictionary<ushort, PacketHandle>();

        public Dictionary<ushort, PacketHandle> GetHandleMap()
        {
            return new Dictionary<ushort, PacketHandle>(PacketHandles);
        }

        /// <summary>
        /// Добавить пакет для обработки текущим узлом
        /// </summary>
        /// <param name="packetId">Индификатор пакета в системе</param>
        /// <param name="packet">Обработчик пакета</param>
        /// <returns></returns>
        public bool AddPacket(ushort packetId, IPacket<TClient> packet)
        {
            if (!PacketHandles.ContainsKey(packetId))
            {
                Packets.Add(packetId, packet);
                PacketHandles.Add(packetId, packet.Receive);

                return true;
            }

            return false;
        }

        public bool AddAsyncPacket(ushort packetId, IAsyncPacket<TClient> packet)
            => AddPacket(packetId, packet);

        public bool AddHandle(ushort packetId, PacketHandle handle)
        {
            if (!PacketHandles.ContainsKey(packetId))
            {
                PacketHandles.Add(packetId, handle);

                return true;
            }

            return false;
        }

        public bool AddAsyncHandle(ushort packetId, AsyncPacketHandle handle)
            => AddHandle(packetId, (client, input) =>
            {
                input.ManualDisposing = true;
                Task.Run(async () =>
                    {
                        try
                        {
                            await handle(client, input);
                        }
                        catch (Exception ex)
                        {
                            CallExceptionEvent(ex, client);
                        }

                        if (input.AsyncDisposing)
                            input.Dispose();
                    });
            });

        public IPacket<TClient> GetPacket(ushort packetId)
        {
            Packets.TryGetValue(packetId, out var result);
            return result;
        }

        public TPacket GetPacket<TPacket>(ushort packetId)
            where TPacket : IPacket<TClient>
        {
            Packets.TryGetValue(packetId, out var result);

            return result as TPacket;
        }

        /// <summary>
        /// Делегат для регистрации события перехвата сетевых ошибок
        /// </summary>
        /// <param name="ex">Возникшая ошибка</param>
        /// <param name="s">Сокет с которым произошла ошибка</param>
        public delegate void ExceptionHandle(Exception ex, TClient client);

        /// <summary>
        /// Делегат для регистрации события перехвата подключения клиента
        /// </summary>
        /// <param name="client">Данные клиента</param>
        public delegate void ClientConnect(TClient client);
        public delegate Task ClientConnectAsync(TClient client);

        /// <summary>
        /// Делегат для регистрации события перехвата отключения клиента
        /// </summary>
        /// <param name="client">Данные клиента</param>
        public delegate void ClientDisconnect(TClient client);
        public delegate Task ClientDisconnectAsync(TClient client);

        /// <summary>
        /// Делегат для регистрации пакета
        /// </summary>
        /// <param name="client">Данные клиента</param>
        /// <param name="data">Входящий буффер с данными</param>
        /// <param name="output">Исходящий буффер с данными(не обязательно)</param>
        public delegate void PacketHandle(TClient client, InputPacketBuffer data);
        public delegate Task AsyncPacketHandle(TClient client, InputPacketBuffer data);



        public delegate void ReceivePacketHandle(TClient client, ushort pid, int len);

        public delegate void SendPacketHandle(TClient client, ushort pid, int len, string stackTrace);

        /// <summary>
        /// События вызываемое при получении ошибки
        /// </summary>
        public event ExceptionHandle OnExceptionEvent = (ex, client) => { };

        /// <summary>
        /// Событие вызываемое при подключении клиента
        /// </summary>
        public event ClientConnect OnClientConnectEvent = (client) => { };
        public event ClientConnectAsync OnClientConnectAsyncEvent = client => Task.CompletedTask;

        /// <summary>
        /// Событие вызываемое при отключении клиента
        /// </summary>
        public event ClientDisconnect OnClientDisconnectEvent = (client) => { };
        public event ClientDisconnectAsync OnClientDisconnectAsyncEvent = client => Task.CompletedTask;

        public event ReceivePacketHandle OnReceivePacket = (c, p, i) => { };
        public event SendPacketHandle OnSendPacket = (c, p, i, stack) => { };

        public void CallReceivePacketEvent(TClient client, ushort pid, int len)
        {
            OnReceivePacket(client, pid, len);
        }

        public void CallSendPacketEvent(TClient client, ushort pid, int len, string stackTrace)
        {
            OnSendPacket(client, pid, len, stackTrace);
        }

        /// <summary>
        /// Вызов события ошибка
        /// </summary>
        public void CallExceptionEvent(Exception ex, TClient client)
        {
            OnExceptionEvent(ex, client);
        }

        /// <summary>
        /// Вызов события подключения клиента
        /// </summary>
        /// <param name="client"></param>
        public void CallClientConnectEvent(TClient client)
        {
            OnClientConnectEvent(client);

            Task.Run(() => OnClientConnectAsyncEvent.InvokeAsync(t => t(client)));
        }

        /// <summary>
        /// Вызов события отключения клиента
        /// </summary>
        /// <param name="client"></param>
        public void CallClientDisconnectEvent(TClient client)
        {
            if (client == null)
                return;

            client.DisconnectTime = DateTime.UtcNow;

            OnClientDisconnectEvent?.Invoke(client);

            Task.Run(() => OnClientDisconnectAsyncEvent.InvokeAsync(t => t(client)));
        }
    }
}
