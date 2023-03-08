using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Exceptions;
using NSL.UDP.Channels;
using NSL.UDP.Enums;
using NSL.UDP.Interface;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace NSL.UDP
{
    public abstract class BaseUDPClient<TClient, TParent> : IClient<DgramPacket>, IUDPClient
        where TClient : INetworkClient
        where TParent : BaseUDPClient<TClient, TParent>
    {
        public abstract TClient Data { get; }

        public event ReceivePacketDebugInfo<TParent> OnReceivePacket;
        public event SendPacketDebugInfo<TParent> OnSendPacket;

        #region Channels

        protected BaseChannel<TClient, TParent> reliableChannel;
		protected BaseChannel<TClient, TParent> unreliableChannel;

		#endregion

		#region Network

		public const int PHeadLenght = 5;

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

        ///// <summary>
        ///// Буффер для приема данных
        ///// </summary>
        //protected byte[] receiveBuffer;

        ///// <summary>
        ///// Текущее положение в буффере, для метода BeginReceive
        ///// </summary>
        //protected int offset;

        ///// <summary>
        ///// Размер читаемых данных при следующем вызове BeginReceive
        ///// </summary>
        //protected int lenght = InputPacketBuffer.headerLenght;

        //protected bool data = false;

        #endregion

        #endregion

        public BaseUDPClient(IPEndPoint endPoint, Socket listenerSocket)
        {
            this.parent = GetParent();
            this.endPoint = endPoint;
            this.listenerSocket = listenerSocket;
        }

        protected virtual void Initialize()
        {
			reliableChannel = new ReliableChannel<TClient, TParent>(this);
			unreliableChannel = new UnreliableChannel<TClient, TParent>(this);
        }


        protected abstract TParent GetParent();

        protected CoreOptions<TClient> options;

        protected Dictionary<ushort, CoreOptions<TClient>.PacketHandle> PacketHandles;

        protected bool disconnected;

        private readonly TParent parent;
        private readonly IPEndPoint endPoint;
        private readonly Socket listenerSocket;

        public CoreOptions Options => options;

        public abstract void ChangeUserData(INetworkClient data);

        public object GetUserData() => Data;

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
        }

        public IPEndPoint GetRemotePoint() => endPoint;

        public Socket GetSocket() => null;

        public bool GetState()
        {
            throw new NotImplementedException();
        }

        public virtual void Receive(byte[] result)
        {
            //замыкаем это все в блок try, если клиент отключился то EndReceive может вернуть ошибку
            try
            {
                //дешефруем и засовываем это все в спец буффер в котором реализованы методы чтения типов, своего рода поток
                InputPacketBuffer pbuff = new InputPacketBuffer(inputCipher.Decode(result, 0, result.Length), true);

                OnReceive(pbuff.PacketId, pbuff.Lenght);

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

        public void Send(DgramPacket packet, bool disposeOnSend = true)
        {
#if DEBUG
            OnSend(packet, Environment.StackTrace);
#else
            OnSend(packet, "");
#endif

            packet.Send(this, disposeOnSend);
        }

        public void Send(OutputPacketBuffer packet, bool disposeOnSend = true)
        {
            if (!(packet is DgramPacket dpkg))
                throw new ArgumentException(nameof(packet));

            Send(dpkg, disposeOnSend);
        }

        public void Send(UDPChannelEnum channel, byte[] buffer)
        {
            var sndBuf = outputCipher.Encode(buffer, 0, buffer.Length);

            if (channel.HasFlag(UDPChannelEnum.Reliable))
                reliableChannel.Send(channel, sndBuf);
            else
                unreliableChannel.Send(channel, sndBuf);
        }

        public void Send(byte[] buffer)
            => throw new NotImplementedException();

        public void Send(byte[] buf, int offset, int lenght)
            => throw new NotImplementedException();

        internal void SocketSend(byte[] sndBuffer)
        {
            try
            {
                if (listenerSocket == null)
                    return;

                listenerSocket.SendTo(sndBuffer, SocketFlags.None, endPoint);
            }
            catch (Exception ex)
            {
                AddWaitPacket(sndBuffer, 0, sndBuffer.Length);
                RunException(ex);

                //отключаем клиента, лишним не будет
                Disconnect();
            }
        }

        public void SendEmpty(ushort packetId)
        {
            DgramPacket rbuff = new DgramPacket
            {
                PacketId = packetId
            };

            Send(rbuff);
        }

        protected virtual void OnReceive(ushort pid, int len)
        {
            OnReceivePacket?.Invoke(parent, pid, len);
        }

        protected abstract void RunDisconnect();

        protected abstract void RunException(Exception ex);

        protected abstract void AddWaitPacket(byte[] buffer, int offset, int length);

        public short GetTtl() => listenerSocket.Ttl;

        protected virtual void OnSend(DgramPacket rbuff, string stackTrace = "")
        {
            OnSendPacket?.Invoke(parent, rbuff.PacketId, rbuff.PacketLenght, stackTrace);
        }
    }
}
