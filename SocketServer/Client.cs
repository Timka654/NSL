﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SocketServer.Utils;
using SocketServer.Utils.Buffer;

namespace SocketServer
{
#if DEBUG
    public delegate void ReceivePacketDebugInfo<T>(Client<T> client, ushort pid, int len) where T : INetworkClient;
    public delegate void SendPacketDebugInfo<T>(Client<T> client, ushort pid, int len, string memberName, string sourceFilePath, int sourceLineNumber) where T : INetworkClient;
#endif

    /// <summary>
    /// Класс обработки клиента
    /// </summary>
    public class Client<T> : IClient where T : INetworkClient
    {
        private T Data { get; set; }

#if DEBUG
        public event ReceivePacketDebugInfo<T> OnReceivePacket;
        public event SendPacketDebugInfo<T> OnSendPacket;
#endif

        /// <summary>
        /// Общие настройки сервера
        /// </summary>
        private ServerOptions<T> serverOptions;

        /// <summary>
        /// Инициализация прослушивания клиента
        /// </summary>
        /// <param name="client">клиент</param>
        /// <param name="options">общие настройки сервера</param>
        public Client(Socket client, ServerOptions<T> options)
        {
            Data = Activator.CreateInstance<T>();

            //установка переменной с общими настройками сервера
            this.serverOptions = options;

            //обзятельная переменная в NetworkClient, для отправки данных, можно использовать привидения типов (Client)NetworkClient но это никому не поможет
            this.Data.Network = this;

            //установка переменной содержащую поток клиента
            this.sclient = client;

            //установка массива для приема данных, размер указан в общих настройках сервера
            this.receiveBuffer = new byte[options.ReceiveBufferSize];
            //установка криптографии для дешифровки входящих данных, указана в общих настройках сервера
            this.inputCipher = (IPacketCipher)options.inputCipher.Clone();
            //установка криптографии для шифровки исходящих данных, указана в общих настройках сервера
            this.outputCipher = (IPacketCipher)options.outputCipher.Clone();

            //Bug fix, в системе Windows это значение берется из реестра, мы не сможем принять больше за раз чем прописанно в нем, если данных будет больше, то цикл приема зависнет
            sclient.ReceiveBufferSize = options.ReceiveBufferSize;

            //Bug fix, отключение буфферизации пакетов для уменьшения трафика, если не отключить то получим фризы, в случае с игровым соединением эту опцию обычно нужно отключать
            sclient.NoDelay = true;

            //Начало приема пакетов от клиента
            options.RunClientConnect(Data);
        }

        public void RunPacketReceiver()
        {
            lenght = InputPacketBuffer.headerLenght;
            offset = 0;
            data = false;

            sclient.BeginReceive(receiveBuffer, offset, lenght, SocketFlags.None, Receive, sclient);
        }

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
        /// Текущее состояние подключения
        /// </summary>
        public bool GetState()
        {
            if (sclient == null)
                return false;
            return sclient.Connected;
        }

        public IPEndPoint GetRemovePoint()
        {
            try
            {
            if (sclient != null && sclient.RemoteEndPoint != null)
                return (IPEndPoint)sclient.RemoteEndPoint;

            }
            catch
            {
            }
            return new IPEndPoint(0, 0);
        }

        public void ChangeUserData(INetworkClient old_client_data)
        {
            if (old_client_data == null)
            {
                this.Data = null;
                return;
            }
            old_client_data.ChangeOwner(this.Data);
        }

        public object GetUserData()
        {
            return this.Data;
        }

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

