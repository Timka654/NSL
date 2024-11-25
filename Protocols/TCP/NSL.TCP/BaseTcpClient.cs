using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Exceptions;
using System;
using System.Collections.Generic;
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

            ResetBuffer();

            sendCycle();

            Receive();
        }

        private async void Receive()
        {
            try
            {
                while (!disconnected)
                    await receive();
            }
            catch (NullReferenceException) { } // on disconnected client
            catch (SocketException ex)
            {
                Disconnect();
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

        private async Task receive()
        {
            if (sclient == null)
                throw new ConnectionLostException(GetRemotePoint(), true);

            int rlen = await sclient.ReceiveAsync(receiveBuffer.AsMemory(offset, length - offset), SocketFlags.None);

            if (rlen < 1)
                throw new ConnectionLostException(GetRemotePoint(), true);

            offset += rlen;

            if (offset == length)
            {
                if (data == false)
                {
                    var peeked = inputCipher.Peek(receiveBuffer);

                    length = BitConverter.ToInt32(peeked, 0);

                    data = true;

                    while (receiveBuffer.Length < length)
                    {
                        Array.Resize(ref receiveBuffer, receiveBuffer.Length * 2);
                        sclient.ReceiveBufferSize = receiveBuffer.Length;
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

            try
            {
                while (!disconnected)
                {
                    await foreach (var buf in reader.ReadAllAsync())
                    {
                        byte[] sndBuffer = outputCipher.Encode(buf, 0, buf.Length);

                        if (sclient == null)
                        {
                            Data?.OnPacketSendFail(buf, 0, buf.Length);

                            Disconnect();
                            return;
                        }

                        var offs = 0;

                        do
                        {
                            var len = await sclient.SendAsync(sndBuffer[offs..], SocketFlags.None);
                            if (len < 0)
                            {
                                Data?.OnPacketSendFail(buf, 0, buf.Length);
                                Disconnect();
                                return;
                            }

                            offs += len;

                        } while (offs < sndBuffer.Length);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                Disconnect();
            }
            catch (SocketException ex)
            {
                Disconnect();
            }
            catch (NullReferenceException ex)
            {
                Disconnect();
            }
            catch (ObjectDisposedException ex)
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

        public async void Disconnect()
        {
            if (disconnected == true)
                return;

            disconnected = true;
            RunDisconnect();

            if (inputCipher != null)
                inputCipher.Dispose();

            if (outputCipher != null)
                outputCipher.Dispose();

            await foreach (var buf in sendChannel.Reader.ReadAllAsync())
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
