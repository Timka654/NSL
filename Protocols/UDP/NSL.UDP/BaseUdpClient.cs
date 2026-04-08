using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Exceptions;
using NSL.SocketServer.Utils;
using NSL.UDP.Channels;
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
        where TClient : BaseNetworkConnection, new()
        where TParent : BaseUDPClient<TClient, TParent>
    {
        public abstract TClient Data { get; }

        public event ReceivePacketDebugInfo<TParent> OnReceivePacket;
        public event SendPacketDebugInfo<TParent> OnSendPacket;

        private CancellationTokenSource liveStateTokenSource = new CancellationTokenSource();

        // Keep a property alias so existing internal usages (Channels, Sync) don't need changing.
        private CancellationTokenSource LiveStateTokenSource => liveStateTokenSource;

        public CancellationToken LiveStateToken => liveStateTokenSource.Token;

        #region Channels

        protected ReliableChannel<TClient, TParent> reliableChannel;
        protected UnreliableChannel<TClient, TParent> unreliableChannel;


        public ReliableChannel<TClient, TParent> ReliableChannel => reliableChannel;

        public UnreliableChannel<TClient, TParent> UnreliableChannel => unreliableChannel;

        #endregion

        #region Network

        public const int PHeadLength = 5;

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
        //protected int length = InputPacketBuffer.headerLength;

        //protected bool data = false;

        #endregion

        #endregion

        TParent parent;

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

                reliableChannel?.CleanupStalePackets();
                unreliableChannel?.CleanupStalePackets();

                if (Data != null && Data.AliveState == false)
                    Disconnect();
            });
        }

        protected abstract TParent GetParent();

        protected UDPClientOptions<TClient> options;

        protected Dictionary<ushort, CoreOptions.PacketHandle> PacketHandles;

        protected bool disconnected;

        public bool IsDisconnected => disconnected;

        /// <summary>
        /// Resets only the liveness state (CTS, Sync-timer registration, disconnected flag)
        /// so the object can receive packets again.  Channels and ciphers are intentionally
        /// NOT reset here — call <see cref="ReinitializeChannels"/> explicitly when a true
        /// new connection is confirmed (e.g. on UDPConnectHandshake receipt).
        /// Must be called under <c>lock(this)</c> by the subclass.
        /// </summary>
        protected void ReinitializeBase()
        {
            // Cancel and replace the old CTS so that:
            // 1. Its Register callback fires immediately → SyncNetworkClientTimer.OnSync -= Sync
            //    (prevents accumulating stale subscriptions across reconnect cycles).
            // 2. Disconnect() which snapshotted the old CTS will cancel it harmlessly here;
            //    it won't see the new CTS and will skip RunDisconnect (see ReferenceEquals guard).
            var oldCts = liveStateTokenSource;
            liveStateTokenSource = new CancellationTokenSource();
            oldCts.Cancel();
            oldCts.Dispose();

            LiveStateToken.Register(() => SyncNetworkClientTimer.OnSync -= Sync);
            SyncNetworkClientTimer.OnSync += Sync;

            disconnected = false;
        }

        /// <summary>
        /// Resets channels and ciphers.  Call this when a genuine new connection is confirmed
        /// (i.e. after receiving <c>UDPConnectHandshake</c>) to avoid sequence-number desync
        /// with legitimate late/retransmitted packets from the previous session.
        /// </summary>
        public void ReinitializeChannels()
        {
            RunException(new Exception($"[UDP-DIAG] ReinitializeChannels ep={GetRemotePoint()}"));

            reliableChannel = new ReliableChannel<TClient, TParent>(this);
            unreliableChannel = new UnreliableChannel<TClient, TParent>(this);

            if (inputCipher != null) inputCipher.Dispose();
            if (outputCipher != null) outputCipher.Dispose();
            inputCipher = options.InputCipher.CreateEntry();
            outputCipher = options.OutputCipher.CreateEntry();
        }
        private readonly IPEndPoint endPoint;
        private readonly Socket listenerSocket;

        public CoreOptions Options => options;

        public abstract void ChangeUserData(BaseNetworkConnection data);

        public abstract void SetClientData(BaseNetworkConnection from);

        public object GetUserData() => Data;

        private void Disconnect(Exception ex)
        {
            RunException(ex);

            Disconnect();
        }

        public void Disconnect()
        {
            // Snapshot the CTS UNDER THE LOCK so that a concurrent Reinitialize() that replaces
            // liveStateTokenSource cannot make us cancel the WRONG (new-session) CTS.
            CancellationTokenSource ctsToCancel;
            lock (this)
            {
                if (disconnected == true)
                    return;

                disconnected = true;
                ctsToCancel = liveStateTokenSource;
            }

            ctsToCancel.Cancel();

            // If Reinitialize() ran between our lock release and here it will have replaced
            // liveStateTokenSource with a brand-new CTS.  In that case the session is already
            // alive again — skip the disconnect event and cipher disposal so we don't tear
            // down the new session.
            if (!ReferenceEquals(ctsToCancel, liveStateTokenSource))
            {
                RunException(new Exception($"[UDP-DIAG] Disconnect suppressed - Reinitialize won race ep={GetRemotePoint()}"));
                return;
            }

            RunException(new Exception($"[UDP-DIAG] Disconnect ep={GetRemotePoint()}\n{Environment.StackTrace}"));

            RunDisconnect();

            if (inputCipher != null)
                inputCipher.Dispose();

            if (outputCipher != null)
                outputCipher.Dispose();
        }

        public IPEndPoint GetRemotePoint() => endPoint;

        public Socket GetSocket() => null;

        public bool GetState()
            => !disconnected && (Data?.AliveState ?? false);

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
            try
            {
                inputCipher.DecodeHeaderRef(ref result, 0);

                inputCipher.DecodeRef(ref result, InputPacketBuffer.DefaultHeaderLength, result.Length - InputPacketBuffer.DefaultHeaderLength);



                DgramInputPacketBuffer pbuff = new DgramInputPacketBuffer(result, channel, true);
                pbuff.SetData(result[7..]);

                OnReceive(pbuff.PacketId, pbuff.PacketLength);

                //предотвращение ошибок в пакете
                try
                {
                    //ищем пакет и выполняем его, передаем ему данные сессии, полученные данные
                    if (PacketHandles.TryGetValue(pbuff.PacketId, out var handler))
                        handler(Data, pbuff);
                    else if (pbuff.PacketId < (ushort)NSLSystemPacketEnum.NSLSystemMinPID)
                        RunException(new Exception($"No handler registered for packet id {pbuff.PacketId}"));
                    // else: unhandled NSL system packet — silently ignore
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
                packet.Position = OutputPacketBuffer.DefaultHeaderLength;
                packet.CopyTo(dpkg);
            }

            Send(dpkg, disposeOnSend);
        }

        public void Send(UDPChannelEnum channel, byte[] buffer)
        {
            outputCipher.EncodeHeaderRef(ref buffer, 0);
            outputCipher.EncodeRef(ref buffer, OutputPacketBuffer.DefaultHeaderLength, buffer.Length - OutputPacketBuffer.DefaultHeaderLength);

            if (channel.HasFlag(UDPChannelEnum.Reliable))
                reliableChannel.Send(channel, buffer);
            else
                unreliableChannel.Send(channel, buffer);
        }

        public void Send(byte[] buffer)
            => throw new NotImplementedException();

        public void Send(byte[] buf, int offset, int length)
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
                Disconnect();
            }
            catch (Exception ex)
            {
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

        public void SendEmpty(ushort packetId)
        {
            // Use Unreliable|Unordered for all single-packet control messages (ping, pong,
            // handshake echo). These don't need delivery guarantees — the caller retries or
            // the next heartbeat cycle covers any loss. Using Reliable here would create
            // pending ACK/retransmit state that accumulates and blocks the heartbeat loop.
            DgramOutputPacketBuffer rbuff = new DgramOutputPacketBuffer
            {
                PacketId = packetId,
                Channel = UDPChannelEnum.Unreliable | UDPChannelEnum.Unordered
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
