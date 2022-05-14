using NSL.SocketCore.Utils;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace NSL.UDP.Client
{
    public class UDPClient<T> : BaseUDPClient<T, UDPClient<T>>
        where T : IServerNetworkClient
    {
        private class PacketTemp
        {
            public ushort PID;

            public ushort Lenght;

            public ConcurrentBag<Memory<byte>> Parts;

            public PacketTemp(ushort PID, ushort len) : this(PID)
            {
                this.Lenght = len;
            }

            public PacketTemp(ushort PID)
            {
                this.PID = PID;
                this.Lenght = 0;
                Parts = new ConcurrentBag<Memory<byte>>();
            }

            public bool Ready() => Lenght > 0 && Parts.Count == Lenght;
        }

        private T Data { get; set; }

        public UDPClient(IPEndPoint receivePoint, Socket listenerSocket, ServerOptions<T> options) : base(receivePoint, listenerSocket)
        {
            this.options = options;

            Initialize();
        }

        protected void Initialize()
        {
            PacketHandles = options.GetHandleMap();

            Data = Activator.CreateInstance<T>();

            //обзятельная переменная в NetworkClient, для отправки данных, можно использовать привидения типов (Client)NetworkClient но это никому не поможет
            Data.Network = this;
            Data.ServerOptions = options;

            //установка криптографии для дешифровки входящих данных, указана в общих настройках сервера
            inputCipher = options.InputCipher.CreateEntry();
            //установка криптографии для шифровки исходящих данных, указана в общих настройках сервера
            outputCipher = options.OutputCipher.CreateEntry();

            disconnected = false;
            //Начало приема пакетов от клиента
            options.RunClientConnect(Data);
        }

        /// <summary>
        /// Общие настройки сервера
        /// </summary>
        public ServerOptions<T> ServerOptions => (ServerOptions<T>)base.options;

        public override void ChangeUserData(INetworkClient old_client_data)
        {
            if (old_client_data == null)
            {
                Data = null;
                return;
            }

            old_client_data.ChangeOwner(Data);
        }

        public override object GetUserData() => Data;

        protected override UDPClient<T> GetParent() => this;

        protected override void AddWaitPacket(byte[] buffer, int offset, int length) => Data?.AddWaitPacket(buffer, offset, length);

        ConcurrentDictionary<ushort, PacketTemp> packetBuffer = new();

        private static ushort ReadPID(Memory<byte> buffer) => BitConverter.ToUInt16(buffer.Span[1..]);
        private static bool ReadLP(Memory<byte> buffer) => buffer.Span[0] == 1;
        private static ushort ReadLPLen(Memory<byte> buffer) => BitConverter.ToUInt16(buffer.Span[3..]);
        private static ushort ReadPOffset(Memory<byte> buffer) => BitConverter.ToUInt16(buffer.Span);

        public void Receive(SocketAsyncEventArgs result)
        {
            using (result)
            {
                var packet = packetBuffer.GetOrAdd(
                    ReadPID(result.MemoryBuffer),
                    id => new PacketTemp(id));

                if (ReadLP(result.MemoryBuffer))
                    packet.Lenght = ReadLPLen(result.MemoryBuffer);
                else
                    packet.Parts.Add(result.MemoryBuffer[3..result.BytesTransferred]);

                if (packet.Ready() &&
                    packetBuffer.TryRemove(packet.PID, out packet))
                {
                    base.Receive(packet.Parts
                    .OrderBy(x => ReadPOffset(x))
                    .SelectMany(x => x.Slice(2).ToArray())
                    .ToArray());
                }
            }
        }

        protected override void OnReceive(ushort pid, int len)
        {
            Data.LastReceiveMessage = DateTime.UtcNow;

            base.OnReceive(pid, len);
        }

        protected override void RunDisconnect() => ServerOptions.RunClientDisconnect(Data);

        protected override void RunException(Exception ex) => ServerOptions.RunException(ex, Data);
    }
}
