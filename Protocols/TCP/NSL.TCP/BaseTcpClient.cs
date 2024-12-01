using Microsoft.Extensions.ObjectPool;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Cipher;
using NSL.SocketCore.Utils.Exceptions;
using NSL.SocketCore.Utils.Logger.Enums;
using System;
using System.Buffers;
using System.Collections.Concurrent;
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

        #endregion

        public BaseTcpClient(CoreOptions<TClient> options, bool legacyTransport)
        {
            this.options = options;
            this.legacyTransport = legacyTransport;
            OnReceivePacket += (client, pid, len) => options.CallReceivePacketEvent(client.Data, pid, len);
            OnSendPacket += (client, pid, len, st) => options.CallSendPacketEvent(client.Data, pid, len, st);

            this.parent = GetParent();
        }

        protected abstract TParent GetParent();

        protected CoreOptions<TClient> options;
        private readonly bool legacyTransport;
        private uint segmentSize;
        private Dictionary<ushort, CoreOptions<TClient>.PacketHandle> PacketHandles;

        public IPEndPoint GetRemotePoint()
        {
            return endPoint;
        }

        protected async void RunReceive()
        {
            segmentSize = Options.SegmentSize;

            disconnected = false;

            PacketHandles = options.GetHandleMap();

            if (!legacyTransport)
            {
                if (!await SendRPData())
                    return;

                if (!await ReceiveRPDataAsync())
                    return;

                startSend();

                asyncReceive();

                return;
            }

            ResetBuffer();

            startSend();

            threadReceive();
        }


        private void asyncReceive()
        {
            var sclient = this.sclient;

            if (sclient == null)
            {
                Disconnect();
                return;
            }

            SocketAsyncEventArgs receiveEventArg = new SocketAsyncEventArgs();
            receiveEventArg.SetBuffer(new byte[rpSegmentSize], 0, (int)rpSegmentSize);
            receiveEventArg.Completed += (s, ea) =>
            {
                try
                {
                    while (!asyncReceiveIter(ea)) { }
                }
                catch (NullReferenceException) { Disconnect(); }
                catch (ObjectDisposedException) { Disconnect(); }
                catch (ConnectionLostException clex)
                {
                    Disconnect(clex);
                }
                catch (Exception ex)
                {
                    Disconnect(new ConnectionLostException(GetRemotePoint(), true, ex));
                }
            };
            receiveEventArg.AcceptSocket = sclient;

            try
            {
                if (!sclient.ReceiveAsync(receiveEventArg))
                {
                    while (!asyncReceiveIter(receiveEventArg)) { }
                }
            }
            catch (NullReferenceException) { Disconnect(); }
            catch (ObjectDisposedException) { Disconnect(); }
            catch (ConnectionLostException clex)
            {
                Disconnect(clex);
            }
            catch (Exception ex)
            {
                Disconnect(new ConnectionLostException(GetRemotePoint(), true, ex));
            }
        }

#if DEBUGEXAMPLES

        int c = 0;

        Stopwatch? sw = null;

