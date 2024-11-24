using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading;
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
        protected int lenght = InputPacketBuffer.DefaultHeaderLength;

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
            lenght = InputPacketBuffer.DefaultHeaderLength;
            offset = 0;
        }

        protected void RunReceive()
        {
            disconnected = false;

            PacketHandles = options.GetHandleMap();

            ResetBuffer();

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

            int rlen = await sclient.ReceiveAsync(receiveBuffer.AsMemory(offset, lenght - offset));

            if (rlen < 1)
                throw new ConnectionLostException(GetRemotePoint(), true);

            offset += rlen;

            if (offset == lenght)
            {
                if (data == false)
                {
                    var peeked = inputCipher.Peek(receiveBuffer);

                    lenght = BitConverter.ToInt32(peeked, 0);

                    data = true;

                    while (receiveBuffer.Length < lenght)
                    {
                        Array.Resize(ref receiveBuffer, receiveBuffer.Length * 2);
                        sclient.ReceiveBufferSize = receiveBuffer.Length;
                    }
                }

                if (offset == lenght && data)
                {
                    InputPacketBuffer pbuff = new InputPacketBuffer(inputCipher.Decode(receiveBuffer, 0, lenght));

                    OnReceive(pbuff.PacketId, lenght);

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
        /// Отправка пакета
        /// </summary>
        /// <param name="packet">спец буффер содержащий в себе данные пакета</param>
        public void Send(OutputPacketBuffer packet, bool disposeOnSend = true)
        {
#if DEBUG
            OnSend(packet, Environment.StackTrace);
#else
            OnSend(packet, "");
#endif

            packet.Send(this, disposeOnSend);
        }

        public async void Send(byte[] buffer)
            => await send(buffer, 0, buffer.Length);

        protected AutoResetEvent _sendLocker;

        /// <summary>
        /// Отправка массива байт
        /// </summary>
        /// <param name="buf">массив байт</param>
        /// <param name="offset">смещение с которого начинается передача</param>
        /// <param name="lenght">размер передаваемых данных</param>
        public async void Send(byte[] buf, int offset, int lenght)
        {
            await send(buf, offset, lenght);
        }

        private async Task send(byte[] buf, int offset, int lenght)
        {
            var sl = _sendLocker;
            try
            {
                sl?.WaitOne();
                //шифруем данные
                byte[] sndBuffer = outputCipher.Encode(buf, offset, lenght);

                //начинаем отправку данных
                if (sclient != null)
                {
                    var offs = 0;

                    do
                    {
                        var len = await sclient.SendAsync(sndBuffer[offs..], SocketFlags.None);
                        if (len < 0)
                        {
                            Data?.OnPacketSendFail(buf, offset, lenght);
                            Disconnect();
                            return;
                        }

                        offs += len;

                    } while (offs < sndBuffer.Length);
                }
                else
                    Data?.OnPacketSendFail(buf, offset, lenght);
            }
            catch (OperationCanceledException ex)
            {
                Data?.OnPacketSendFail(buf, offset, lenght);
                Disconnect();
            }
            catch (SocketException ex)
            {
                Data?.OnPacketSendFail(buf, offset, lenght);
                Disconnect();
            }
            catch (NullReferenceException ex)
            {
                Data?.OnPacketSendFail(buf, offset, lenght);
                Disconnect();
            }
            catch (ObjectDisposedException ex)
            {
                Data?.OnPacketSendFail(buf, offset, lenght);
                Disconnect();
            }
            catch (Exception ex)
            {
                Data?.OnPacketSendFail(buf, offset, lenght);
                Disconnect(ex);
            }
            finally
            {
                sl?.Set();
            }
        }

        ///// <summary>
        ///// Завершение отправки данных
        ///// </summary>
        ///// <param name="r"></param>
        //private void EndSend(IAsyncResult r)
        //{
        //    //замыкаем это все в блок try, если клиент отключился то EndSend может вернуть ошибку
        //    try
        //    {
        //        //получаем размер переданных данных
        //        int len = sclient?.EndSend(r) ?? 0;
        //        //при некоторых ошибках размер возвращает 0 или -1, проверяем
        //        if (len < 1)
        //        {
        //            var sas = ((SendAsyncState)r.AsyncState);
        //            Data?.OnPacketSendFail(sas.buf, sas.offset, sas.len);
        //            Disconnect();
        //        }
        //    }
        //    catch (SocketException ex)
        //    {

        //        var sas = ((SendAsyncState)r.AsyncState);
        //        Data?.OnPacketSendFail(sas.buf, sas.offset, sas.len);
        //        Disconnect();
        //    }
        //    catch (NullReferenceException ex)
        //    {

        //        var sas = ((SendAsyncState)r.AsyncState);
        //        Data?.OnPacketSendFail(sas.buf, sas.offset, sas.len);
        //        Disconnect();
        //    }
        //    catch (ObjectDisposedException ex)
        //    {

        //        var sas = ((SendAsyncState)r.AsyncState);
        //        Data?.OnPacketSendFail(sas.buf, sas.offset, sas.len);
        //        Disconnect();
        //    }
        //    catch (Exception ex)
        //    {
        //        var sas = ((SendAsyncState)r.AsyncState);
        //        Data?.OnPacketSendFail(sas.buf, sas.offset, sas.len);
        //        Disconnect(ex);
        //    }
        //    catch
        //    {
        //        var sas = ((SendAsyncState)r.AsyncState);
        //        Data?.OnPacketSendFail(sas.buf, sas.offset, sas.len);
        //        Disconnect();
        //    }
        //}

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

            var sl = _sendLocker;

            _sendLocker = default;

            sl?.Dispose();

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
