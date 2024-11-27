using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
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

        protected byte[] receiveBuffer;

        protected int offset;

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

        private Dictionary<ushort, CoreOptions<TClient>.PacketHandle> PacketHandles;

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

        protected void RunReceive()
        {
            disconnected = false;

            PacketHandles = options.GetHandleMap();

            SendRPData();

            ReceiveRPDataAsync(() =>
            {
                ResetBuffer();

                sendCycle();

                Receive();
            });
        }

        private void Receive()
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
                while (!ProcessReceive(ea)) {  }
            };
            receiveEventArg.AcceptSocket = sclient;

            if (!sclient.ReceiveAsync(receiveEventArg))
            {
                while(!ProcessReceive(receiveEventArg)) { }
            }
        }

        int c = 0;

        Stopwatch? sw = null;

        private static byte[] emptyArray = (byte[])Array.CreateInstance(typeof(byte), 128 * 1024);

        #region remotePointSettings

        uint rpSegmentSize = 0;

        protected void SendRPData()
        {
            var data = new byte[2048];

            BitConverter.GetBytes(Options.SegmentSize).CopyTo(data.AsSpan(0));

            sclient.Send(data);
        }

        protected async void ReceiveRPDataAsync(Action onReceive)
        {
            try
            {
                var receive = new byte[2048];

                int offset = 0;

                do
                {
                    int r = await sclient.ReceiveAsync(new ArraySegment<byte>(receive, offset, 2048));

                    offset += r;
                } while (offset < receive.Length);

                rpSegmentSize = BitConverter.ToUInt32(receive, 0);

                onReceive();
            }
            catch (Exception ex)
            {
                Disconnect(new ConnectionLostException(GetRemotePoint(), true, ex));
            }
        }

        #endregion

        int soffset = 0;
        int poffset = 0;
        int hSkip = 0;
        int sskip = 0;
        int offs;

        InputPacketBuffer rBuff = null;

        static ArrayPool<byte> rented = ArrayPool<byte>.Create();

        private bool ProcessReceive(SocketAsyncEventArgs e)
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
                return sclient.ReceiveAsync(e);
            }
            else
            {
                e.SetBuffer(e.Buffer, 0, (int)rpSegmentSize);
                soffset = default;
            }

            hSkip = 0;

            if (rBuff == null)
            {
                sw ??= Stopwatch.StartNew();

                inputCipher.Peek(new ArraySegment<byte>(e.Buffer, hSkip, 7));

                rBuff = new InputPacketBuffer(e.Buffer.AsSpan(hSkip, 7));

                rBuff.OnDispose += buff => rented.Return(buff.Data);

                if (rBuff.DataLength < 1)
                { 
                
                }

                rBuff.SetData(rented.Rent(rBuff.DataLength));

                poffset = 7;
                hSkip += 7;
            }

            var recv = Math.Min(rBuff.DataLength - poffset, e.BytesTransferred - hSkip);

            var buf = e.Buffer;

            inputCipher.DecodeRef(ref buf, hSkip, recv);

            Buffer.BlockCopy(buf, 0, rBuff.Data, 0, recv);

            poffset += recv;

            if (poffset >= rBuff.DataLength)
            {
                sskip = (int)(rpSegmentSize -  (rBuff.DataLength + 7));

                OnReceive(rBuff.PacketId, length);

                ++c;

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

                Options.HelperLogger?.Append(LoggerLevel.Debug, $"Received packets {c} in {sw.Elapsed.TotalMilliseconds} ms");
            }

            return sclient.ReceiveAsync(e);
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

                    do
                    {
                        ArraySegment<byte> sndBuffer = outputCipher.Encode(buf, 0, buf.Length);

                        var offs = 0;

                        do
                        {
                            var len = await sclient.SendAsync(sndBuffer[offs..], SocketFlags.None);

                            if (len < 0)
                                throw new ObjectDisposedException(nameof(sclient));

                            offs += len;

                        } while (offs < sndBuffer.Count);

                        offs = 0;

                        sndBuffer = emptyArray[..(int)(options.SegmentSize - (sndBuffer.Count % options.SegmentSize))];

                        while (offs < sndBuffer.Count)
                        {
                            var len = await sclient.SendAsync(sndBuffer[offs..], SocketFlags.None);

                            if (len < 0)
                                throw new ObjectDisposedException(nameof(sclient));

                            offs += len;

                        }

                        buf = null;

                    } while (reader.TryRead(out buf));
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