#endif

        private static byte[] emptyArray = (byte[])Array.CreateInstance(typeof(byte), 128 * 1024);

        #region remotePointSettings

        uint rpSegmentSize = 0;

        protected async Task<bool> SendRPData()
        {
            var data = new byte[2048];

            BitConverter.GetBytes(Options.SegmentSize).CopyTo(data.AsSpan(0));

            try
            {
                var sclient = this.sclient;

                if (sclient == null)
                {
                    Disconnect();
                    return false;
                }

                int offset = 0;

                do
                {
                    var r = await sclient.SendAsync(new ArraySegment<byte>(data, offset, data.Length - offset), SocketFlags.None);

                    if (r < 1)
                    {
                        Disconnect();
                        return false;
                    }

                    offset += r;
                } while (offset < data.Length);

                return true;

            }
            catch (NullReferenceException) { Disconnect(); }
            catch (ObjectDisposedException) { Disconnect(); }
            catch (Exception ex)
            {
                Disconnect(new ConnectionLostException(GetRemotePoint(), false, ex));
            }

            return false;
        }

        protected async Task<bool> ReceiveRPDataAsync()
        {
            var receive = new byte[2048];

            int offset = 0;

            try
            {

                do
                {
                    int r = await sclient.ReceiveAsync(new ArraySegment<byte>(receive, offset, receive.Length - offset), SocketFlags.None);

                    if (r < 1)
                    {
                        Disconnect();
                        return false;
                    }

                    offset += r;
                } while (offset < receive.Length);

                rpSegmentSize = BitConverter.ToUInt32(receive, 0);

                return true;
            }
            catch (NullReferenceException) { Disconnect(); }
            catch (ObjectDisposedException) { Disconnect(); }
            catch (Exception ex)
            {
                Disconnect(new ConnectionLostException(GetRemotePoint(), true, ex));
            }

            return false;
        }

        #endregion

        int soffset = 0;
        int poffset = 0;
        int hSkip = 0;

        InputPacketBuffer rBuff = null;

        static ArrayPool<byte> byteArrayPool = ArrayPool<byte>.Create();

        private bool asyncReceiveIter(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred == 0 || e.SocketError != SocketError.Success)
            {
                Disconnect();
                return true;
            }

            soffset += e.BytesTransferred;

            if (soffset < rpSegmentSize)
            {
                e.SetBuffer(e.Buffer, soffset, (int)(rpSegmentSize - soffset));
                return e.AcceptSocket.ReceiveAsync(e);
            }

            hSkip = 0;

            var buf = e.Buffer;


            if (rBuff == null)
            {
#if DEBUGEXAMPLES
                sw ??= Stopwatch.StartNew();
#endif

                if (!inputCipher.DecodeHeaderRef(ref buf, hSkip))
                    throw new CipherCodingException();

                rBuff = new InputPacketBuffer(buf.AsSpan(hSkip));

                length = rBuff.PacketLength;

                if (length < 1)
                    throw new NSLInvalidDataException(length, hSkip, buf, rBuff);

                rBuff.OnDispose += buff => byteArrayPool.Return(buff.Data);

                rBuff.SetData(byteArrayPool.Rent(rBuff.DataLength));

                poffset = 7;
                hSkip += 7;
            }

            var recv = Math.Min(soffset - hSkip, length - poffset);


            if (!inputCipher.DecodeRef(ref buf, hSkip, recv))
                throw new CipherCodingException();

            Buffer.BlockCopy(buf, hSkip, rBuff.Data, poffset - 7, recv);

            poffset += recv;

            if (poffset >= length)
            {
                OnReceive(rBuff.PacketId, rBuff.DataLength);

#if DEBUGEXAMPLES
                ++c;
#endif

                try
                {
                    PacketHandles[rBuff.PacketId](Data, rBuff);
                }
                catch (Exception ex)
                {
                    RunException(ex);
                }

                if (!rBuff.ManualDisposing)
                    rBuff.Dispose();

                rBuff = null;

#if DEBUGEXAMPLES
                if (c % 10000 == 0)
                    Options.HelperLogger?.Append(LoggerLevel.Debug, $"Received packets {c} in {sw.Elapsed.TotalMilliseconds} ms");
#endif
            }

            if (e.Count < rpSegmentSize)
                e.SetBuffer(e.Buffer, 0, (int)rpSegmentSize);

            soffset = default;

            return e.AcceptSocket.ReceiveAsync(e);
        }

        #region ThreadReceive

        protected byte[] receiveBuffer;

        protected int offset;

        protected int length;


        protected void ResetBuffer()
        {
            rBuff = null;
            length = InputPacketBuffer.DefaultHeaderLength;
            offset = 0;
        }


        CancellationTokenSource? thCancelTS;
        Thread? thReceiveThread;

        protected void threadReceive()
        {
            thCancelTS?.Cancel();

            thCancelTS = new CancellationTokenSource();

            var token = thCancelTS.Token;

            receiveBuffer = new byte[options.ReceiveBufferSize];

            thReceiveThread = new Thread(() =>
            {
                try
                {
                    while (!disconnected && !token.IsCancellationRequested)
                        threadReceiveIter(token);
                }
                catch (NullReferenceException) { Disconnect(); }
                catch (ObjectDisposedException) { Disconnect(); }
                catch (ConnectionLostException clex)
                {
                    Disconnect(clex);
                }
                catch (Exception ex)
                {
                    Disconnect(new ConnectionLostException(GetRemotePoint(), true, ex));
                }
            });

            thReceiveThread.Start();
        }

        private void threadReceiveIter(CancellationToken token)
        {
            var sclient = this.sclient;

            if (sclient == null)
                throw new ObjectDisposedException(nameof(sclient));

            int rlen = sclient.Receive(new ArraySegment<byte>(receiveBuffer, offset, length - offset), SocketFlags.None);

            if (rlen < 1)
                throw new ObjectDisposedException(nameof(sclient));

            offset += rlen;

            if (rBuff == null && offset == length)
            {
#if DEBUGEXAMPLES
                sw ??= Stopwatch.StartNew();
#endif

                if (!inputCipher.DecodeHeaderRef(ref receiveBuffer, 0))
                    throw new CipherCodingException($"Cannot peek message header {string.Join(" ", receiveBuffer?[0..7].Select(x => x.ToString("x2")) ?? Enumerable.Empty<string>())}");

                rBuff = new InputPacketBuffer(receiveBuffer);

                rBuff.SetData(byteArrayPool.Rent(rBuff.DataLength));

                rBuff.OnDispose += (rBuff) =>
                {
                    if (rBuff.ManualDisposing)
                        byteArrayPool.Return(rBuff.Data);
                };

                length = rBuff.PacketLength;

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

            if (rBuff != null && offset == length)
            {
                if (!inputCipher.DecodeRef(ref receiveBuffer, 7, rBuff.DataLength))
                    throw new CipherCodingException();

                Buffer.BlockCopy(receiveBuffer, 7, rBuff.Data, 0, rBuff.DataLength);

                OnReceive(rBuff.PacketId, rBuff.DataLength);

#if DEBUGEXAMPLES
                ++c;
#endif

                try
                {
                    PacketHandles[rBuff.PacketId](Data, rBuff);
                }
                catch (Exception ex)
                {
                    RunException(ex);
                }

                if (!rBuff.ManualDisposing)
                    rBuff.Dispose();

                ResetBuffer();
#if DEBUGEXAMPLES
                if (c % 10000 == 0)
                    options.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Debug, $"{c}, {sw.Elapsed.TotalMilliseconds}");
#endif
            }
        }

        #endregion



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

        private SocketAsyncEventArgs sendArgs;
        ChannelReader<byte[]> sendChannelReader;

        private async void startSend()
        {
            sendChannelReader = sendChannel.Reader;

            sendArgs = new SocketAsyncEventArgs()
            {
                SocketError = SocketError.Success,
                BufferList = new List<ArraySegment<byte>>(),
                UserToken = false
            };

            sendArgs.Completed += sendBufHandle;

            while (!await sendProc(sendArgs)) { }
        }
        public int maxSendTime = 0;

        private int spc = 0;

        static ObjectPool<SocketAsyncEventArgs> sendBufferPool = ObjectPool.Create<SocketAsyncEventArgs>();

        private async void sendBufHandle(object s, SocketAsyncEventArgs e)
        {
            while (!await sendProc(e)) { }
        }

        private void returnBufferPool(SocketAsyncEventArgs args)
        {
            args.Completed -= sendBufHandle;
            Interlocked.Decrement(ref spc);
            sendBufferPool.Return(args);
        }

        private async Task<bool> sendProc(SocketAsyncEventArgs args)
        {
            bool pooledObject = ((bool)args.UserToken);

            byte[] buf = null;

            try
            {
                if (args.SocketError != SocketError.Success)
                {
                    Data?.OnPacketSendFail(args.BufferList[0].Array, 0, args.BufferList[0].Count);

                    if (pooledObject)
                    {
                        returnBufferPool(args);
                    }
                    Disconnect();
                    return true;
                }

                var sclient = this.sclient;

                if (disconnected || sclient == null)
                    return true;



                if (pooledObject)
                {
                    if (!sendChannelReader.TryRead(out buf))
                    {
                        returnBufferPool(args);
                        return true;
                    }
                }
                else
                {
                    if (!sendChannelReader.TryRead(out buf))
                        buf = await sendChannelReader.ReadAsync();
                }


                bool r = sendBuf(buf, args);

                if (!pooledObject)
                {
                    while (sendChannelReader.TryRead(out buf))
                    {
                        args = sendBufferPool.Get();
                        args.BufferList = new List<ArraySegment<byte>>();
                        args.Completed += sendBufHandle;
                        args.UserToken = true;


                        Interlocked.Increment(ref spc);
                        sendBuf(buf, args);
                    }
                }

                buf = null;

                return r;
            }
            catch (NullReferenceException) { Disconnect(); }
            catch (ObjectDisposedException) { Disconnect(); }
            catch (Exception ex)
            {
                Disconnect(new ConnectionLostException(GetRemotePoint(), false, ex));
            }
            finally
            {
                if (buf != null)
                    Data?.OnPacketSendFail(buf, 0, buf.Length);
            }

            return true;
        }

        private bool sendBuf(byte[] buf, SocketAsyncEventArgs args)
        {

            var pid = BitConverter.ToUInt16(buf, 4);

            ArraySegment<byte> sndBuffer = outputCipher.Encode(buf, 0, buf.Length);

            args.BufferList.Clear();

            args.BufferList.Add(sndBuffer);

            int s = (int)(sndBuffer.Count % segmentSize);

            s = (int)(segmentSize - s);

            if (s > 0)
                args.BufferList.Add(new ArraySegment<byte>(emptyArray, 0, s));

            args.BufferList = args.BufferList;


            return sclient.SendAsync(args);
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

            if (rBuff != null)
            {
                rBuff?.Dispose();
                rBuff = null;
            }
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
