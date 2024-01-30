using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
            return (IPEndPoint)sclient?.RemoteEndPoint;
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

            //Возвращает значения в исходное положение
            ResetBuffer();

            //Начало приема пакетов от клиента
            sclient.BeginReceive(receiveBuffer, offset, lenght, SocketFlags.None, Receive, sclient);
        }

        private void Receive(IAsyncResult result)
        {
            //замыкаем это все в блок try, если клиент отключился то EndReceive может вернуть ошибку
            try
            {
                if (sclient == null)
                    throw new ConnectionLostException(GetRemotePoint(), true);
                //принимаем размер данных которые удалось считать
                int rlen = sclient.EndReceive(result);
                //при некоторых ошибках размер возвращает 0 или -1, проверяем
                if (rlen < 1)
                    throw new ConnectionLostException(GetRemotePoint(), true);

                //добавляем offset для дальнейшей считки пакета
                offset += rlen;

                //если размер ожидаемвых данных соответствует ожидаемому - обрабатываем
                if (offset == lenght)
                {
                    if (data == false)
                    {
                        var peeked = inputCipher.Peek(receiveBuffer);
                        //если все ок
                        //получаем размер пакета
                        lenght = BitConverter.ToInt32(peeked, 0);

                        ushort pid = BitConverter.ToUInt16(peeked, 4);
                        OnReceive(pid, lenght);
                        data = true;

                        while (receiveBuffer.Length < lenght)
                        {
                            Array.Resize(ref receiveBuffer, receiveBuffer.Length * 2);
                            sclient.ReceiveBufferSize = receiveBuffer.Length;
                        }
                    }

                    //если все на месте, запускаем обработку
                    if (offset == lenght && data)
                    {
                        //обработка пакета

                        //дешефруем и засовываем это все в спец буффер в котором реализованы методы чтения типов, своего рода поток
                        InputPacketBuffer pbuff = new InputPacketBuffer(inputCipher.Decode(receiveBuffer, 0, lenght));

                        // обнуляем показатели что-бы успешно запустить цикл заново
                        ResetBuffer();


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
                }
                sclient.BeginReceive(receiveBuffer, offset, lenght - offset, SocketFlags.None, Receive, sclient);
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

        public void Send(byte[] buffer)
            => Send(buffer, 0, buffer.Length);

        protected AutoResetEvent _sendLocker = new AutoResetEvent(true);

        /// <summary>
        /// Отправка массива байт
        /// </summary>
        /// <param name="buf">массив байт</param>
        /// <param name="offset">смещение с которого начинается передача</param>
        /// <param name="lenght">размер передаваемых данных</param>
        public void Send(byte[] buf, int offset, int lenght)
        {
            _sendLocker.WaitOne();

            try
            {
                //шифруем данные
                byte[] sndBuffer = outputCipher.Encode(buf, offset, lenght);

                //начинаем отправку данных
                if (sclient != null)
                    sclient.BeginSend(sndBuffer, 0, lenght, SocketFlags.None, EndSend, new SendAsyncState { buf = buf, offset = offset, len = lenght });
            }
            catch (Exception ex)
            {
                Data?.OnPacketSendFail(buf, offset, lenght);
                Disconnect(ex);
            }
            finally
            {
                _sendLocker.Set();
            }
        }

        /// <summary>
        /// Завершение отправки данных
        /// </summary>
        /// <param name="r"></param>
        private void EndSend(IAsyncResult r)
        {
            //замыкаем это все в блок try, если клиент отключился то EndSend может вернуть ошибку
            try
            {
                //получаем размер переданных данных
                int len = sclient?.EndSend(r) ?? 0;
                //при некоторых ошибках размер возвращает 0 или -1, проверяем
                if (len < 1)
                    throw new ConnectionLostException(GetRemotePoint(), false);
            }
            catch (Exception ex)
            {
                var sas = ((SendAsyncState)r.AsyncState);
                Data?.OnPacketSendFail(sas.buf, sas.offset, sas.len);
                Disconnect(ex);
            }
            catch
            {
                Disconnect();
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

            //проверяем возможно клиент и не был инициализирован, в случае дос атак, такое возможно
            if (sclient != null)
            {
                //отключаем и очищаем данные о клиенте
                try { sclient.Disconnect(false); } catch { }
                try { sclient.Dispose(); } catch { }
            }
            //очищаем буффер данных
            receiveBuffer = null;

            sclient = null;
        }

        private readonly TParent parent;

        public CoreOptions Options => options;

        public abstract void ChangeUserData(INetworkClient data);

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
