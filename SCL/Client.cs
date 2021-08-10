using SocketCore;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketCore.Utils.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SCL
{
    internal class SendAsyncState
    {
        public byte[] buf { get; set; }

        public int offset { get; set; }

        public int len { get; set; }
    }

    /// <summary>
    /// Класс обработки клиента
    /// </summary>
    public class Client<T> : IClient
        where T : BaseSocketNetworkClient
    {

        public event ReceivePacketDebugInfo<Client<T>> OnReceivePacket;
        public event SendPacketDebugInfo<Client<T>> OnSendPacket;

        public long Version { get; set; }

        /// <summary>
        /// Криптография с помощью которой мы расшифровываем полученные данные
        /// </summary>
        private IPacketCipher inputCipher;

        /// <summary>
        /// Криптография с помощью которой мы разшифровываем данные
        /// </summary>
        private IPacketCipher outputCipher;

        /// <summary>
        /// Сокет, собственно поток для взаимодействия с пользователем
        /// </summary>
        private Socket sclient;

        /// <summary>
        /// Буффер для приема данных
        /// </summary>
        private byte[] receiveBuffer;

        /// <summary>
        /// Текущее положение в буффере, для метода BeginReceive
        /// </summary>
        private int offset;

        /// <summary>
        /// Размер читаемых данных при следующем вызове BeginReceive
        /// </summary>
        private int lenght = InputPacketBuffer.headerLenght;

        private bool data = false;

        private IPEndPoint currentPoint;

        /// <summary>
        /// Общие настройки сервера
        /// </summary>
        protected ClientOptions<T> clientOptions;

        /// <summary>
        /// Инициализация прослушивания клиента
        /// </summary>
        /// <param name="options">общие настройки сервера</param>
        public Client(ClientOptions<T> options)
        {
            //установка переменной с общими настройками сервера
            this.clientOptions = options;
        }

        /// <summary>
        /// Запуск цикла приема пакетов
        /// </summary>
        /// <param name="client">клиент</param>
        public void Reconnect(Socket client)
        {
            disconnected = false;
            string[] keys = clientOptions.ClientData?.RecoverySessionKeyArray;
            IEnumerable<byte[]> waitBuffer = clientOptions.ClientData?.GetWaitPackets();

            clientOptions.ClientData = Activator.CreateInstance<T>();

            clientOptions.ClientData.SetRecoveryData(clientOptions.ClientData.Session, keys);
            clientOptions.ClientData.Network = (IClient)this;

            if (waitBuffer != null)
                foreach (var item in waitBuffer)
                {
                    clientOptions.ClientData.AddWaitPacket(item, 0, item.Length);
                }

            //установка переменной содержащую поток клиента
            this.sclient = client;
            currentPoint = (IPEndPoint)client.RemoteEndPoint;

            //установка массива для приема данных, размер указан в общих настройках сервера
            this.receiveBuffer = new byte[clientOptions.ReceiveBufferSize];
            //установка криптографии для дешифровки входящих данных, указана в общих настройках сервера
            this.inputCipher = (IPacketCipher)clientOptions.inputCipher.Clone();
            //установка криптографии для шифровки исходящих данных, указана в общих настройках сервера
            this.outputCipher = (IPacketCipher)clientOptions.outputCipher.Clone();

            //Bug fix, в системе Windows это значение берется из реестра, мы не сможем принять больше за раз чем прописанно в нем, если данных будет больше, то цикл зависнет
            sclient.ReceiveBufferSize = clientOptions.ReceiveBufferSize;

            //Bug fix, отключение буфферизации пакетов для уменьшения трафика, если не отключить то получим фризы, в случае с игровым соединением эту опцию обычно нужно отключать
            sclient.NoDelay = true;
            _sendLocker.Set();

            //Возвращает значения в исходное положение
            data = false;
            lenght = InputPacketBuffer.headerLenght;
            offset = 0;

            //Начало приема пакетов от клиента
            sclient.BeginReceive(receiveBuffer, offset, lenght, SocketFlags.None, Receive, sclient);

            Utils.SystemPackets.Version.Send(clientOptions.ClientData, Version);
            Utils.SystemPackets.RecoverySession<T>.Send(clientOptions.ClientData);

            clientOptions.RunClientConnect();
        }

        private void Receive(IAsyncResult result)
        {
            //замыкаем это все в блок try, если клиент отключился то EndReceive может вернуть ошибку
            try
            {
                if (sclient == null)
                    throw new ConnectionLostException(currentPoint, true);
                //принимаем размер данных которые удалось считать
                int rlen = sclient.EndReceive(result);
                //при некоторых ошибках размер возвращает 0 или -1, проверяем
                if (rlen < 1)
                    throw new ConnectionLostException(currentPoint, true);

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
                        //#if DEBUG
                        //                        if (this.receiveBuffer.Length > clientOptions.ReceiveBufferSize)
                        //                            Debug.LogWarning($"Warning: client receive buffer > {clientOptions.ReceiveBufferSize}({this.receiveBuffer.Length})");
                        //#endif
                        //обработка пакета

                        //дешефруем и засовываем это все в спец буффер в котором реализованы методы чтения типов, своего рода поток
                        InputPacketBuffer pbuff = new InputPacketBuffer(inputCipher.Decode(receiveBuffer, 0, lenght));

                        // обнуляем показатели что-бы успешно запустить цикл заново
                        lenght = InputPacketBuffer.headerLenght;
                        offset = 0;
                        data = false;


                        //ищем пакет и выполняем его, передаем ему данные сессии, полученные данные, и просим у него данные для передачи
                        clientOptions.Packets[pbuff.PacketId].Receive(clientOptions.ClientData, pbuff);
                        pbuff.Dispose();
                    }
                }
                sclient.BeginReceive(receiveBuffer, offset, lenght - offset, SocketFlags.None, Receive, sclient);
            }
            catch (ConnectionLostException clex)
            {
                clientOptions.RunException(clex);
                Disconnect();
            }
            catch (Exception ex)
            {
                clientOptions.RunException(ex);

                //отключаем клиента, в случае ошибки не в транспортном потоке а где-то в пакете, что-бы клиент не завис 
                Disconnect();
            }
        }

        /// <summary>
        /// Отправка пакета
        /// </summary>
        /// <param name="rbuff">спец буффер содержащий в себе данные пакета</param>
        public void Send(OutputPacketBuffer rbuff
            )
        {
#if DEBUG
            OnSend(rbuff, Environment.StackTrace);
#else
            OnSend(rbuff);
#endif

            Send(rbuff.CompilePacket(), 0, rbuff.PacketLenght);
        }

        protected virtual void OnSend(OutputPacketBuffer rbuff, string stackTrace = "")
        {
            OnSendPacket?.Invoke(this, rbuff.PacketId, rbuff.PacketLenght, stackTrace);

        }

        protected virtual void OnReceive(ushort pid, int len)
        {
            OnReceivePacket?.Invoke(this, pid, len);
        }

        private AutoResetEvent _sendLocker = new AutoResetEvent(true);

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
                sclient.BeginSend(sndBuffer, 0, lenght, SocketFlags.None, EndSend, new SendAsyncState { buf = buf, offset = offset, len = lenght });
            }
            catch (Exception ex)
            {
                clientOptions.ClientData.AddWaitPacket(buf, offset, lenght);
                clientOptions.RunException(ex);

                //отключаем клиента, лишним не будет
                Disconnect();
            }

            _sendLocker.Set();
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
                int len = sclient.EndSend(r);
                //при некоторых ошибках размер возвращает 0 или -1, проверяем
                if (len < 1)
                    throw new ConnectionLostException(sclient.RemoteEndPoint, false);
            }
            catch (ConnectionLostException clex)
            {
                var sas = ((SendAsyncState)r.AsyncState);
                this.clientOptions.ClientData?.AddWaitPacket(sas.buf, sas.offset, sas.len);
                clientOptions.RunException(clex);
                Disconnect();
            }
            catch
            {
                Disconnect();
            }
        }

        /// <summary>
        /// Отключить клиента
        /// </summary>
        public void Disconnect(bool disconnecteventcall = true)
        {
            //проверяем возможно клиент и не был инициализирован, в случае дос атак, такое возможно
            if (sclient != null)
            {
                //отключаем и очищаем данные о клиенте
                try { sclient.Disconnect(false); } catch { }
                try { sclient.Dispose(); } catch { }
                sclient = null;
            }
            lenght = InputPacketBuffer.headerLenght;
            offset = 0;
            if (disconnecteventcall)
                this.clientOptions.RunClientDisconnect();
        }

        public bool GetState()
        {
            if (sclient == null)
                return false;
            return sclient.Connected;
        }

