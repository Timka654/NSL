using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Exceptions;
using NSL.SocketServer.Utils;
using NSL.UDP.Channels;
using NSL.UDP.Enums;
using NSL.UDP.Interface;
using NSL.UDP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.UDP
{
    public abstract class BaseUDPClient<TClient, TParent> : IClient<DgramOutputPacketBuffer>, IUDPClient
        where TClient : IServerNetworkClient
        where TParent : BaseUDPClient<TClient, TParent>
    {
        public abstract TClient Data { get; }

        public event ReceivePacketDebugInfo<TParent> OnReceivePacket;
        public event SendPacketDebugInfo<TParent> OnSendPacket;

        private CancellationTokenSource LiveStateTokenSource { get; } = new CancellationTokenSource();

        public CancellationToken LiveStateToken => LiveStateTokenSource.Token;

        #region Channels

        protected ReliableChannel<TClient, TParent> reliableChannel;
        protected UnreliableChannel<TClient, TParent> unreliableChannel;


        public ReliableChannel<TClient, TParent> ReliableChannel => reliableChannel;

        public UnreliableChannel<TClient, TParent> UnreliableChannel => unreliableChannel;

        #endregion

        #region Network

        public const int PHeadLenght = 5;

        #region Cipher

        /// <summary>
        /// Криптография с помощью которой мы расшифровываем полученные данные
        /// </summary>
        protected IPacketCipher inputCipher;

        /// <summary>
        /// Криптография с помощью которой мы разшифровываем данные
        /// </summary>
        protected IPacketCipher outputCipher;

        #endregion

        #region Buffer

        ///// <summary>
        ///// Буффер для приема данных
        ///// </summary>
        //protected byte[] receiveBuffer;

        ///// <summary>
        ///// Текущее положение в буффере, для метода BeginReceive
        ///// </summary>
        //protected int offset;

        ///// <summary>
        ///// Размер читаемых данных при следующем вызове BeginReceive
        ///// </summary>
        //protected int lenght = InputPacketBuffer.headerLenght;

        //protected bool data = false;

        #endregion

        #endregion

        public BaseUDPClient(IPEndPoint endPoint, Socket listenerSocket, UDPClientOptions<TClient> options)
        {
            this.options = options;

            OnReceivePacket += (client, pid, len) => options.CallReceivePacketEvent(client.Data, pid, len);
            OnSendPacket += (client, pid, len, st) => options.CallSendPacketEvent(client.Data, pid, len, st);

            this.parent = GetParent();
            this.endPoint = endPoint;
            this.listenerSocket = listenerSocket;
        }

        protected virtual void Initialize()
        {
            reliableChannel = new ReliableChannel<TClient, TParent>(this);
            unreliableChannel = new UnreliableChannel<TClient, TParent>(this);

            LiveStateToken.Register(() => SyncNetworkClientTimer.OnSync -= Sync);

            SyncNetworkClientTimer.OnSync += Sync;
        }

        private async void Sync()
        {
            await Task.Run(() =>
            {
                latestSendRate = currentSendRate;

                currentSendRate = 0;

                latestReceiveRate = currentReceiveRate;

                currentReceiveRate = 0;

                if (Data != null && Data.AliveState == false)
                    Disconnect();
            });
        }

        protected abstract TParent GetParent();

        protected UDPClientOptions<TClient> options;

        protected Dictionary<ushort, CoreOptions<TClient>.PacketHandle> PacketHandles;

        protected bool disconnected;

        private readonly TParent parent;
        private readonly IPEndPoint endPoint;
        private readonly Socket listenerSocket;

        public CoreOptions Options => options;

        public abstract void ChangeUserData(INetworkClient data);

        public object GetUserData() => Data;

        private void Disconnect(Exception ex)
        {
            RunException(ex);

            Disconnect();
        }

        public void Disconnect()
        {
            lock (this)
            {
                if (disconnected == true)
                    return;

                disconnected = true;
            }

            LiveStateTokenSource.Cancel();

            RunDisconnect();

            if (inputCipher != null)
                inputCipher.Dispose();

            if (outputCipher != null)
                outputCipher.Dispose();
        }

        public IPEndPoint GetRemotePoint() => endPoint;

        public Socket GetSocket() => null;

        public bool GetState()
            => Data.AliveState;

        public void Receive(Span<byte> receivedBytes)
        {
            Interlocked.Add(ref currentReceiveRate, receivedBytes.Length);

            var channel = DgramOutputPacketBuffer.ReadChannel(receivedBytes);

            if (channel.HasFlag(UDPChannelEnum.Reliable))
                reliableChannel.Receive(channel, receivedBytes);
            else if (channel.HasFlag(UDPChannelEnum.Unreliable))
                unreliableChannel.Receive(channel, receivedBytes);
        }

        public virtual void Receive(byte[] result, UDPChannelEnum channel)
        {
            //замыкаем это все в блок try, если клиент отключился то EndReceive может вернуть ошибку
            try
            {
                //дешефруем и засовываем это все в спец буффер в котором реализованы методы чтения типов, своего рода поток
                DgramInputPacketBuffer pbuff = new DgramInputPacketBuffer(inputCipher.Decode(result, 0, result.Length), channel, true);

                OnReceive(pbuff.PacketId, pbuff.PacketLength);

                //предотвращение ошибок в пакете
                try
                {
                    //ищем пакет и выполняем его, передаем ему данные сессии, полученные данные
                    PacketHandles[pbuff.PacketId](Data, pbuff);
                }
                catch (Exception ex)
                {
                    RunException(ex);
                }

                if (!pbuff.ManualDisposing)
                    pbuff.Dispose();
            }
            catch (ConnectionLostException clex)
            {
                Disconnect(clex);
            }
            catch (Exception ex)
            {
                Disconnect(ex);
            }
        }

        public void Send(DgramOutputPacketBuffer packet, bool disposeOnSend = true)
        {
#if DEBUG
            OnSend(packet, Environment.StackTrace);
#else
            OnSend(packet, "");
#endif

            packet.Send(this, disposeOnSend);
        }

        public void Send(OutputPacketBuffer packet, bool disposeOnSend = true)
        {
            if (!(packet is DgramOutputPacketBuffer dpkg))
            {
                dpkg = new DgramOutputPacketBuffer() { Channel = UDPChannelEnum.ReliableOrdered, PacketId = packet.PacketId };
                packet.DataPosition = 0;
                packet.CopyTo(dpkg);
            }

            Send(dpkg, disposeOnSend);
        }

        public void Send(UDPChannelEnum channel, byte[] buffer)
        {
            var sndBuf = outputCipher.Encode(buffer, 0, buffer.Length);

            if (channel.HasFlag(UDPChannelEnum.Reliable))
                reliableChannel.Send(channel, sndBuf);
            else
                unreliableChannel.Send(channel, sndBuf);
        }

        public void Send(byte[] buffer)
            => throw new NotImplementedException();

        public void Send(byte[] buf, int offset, int lenght)
            => throw new NotImplementedException();

        internal void SocketSend(byte[] sndBuffer, PacketWaitTemp packet)
        {
            try
            {
                if (listenerSocket == null)
                    return;

                if (currentSendRate + sndBuffer.Length > options.ClientLimitSendRate)
                    return;

                Interlocked.Add(ref currentSendRate, sndBuffer.Length);

                listenerSocket.SendTo(sndBuffer, SocketFlags.None, endPoint);
            }
            catch (ObjectDisposedException)
            {
                PacketFailProd(packet);

                Disconnect();
            }
            catch (Exception ex)
            {
                PacketFailProd(packet);

                Disconnect(ex);
            }
        }

        internal void SocketSend(byte[] sndBuffer)
        {
            try
            {
                if (listenerSocket == null)
                    return;

                if (currentSendRate + sndBuffer.Length > options.ClientLimitSendRate)
                    return;

                Interlocked.Add(ref currentSendRate, sndBuffer.Length);

                listenerSocket.SendTo(sndBuffer, SocketFlags.None, endPoint);
            }
            catch (ObjectDisposedException)
            {
                Disconnect();
            }
            catch (Exception ex)
            {
                Disconnect(ex);
            }
        }

        private void PacketFailProd(PacketWaitTemp packet)
        {
            var dataArray = new byte[packet.Head.Length + packet.Parts.Sum(x => x.Length)];

            Memory<byte> data = new Memory<byte>(dataArray);

            packet.Head.CopyTo(data);

            foreach (var item in packet.Parts)
            {
                item.CopyTo(data);
            }

            Data?.OnPacketSendFail(dataArray, 0, dataArray.Length);
        }

        public void SendEmpty(ushort packetId)
        {
            DgramOutputPacketBuffer rbuff = new DgramOutputPacketBuffer
            {
                PacketId = packetId
            };

            Send(rbuff);
        }

        protected virtual void OnReceive(ushort pid, int len)
        {
            OnReceivePacket?.Invoke(parent, pid, len);
        }

        protected abstract void RunDisconnect();

        protected abstract void RunException(Exception ex);

        public short GetTtl() => listenerSocket.Ttl;

        protected virtual void OnSend(DgramOutputPacketBuffer rbuff, string stackTrace = "")
        {
            OnSendPacket?.Invoke(parent, rbuff.PacketId, rbuff.PacketLength, stackTrace);
        }

        private int currentSendRate;
        private int currentReceiveRate;

        /// <summary>
        /// Send bytes per latest second
        /// </summary>
        public int SendBytesRate => latestSendRate;

        public int ReceiveBytesRate => latestReceiveRate;

        private int latestSendRate;
        private int latestReceiveRate;
    }
}
