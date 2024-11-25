using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Exceptions;
using NSL.SocketCore.Utils;
using NSL.SocketCore;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Linq;
using System.Threading.Channels;

namespace NSL.WebSockets
{
    public abstract class BaseWSClient<TClient, TParent> : IClient<OutputPacketBuffer>
        where TClient : INetworkClient
        where TParent : BaseWSClient<TClient, TParent>
    {
        public event ReceivePacketDebugInfo<TParent> OnReceivePacket;
        public event SendPacketDebugInfo<TParent> OnSendPacket;

        public abstract TClient Data { get; }

        #region Network

        /// <summary>
        /// Сокет, собственно поток для взаимодействия с пользователем
        /// </summary>
        protected WebSocket sclient;

        protected HttpListenerContext context;

        protected IPEndPoint remoteEndPoint;

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

        /// <summary>
        /// Буффер для приема данных
        /// </summary>
        protected byte[] receiveBuffer;

        /// <summary>
        /// Текущее положение в буффере, для метода BeginReceive
        /// </summary>
        protected int offset;

        /// <summary>
        /// Размер читаемых данных при следующем вызове BeginReceive
        /// </summary>
        protected int length = InputPacketBuffer.DefaultHeaderLength;

        protected bool data = false;

        #endregion

        #endregion

        public BaseWSClient(CoreOptions<TClient> options)
        {
            this.options = options;

            OnReceivePacket += (client, pid, len) => options.CallReceivePacketEvent(client.Data, pid, len);
            OnSendPacket += (client, pid, len, st) => options.CallSendPacketEvent(client.Data, pid, len, st);

            this.parent = GetParent();
        }

        protected abstract TParent GetParent();

        protected CoreOptions<TClient> options;

        private Dictionary<ushort, CoreOptions<TClient>.PacketHandle> PacketHandles;

        public virtual IPEndPoint GetRemotePoint()
        {
            return remoteEndPoint;
        }

        protected void ResetBuffer()
        {
            data = false;
            length = InputPacketBuffer.DefaultHeaderLength;
            offset = 0;
        }

        protected async void RunReceive()
        {
            await ReceiveLoop();
        }

        protected async Task ReceiveLoop()
        {
            disconnected = false;

            PacketHandles = options.GetHandleMap();

            sendCycle();

            ResetBuffer();

            try
            {
                while (!disconnected)
                    await Receive();
            }
            catch (ConnectionLostException clex)
            {
                Disconnect(clex);
            }
            catch (Exception ex)
            {
                Disconnect(new ConnectionLostException(GetRemotePoint(), true, ex));
            }
        }

        private async Task Receive()
        {
            if (sclient == null)
                throw new ObjectDisposedException(nameof(sclient));

            var receiveData = await sclient.ReceiveAsync(new ArraySegment<byte>(receiveBuffer, offset, length - offset), CancellationToken.None);

            if (receiveData.CloseStatus.HasValue)
                throw new WebSocketClosedException(receiveData.CloseStatus, receiveData.CloseStatusDescription);

            int rlen = receiveData.Count;

            if (rlen < 1)
                throw new ObjectDisposedException(nameof(sclient));

            offset += rlen;

            if (offset == length)
            {
                if (data == false)
                {
                    var peeked = inputCipher.Peek(receiveBuffer);

                    if (peeked == null)
                        throw new Exception($"Cannot peek message header {string.Join(" ", receiveBuffer?[0..7].Select(x => x.ToString("x2")) ?? Enumerable.Empty<string>())}");

                    length = BitConverter.ToInt32(peeked, 0);

                    data = true;

                    while (receiveBuffer.Length < length)
                    {
                        Array.Resize(ref receiveBuffer, receiveBuffer.Length * 2);
                    }
                }

                if (offset == length && data)
                {
                    InputPacketBuffer pbuff = new InputPacketBuffer(inputCipher.Decode(receiveBuffer, 0, length));

                    OnReceive(pbuff.PacketId, length);

                    ResetBuffer();

                    try
                    {
                        PacketHandles[pbuff.PacketId](Data, pbuff);
                    }
                    catch (Exception ex)
                    {
                        RunException(ex);
                    }

                    if (!pbuff.ManualDisposing)
                        pbuff.Dispose();
                }
            }
        }

        #region Send

        /// <summary>
        /// Send byte buffer async
        /// </summary>
        /// <param name="buffer"></param>
        public void Send(OutputPacketBuffer packet, bool disposeOnSend = true)
        {
#if DEBUG
            OnSend(packet, Environment.StackTrace);
#else
            OnSend(packet, "");
#endif

            packet.Send(this, disposeOnSend);
        }

        /// <summary>
        /// Send byte buffer async
        /// </summary>
        /// <param name="buffer"></param>
        public async void Send(byte[] buffer)
        {
            try { await sendChannel.Writer.WriteAsync(buffer); } catch (InvalidOperationException) { Data?.OnPacketSendFail(buffer, 0, buffer.Length); }
        }
        /// <summary>
        /// Send byte buffer async to server
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public async void Send(byte[] buf, int offset, int length)
        {
            try
            {
                if (offset == 0 && length == buf.Length)
                {
                    await sendChannel.Writer.WriteAsync(buf);
                    return;
                }

                await sendChannel.Writer.WriteAsync(buf[offset..(offset + length)]);
            }
            catch (InvalidOperationException) { Data?.OnPacketSendFail(buf, offset, length); }

        }

        private Channel<byte[]> sendChannel = Channel.CreateUnbounded<byte[]>();

        private async void sendCycle()
        {
            var reader = sendChannel.Reader;

            byte[]? buf = null;

            try
            {
                while (!disconnected)
                {
                    buf = await reader.ReadAsync();

                    byte[] sndBuffer = outputCipher.Encode(buf, offset, length);

                    await sclient.SendAsync(sndBuffer, WebSocketMessageType.Binary, true, CancellationToken.None);

                    buf = null;
                }
            }
            catch (Exception ex)
            {
                if (buf != null)
                    Data?.OnPacketSendFail(buf, 0, buf.Length);

                Disconnect(ex);
            }
        }

        public void SendEmpty(ushort packetId)
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            Send(rbuff);
        }