#region SendOneValueExtensions

        public void SendEmpty(ushort packetId)
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            Send(rbuff);
        }

        private bool disconnected;

        public void Disconnect()
        {
            if (disconnected == true)
                return;

            disconnected = true;
            this.clientOptions.RunClientDisconnect();
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

        public IPEndPoint GetRemovePoint()
        {
            return (IPEndPoint)sclient?.RemoteEndPoint;
        }

        public void ChangeUserData(INetworkClient data)
        {
            clientOptions.ClientData = (T)data;
        }

        public object GetUserData()
        {
            return clientOptions.ClientData;
        }

        public Socket GetSocket()
        {
            return sclient;
        }
        #endregion

    }

    //public interface IClient
    //{
    //    void Send(OutputPacketBuffer packet);

    //    void SendEmpty(ushort packetId);

    //    void Send(ushort packetId, byte value);

    //    void Send(ushort packetId, int value);

    //    void Send(ushort packetId, bool value);

    //    void Send(ushort packetId, short value);

    //    void Send(ushort packetId, ushort value);

    //    void Send(ushort packetId, uint value);

    //    void Send(ushort packetId, long value);

    //    void Send(ushort packetId, ulong value);

    //    void Send(ushort packetId, float value);

    //    void Send(ushort packetId, double value);

    //    void Send(ushort packetId, DateTime? value);

    //    void Send(ushort packetId, string value);

    //    void SendSerialize<O>(ushort packetId, O obj, string scheme);

    //    void Send(byte[] buf, int offset, int lenght);

    //    void Disconnect(bool disconnecteventcall = true);

    //    bool GetState();
    //}

}
