using SCL.SocketClient.Utils;
using SCL.SocketClient.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace SCL.SocketClient
{
#if DEBUG
    public delegate void ReceivePacketDebugInfo<T>(Client<T> client, ushort pid, int len) where T : BaseSocketNetworkClient;
    public delegate void SendPacketDebugInfo<T>(Client<T> client, ushort pid, int len) where T : BaseSocketNetworkClient;
#endif
    /// <summary>
    /// Класс обработки клиента
    /// </summary>
    public class Client<T> : IClient
        where T : BaseSocketNetworkClient
    {

#if DEBUG
        public event ReceivePacketDebugInfo<T> OnReceivePacket;
        public event SendPacketDebugInfo<T> OnSendPacket;
#endif

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

            options.AddPacket((ushort) Utils.SystemPackets.Enums.ServerPacketEnum.SystemTime,
                new Utils.SystemPackets.SystemTime<T>(clientOptions));
            options.AddPacket((ushort)Utils.SystemPackets.Enums.ServerPacketEnum.AliveConnection,
                new Utils.SystemPackets.AliveConnection<T>(clientOptions));
            options.AddPacket((ushort)Utils.SystemPackets.Enums.ServerPacketEnum.RecoverySessionResult,
                new Utils.SystemPackets.RecoverySession<T>(clientOptions));
        }

        /// <summary>
        /// Запуск цикла приема пакетов
        /// </summary>
        /// <param name="client">клиент</param>
        public void Reconnect(Socket client)
        {
            string[] keys = clientOptions.ClientData?.RecoverySessionKeyArray;
            IEnumerable<byte[]> waitBuffer = clientOptions.ClientData?.GetWaitPackets();

            clientOptions.ClientData = Activator.CreateInstance<T>();

            clientOptions.ClientData.RecoverySessionKeyArray = keys;
            clientOptions.ClientData.Network = this;

            if(waitBuffer != null)
                foreach (var item in waitBuffer)
                {
                    clientOptions.ClientData.AddWaitPacket(item, item.Length);
                }
            
            //установка переменной содержащую поток клиента
            this.sclient = client;

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
                //принимаем размер данных которые удалось считать
                int rlen = sclient.EndReceive(result);
                //при некоторых ошибках размер возвращает 0 или -1, проверяем
                if (rlen < 1)
                {
                    Disconnect();
                    return;
                }

                //добавляем offset для дальнейшей считки пакета
                offset += rlen;

                //если размер ожидаемвых данных соответствует ожидаемому - обрабатываем
                if (offset == lenght)
                {
                    if(data == false)
                    {
                        var peeked = inputCipher.Peek(receiveBuffer);
                        //если все ок
                        //получаем размер пакета
                        lenght = BitConverter.ToInt32(peeked, 0);
    #if DEBUG
                        int len = lenght;
                        ushort pid = BitConverter.ToUInt16(peeked, 4);
                        ThreadHelper.InvokeOnMain(() =>
                        {
                            OnReceivePacket?.Invoke(this, pid, len);
                        });
    #endif
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
                        lenght = InputPacketBuffer.headerLenght;
                        offset = 0;
                        data = false;


                        //ищем пакет и выполняем его, передаем ему данные сессии, полученные данные, и просим у него данные для передачи
                        clientOptions.PacketHandles[pbuff.PacketId](pbuff);
                    }
                }
                sclient.BeginReceive(receiveBuffer, offset, lenght - offset, SocketFlags.None, Receive, sclient);
            }
            catch (Exception ex)
            {
                clientOptions.RunExtension(ex);

                //отключаем клиента, в случае ошибки не в транспортном потоке а где-то в пакете, что-бы клиент не завис 
                Disconnect();
            }
        }

        /// <summary>
        /// Отправка пакета
        /// </summary>
        /// <param name="rbuff">спец буффер содержащий в себе данные пакета</param>
        public void Send(OutputPacketBuffer rbuff)
        {
#if DEBUG
            ThreadHelper.InvokeOnMain(() => { OnSendPacket?.Invoke(this, rbuff.PacketId, rbuff.PacketLenght); });
#endif

            Send(rbuff.CompilePacket(), 0, rbuff.PacketLenght);
        }

        public void SendSerialize<O>(ushort packetId, O obj, string scheme)
        {
            var rbuff = new OutputPacketBuffer { PacketId = packetId };

            rbuff.Serialize<O>(obj, scheme);

#if DEBUG
            ThreadHelper.InvokeOnMain(() => { OnSendPacket?.Invoke(this, rbuff.PacketId, rbuff.PacketLenght); });
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
            catch (Exception ex)
            {
                clientOptions.ClientData.AddWaitPacket(buf, lenght);
                clientOptions.RunExtension(ex);

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
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                clientOptions.RunExtension(ex);

                //отключаем клиента, лишним не будет
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
            if(disconnecteventcall)
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

            Send(rbuff.CompilePacket(), 0, rbuff.PacketLenght);
        }

        public void Send(ushort packetId, int value)
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteInt32(value);

            Send(rbuff.CompilePacket(), 0, rbuff.PacketLenght);
        }

        public void Send(ushort packetId, byte value)
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteByte(value);

            Send(rbuff.CompilePacket(), 0, rbuff.PacketLenght);
        }

        public void Send(ushort packetId, bool value)
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteBool(value);

            Send(rbuff.CompilePacket(), 0, rbuff.PacketLenght);
        }

        public void Send(ushort packetId, short value)
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteInt32(value);

            Send(rbuff.CompilePacket(), 0, rbuff.PacketLenght);
        }

        public void Send(ushort packetId, ushort value)
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteInt32(value);

            Send(rbuff.CompilePacket(), 0, rbuff.PacketLenght);
        }

        public void Send(ushort packetId, uint value)
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteUInt32(value);

            Send(rbuff.CompilePacket(), 0, rbuff.PacketLenght);
        }

        public void Send(ushort packetId, long value)
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteInt64(value);

            Send(rbuff.CompilePacket(), 0, rbuff.PacketLenght);
        }

        public void Send(ushort packetId, ulong value)
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteUInt64(value);

            Send(rbuff.CompilePacket(), 0, rbuff.PacketLenght);
        }

        public void Send(ushort packetId, float value)
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteFloat(value);

            Send(rbuff.CompilePacket(), 0, rbuff.PacketLenght);
        }

        public void Send(ushort packetId, double value)
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteDouble(value);

            Send(rbuff.CompilePacket(), 0, rbuff.PacketLenght);
        }

        public void Send(ushort packetId, DateTime? value)
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteDateTime(value);

            Send(rbuff.CompilePacket(), 0, rbuff.PacketLenght);
        }

        public void Send(ushort packetId, string value)
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteString16(value);

            Send(rbuff.CompilePacket(), 0, rbuff.PacketLenght);
        }

        #endregion

    }

    public interface IClient
    {
        void Send(OutputPacketBuffer packet);

        void SendEmpty(ushort packetId);

        void Send(ushort packetId, byte value);

        void Send(ushort packetId, int value);

        void Send(ushort packetId, bool value);

        void Send(ushort packetId, short value);

        void Send(ushort packetId, ushort value);

        void Send(ushort packetId, uint value);

        void Send(ushort packetId, long value);

        void Send(ushort packetId, ulong value);

        void Send(ushort packetId, float value);

        void Send(ushort packetId, double value);

        void Send(ushort packetId, DateTime? value);

        void Send(ushort packetId, string value);

        void SendSerialize<O>(ushort packetId, O obj, string scheme);

        void Send(byte[] buf, int offset, int lenght);

        void Disconnect(bool disconnecteventcall = true);

        bool GetState();
    }
}
