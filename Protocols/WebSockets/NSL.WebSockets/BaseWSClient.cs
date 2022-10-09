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

namespace NSL.WebSockets
{
    public abstract class BaseWSClient<TClient, TParent> : IClient<OutputPacketBuffer>
        where TClient : INetworkClient
        where TParent : BaseWSClient<TClient, TParent>
    {
        public event ReceivePacketDebugInfo<TParent> OnReceivePacket;
        public event SendPacketDebugInfo<TParent> OnSendPacket;

        #region Network

        /// <summary>
        /// Сокет, собственно поток для взаимодействия с пользователем
        /// </summary>
        protected WebSocket sclient;

        protected HttpListenerContext context;

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
        protected int lenght = InputPacketBuffer.headerLenght;

        protected bool data = false;

        #endregion

        #endregion

        public BaseWSClient()
        {
            this.parent = GetParent();
        }

        protected abstract TParent GetParent();

        protected CoreOptions<TClient> options;

        private Dictionary<ushort, CoreOptions<TClient>.PacketHandle> PacketHandles;

        public virtual IPEndPoint GetRemotePoint()
        {
            return (IPEndPoint)context?.Request.RemoteEndPoint;
        }

        protected void ResetBuffer()
        {
            data = false;
            lenght = InputPacketBuffer.headerLenght;
            offset = 0;
        }

        protected void InitReceiver()
        {
            disconnected = false;

            PacketHandles = options.GetHandleMap();

            //Возвращает значения в исходное положение
            ResetBuffer();
        }

        protected async void RunReceiveAsync()
        {
            await ReceiveLoop();
        }

        protected async Task ReceiveLoop()
        {
            do
            {
                await Receive();

            } while (!disconnected);
        }

        private async Task Receive()
        {
            //замыкаем это все в блок try, если клиент отключился то EndReceive может вернуть ошибку
            try
            {
                if (sclient == null)
                    throw new ConnectionLostException(GetRemotePoint(), true);

                var receiveData = await sclient.ReceiveAsync(new ArraySegment<byte>(receiveBuffer, offset, lenght - offset), CancellationToken.None);

                if (receiveData.CloseStatus.HasValue)
                {
                    Disconnect();
                    return;
                }

                //принимаем размер данных которые удалось считать
                int rlen = receiveData.Count;
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
                            PacketHandles[pbuff.PacketId]((TClient)GetUserData(), pbuff);
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
            catch (ConnectionLostException clex)
            {
                RunException(clex);
                Disconnect();
            }
            catch (Exception ex)
            {
                if (!disconnected)
                {
                    RunException(ex);

                    //отключаем клиента, в случае ошибки не в транспортном потоке а где-то в пакете, что-бы клиент не завис 
                    Disconnect();
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

        protected AutoResetEvent _sendLocker = new AutoResetEvent(true);

        /// <summary>
        /// Отправка массива байт
        /// </summary>
        /// <param name="buf">массив байт</param>
        /// <param name="offset">смещение с которого начинается передача</param>
        /// <param name="lenght">размер передаваемых данных</param>
        public async void Send(byte[] buf, int offset, int lenght)
        {
            _sendLocker.WaitOne();
            try
            {
                //шифруем данные
                byte[] sndBuffer = outputCipher.Encode(buf, offset, lenght);

                //начинаем отправку данных
                if (sclient != null)
                    await sclient.SendAsync(new ArraySegment<byte>(sndBuffer, offset, lenght), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                AddWaitPacket(buf, offset, lenght);
                RunException(ex);

                //отключаем клиента, лишним не будет
                Disconnect();
            }

            _sendLocker.Set();
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
                try { sclient.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None); } catch { }
                try { sclient.Dispose(); } catch { }
            }
            //очищаем буффер данных
            receiveBuffer = null;

            sclient = null;
        }

        private readonly TParent parent;

        public CoreOptions Options => options;

        public abstract void ChangeUserData(INetworkClient data);

        public abstract object GetUserData();

        public Socket GetSocket() => default;

        protected virtual void OnSend(OutputPacketBuffer rbuff, string stackTrace = "")
        {
            OnSendPacket?.Invoke(parent, rbuff.PacketId, rbuff.PacketLenght, stackTrace);
        }

        protected virtual void OnReceive(ushort pid, int len)
        {
            OnReceivePacket?.Invoke(parent, pid, len);
        }

        protected abstract void AddWaitPacket(byte[] buffer, int offset, int length);

        protected abstract void RunDisconnect();

        protected abstract void RunException(Exception ex);

        public short GetTtl() => -1;
    }
}