        /// <summary>
        /// прием хедера пакета, который обычно содержит размер и индификатор пакета, etc
        /// </summary>
        /// <param name="result"></param>
        private void Receive(IAsyncResult result)
        {
            //замыкаем это все в блок try, если клиент отключился то EndReceive может вернуть ошибку
            try
            {
                //принимаем размер данных которые удалось считать
                int rlen = sclient.EndReceive(result);
                //при некоторых ошибках размер возвращает 0 или -1, проверяем
                if (rlen < 1)
                    throw new Exception();
                //добавляем offset для дальнейшей считки пакета
                offset += rlen;
                //если полученный размер меньше размера пакета, дополучаем данные
                if (offset < lenght)
                {
                    sclient.BeginReceive(receiveBuffer, offset, lenght - offset, SocketFlags.None, Receive, sclient);

                    return;
                }

                if (!data)
                {
                    //если все ок
                    //получаем размер пакета
                    byte[] peeked = inputCipher.Peek(receiveBuffer);
                    lenght = BitConverter.ToInt32(peeked, 0);
#if DEBUG
                    OnReceivePacket?.Invoke(this, BitConverter.ToUInt16(peeked, 4), lenght);
#endif
                    data = true;
                }

                //если пакет не принимает никаких данных, бывают пустые пакеты, но такой пакет есть, запускаем исполнение
                if (offset == lenght && data)
                {
                    //обработка пакета
                    //дешефруем и засовываем это все в спец буффер в котором реализованы методы чтения типов, своего рода поток
                    InputPacketBuffer pbuff = new InputPacketBuffer(inputCipher.Decode(receiveBuffer, 0, lenght));
                    Data.LastReceiveMessage = DateTime.Now;
                    // обнуляем показатели что-бы успешно запустить цикл заново
                    lenght = InputPacketBuffer.headerLenght;
                    offset = 0;

                    //ищем пакет и выполняем его, передаем ему данные сессии, полученные данные, и просим у него данные для передачи
                    serverOptions.Packets[pbuff.PacketId].Receive(Data, pbuff);
                    
                    data = false;
                    //перезапускаем последовательность
                }
                sclient.BeginReceive(receiveBuffer, offset, lenght - offset, SocketFlags.None, Receive, sclient);
            }
            catch(Exception ex)
            {
                serverOptions.RunExtension(ex, Data);
                //отключаем клиента, в случае ошибки не в транспортном потоке а где-то в пакете, что-бы клиент не завис 
                Disconnect();
            }
        }

        /// <summary>
        /// Отправка пакета
        /// </summary>
        /// <param name="rbuff">спец буффер содержащий в себе данные пакета</param>
#if DEBUG
        public void Send(OutputPacketBuffer rbuff, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
#else
        public void Send(OutputPacketBuffer rbuff)
#endif
        {
#if DEBUG
            OnSendPacket?.Invoke(this, rbuff.PacketId, rbuff.PacketLenght, memberName, sourceFilePath, sourceLineNumber);
#endif
            Send(rbuff.CompilePacket(), 0, rbuff.PacketLenght);
        }

#if DEBUG
        public void SendSerialize(ushort packetId, object obj, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
#else
        public void Send(ushort packetId,object obj)
#endif
        {
            var rbuff = new OutputPacketBuffer { PacketId = packetId };
            rbuff.Serialize(obj);

#if DEBUG
            OnSendPacket?.Invoke(this, rbuff.PacketId, rbuff.PacketLenght, memberName, sourceFilePath, sourceLineNumber);
#endif
            Send(rbuff.CompilePacket(), 0, rbuff.PacketLenght);

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
                sclient.BeginSend(sndBuffer, 0, lenght, SocketFlags.None, EndSend, sclient);
                
            }
            catch
            {
                Data?.AddWaitPacket(buf, lenght);

                //отключаем клиента, в случае ошибки не в транспортном потоке а где-то в пакете, что-бы клиент не завис 
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
                {
                    throw new Exception();
                }
            }
            catch
            {
                //отключаем клиента, лишним не будет
                Disconnect();
            }
        }

        /// <summary>
        /// Отключить клиента
        /// </summary>
        public void Disconnect()
        {
            this.serverOptions.RunClientDisconnect(Data);
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

        public Socket GetSocket()
        {
            return sclient;
        }
    }

    public interface IClient
    {
#if DEBUG
        void Send(OutputPacketBuffer packet, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0);
#else
        void Send(OutputPacketBuffer packet);
#endif

#if DEBUG
        void SendSerialize(ushort packetId, object obj, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0);
#else
        void SendSerialize(ushort packetId,object obj);
#endif

        void Send(byte[] buf, int offset, int lenght);

        void Disconnect();

        bool GetState();

        IPEndPoint GetRemovePoint();

        void ChangeUserData(INetworkClient data);

        object GetUserData();

        Socket GetSocket();
    }
}
