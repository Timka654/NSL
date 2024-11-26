using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NSL.TCP
{
    public abstract class BaseTcpClient<TClient, TParent> : IClient<OutputPacketBuffer>
        where TClient : INetworkClient
        where TParent : BaseTcpClient<TClient, TParent>
    {
        public abstract TClient Data { get; }

        public event ReceivePacketDebugInfo<TParent> OnReceivePacket;
        public event SendPacketDebugInfo<TParent> OnSendPacket;

        #region Network

        /// <summary>
        /// Сокет, собственно поток для взаимодействия с пользователем
        /// </summary>
        protected Socket sclient;

        protected IPEndPoint endPoint;

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

        public BaseTcpClient(CoreOptions<TClient> options)
        {
            this.options = options;

            OnReceivePacket += (client, pid, len) => options.CallReceivePacketEvent(client.Data, pid, len);
            OnSendPacket += (client, pid, len, st) => options.CallSendPacketEvent(client.Data, pid, len, st);

            this.parent = GetParent();
        }

        protected abstract TParent GetParent();

        protected CoreOptions<TClient> options;

        protected Dictionary<ushort, CoreOptions<TClient>.PacketHandle> PacketHandles;

        public IPEndPoint GetRemotePoint()
        {
            return endPoint;
        }

        protected void ResetBuffer()
        {
            data = false;
            length = InputPacketBuffer.DefaultHeaderLength;
            offset = 0;
        }

        protected void RunReceive(bool legacyThread = false)
        {
            disconnected = false;

            PacketHandles = options.GetHandleMap();

            ResetBuffer();

            sendCycle();

            if (legacyThread)
            {
                threadReceive();
                return;
            }

            taskReceive();
        }

        private async void taskReceive()
        {
            try
            {
                while (!disconnected)
                    await taskReceiveIter();
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

        private async Task taskReceiveIter()
        {
            if (sclient == null)
                throw new ObjectDisposedException(nameof(sclient));

            int rlen = await sclient.ReceiveAsync(new ArraySegment<byte>(receiveBuffer, offset, length - offset), SocketFlags.None);

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

                    if (length > receiveBuffer.Length)
                    {
                        int n = receiveBuffer.Length;

                        do
                        {
                            n *= 2;
                        } while (n < length);

                        Array.Resize(ref receiveBuffer, n);
                        sclient.ReceiveBufferSize = n;
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



        Thread receiveThread;
        CancellationTokenSource threadCancelTS;

        int c = 0;
        Stopwatch? sw = null;

        protected void threadReceive()
        {
            threadCancelTS?.Cancel();

            threadCancelTS = new CancellationTokenSource();

            var token = threadCancelTS.Token;

            receiveThread = new Thread(() =>
            {
                try
                {
                    while (!disconnected && !token.IsCancellationRequested)
                        threadReceiveIter(token);
                }
                catch (ConnectionLostException clex)
                {
                    Disconnect(clex);
                }
                catch (Exception ex)
                {
                    Disconnect(new ConnectionLostException(GetRemotePoint(), true, ex));
                }


            });

            receiveThread.Start();
        }

        private void threadReceiveIter(CancellationToken token)
        {
            if (sclient == null)
                throw new ObjectDisposedException(nameof(sclient));

            int rlen = sclient.Receive(new ArraySegment<byte>(receiveBuffer, offset, length - offset), SocketFlags.None);

            if (rlen < 1)
                throw new ObjectDisposedException(nameof(sclient));

            offset += rlen;

            if (offset == length)
            {
                if (data == false)
                {
                    sw ??= Stopwatch.StartNew();

                    var peeked = inputCipher.Peek(receiveBuffer);

                    if (peeked == null)
                        throw new Exception($"Cannot peek message header {string.Join(" ", receiveBuffer?[0..7].Select(x => x.ToString("x2")) ?? Enumerable.Empty<string>())}");

                    length = BitConverter.ToInt32(peeked, 0);

                    data = true;

                    if (length > receiveBuffer.Length)
                    {
                        int n = receiveBuffer.Length;

                        do
                        {
                            n *= 2;
                        } while (n < length);

                        Array.Resize(ref receiveBuffer, n);
                        sclient.ReceiveBufferSize = n;
                    }
                }

                if (offset == length && data)
                {
                    InputPacketBuffer pbuff = new InputPacketBuffer(inputCipher.Decode(receiveBuffer, 0, length));

                    OnReceive(pbuff.PacketId, length);
                    ++c;

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

                    options.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Debug, $"{c}, {sw.Elapsed.TotalMilliseconds}");
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

                    byte[] sndBuffer = outputCipher.Encode(buf, 0, buf.Length);

                    var offs = 0;

                    do
                    {
                        var len = await sclient.SendAsync(sndBuffer[offs..], SocketFlags.None);

                        if (len < 0)
                            throw new ObjectDisposedException(nameof(sclient));

                        offs += len;

                    } while (offs < sndBuffer.Length);

                    buf = null;
                }
            }
            catch (Exception ex)
            {
                if (buf != null)
                    Data?.OnPacketSendFail(buf, 0, buf.Length);

                Disconnect(new ConnectionLostException(GetRemotePoint(), false, ex));
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

            return sclient.Connected;
        }

        protected bool disconnected = true;

        public void Disconnect(Exception ex)
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

            while (sendChannel.Reader.TryRead(out var buf))
            {
                Data?.OnPacketSendFail(buf, 0, buf.Length);
            }

            if (sclient != null)
            {
                try { sclient.Disconnect(false); } catch { }
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

        public Socket GetSocket() => sclient;

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

        public short GetTtl() => sclient.Ttl;
    }
}