        #endregion

        public bool GetState()
        {
            if (sclient == null || disconnected)
                return false;

            return sclient.State == WebSocketState.Closed || sclient.CloseStatus.HasValue == false;
        }

        protected bool disconnected = true;

        private void Disconnect(Exception ex)
        {
            RunException(ex);

            Disconnect();
        }

        public void Disconnect()
        {
            if (disconnected == true)
                return;

            disconnected = true;
            RunDisconnect();

            if (inputCipher != null)
                inputCipher.Dispose();

            if (outputCipher != null)
                outputCipher.Dispose();


            if (sclient != null)
            {
                //отключаем и очищаем данные о клиенте
                try { sclient.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None); } catch { }
                try { sclient.Dispose(); } catch { }
            }

            receiveBuffer = null;

            sclient = null;
        }

        private readonly TParent parent;

        public CoreOptions Options => options;

        public abstract void ChangeUserData(INetworkClient data);

        public abstract void SetClientData(INetworkClient from);

        public object GetUserData() => Data;

        public Socket GetSocket() => default;

        protected virtual void OnSend(OutputPacketBuffer rbuff, string stackTrace = "")
        {
            OnSendPacket?.Invoke(parent, rbuff.PacketId, rbuff.PacketLength, stackTrace);
        }

        protected virtual void OnReceive(ushort pid, int len)
        {
            OnReceivePacket?.Invoke(parent, pid, len);
        }

        protected abstract void RunDisconnect();

        protected abstract void RunException(Exception ex);

        public short GetTtl() => -1;
    }
}
