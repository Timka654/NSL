using SocketCore;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketCore.Utils.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NSL.UDP
{
    public abstract class BaseUDPClient<TClient, TParent> : IClient<DgramPacket>
        where TClient : INetworkClient
        where TParent : BaseUDPClient<TClient, TParent>
    {
        public event ReceivePacketDebugInfo<TParent> OnReceivePacket;
        public event SendPacketDebugInfo<TParent> OnSendPacket;

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

        protected abstract TParent GetParent();

        protected CoreOptions<TClient> options;

        protected Dictionary<ushort, CoreOptions<TClient>.PacketHandle> PacketHandles;

        protected bool disconnected;

        private readonly TParent parent;
        private readonly IPEndPoint endPoint;
        private readonly Socket listenerSocket;

        public CoreOptions Options => options;

        public abstract void ChangeUserData(INetworkClient data);

        public abstract object GetUserData();

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
                InputPacketBuffer pbuff = new InputPacketBuffer(inputCipher.Decode(result, 0, result.Length));

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

        public void Send(DgramPacket packet)
        {
#if DEBUG
            OnSend(packet, Environment.StackTrace);
#else
            OnSend(packet, "");
#endif

            Send(packet.CompilePacket(), 0, packet.PacketLenght);
        }
        public void Send(OutputPacketBuffer packet)
        {
            if (!(packet is DgramPacket dpkg))
                throw new ArgumentException(nameof(packet));

            Send(dpkg);
        }

        public async void Send(byte[] buf, int offset, int lenght)
        {
            try
            {
                if (listenerSocket == null)
                    return;

                //шифруем данные
                byte[] sndBuffer = outputCipher.Encode(buf, offset, lenght);

                await Fragmentate(sndBuffer);
            }
            catch (Exception ex)
            {
                AddWaitPacket(buf, offset, lenght);
                RunException(ex);

                //отключаем клиента, лишним не будет
                Disconnect();
            }

        }

        ushort currentPID = 0;

        byte[] nlpBytes = new byte[] { 0 };

        private ushort GetPID()
        {
            lock (this)
            {
                return currentPID++;
            }
        }

        private async Task Fragmentate(byte[] arr)
        {
            var count = (int)Math.Ceiling(arr.Length * 1.0 / options.ReceiveBufferSize);

            var ppid = GetPID();

            var pidBytes = BitConverter.GetBytes(ppid);

            #region LP

            var lpBuf = (Memory<byte>)new byte[5];

            BitConverter.GetBytes((byte)1)
                .CopyTo(lpBuf);

            pidBytes
                .CopyTo(lpBuf[1..]);

            BitConverter.GetBytes((ushort)count)
                .CopyTo(lpBuf[3..]);

            await listenerSocket.SendToAsync(lpBuf.ToArray(), SocketFlags.None, endPoint);

            #endregion

            ushort h = default;
            for (int i = 0; i < arr.Length; i += options.ReceiveBufferSize)
            {
                var dest = i + options.ReceiveBufferSize > arr.Length ? arr[i..] : arr[i..(i + options.ReceiveBufferSize)];

                Memory<byte> pbuf = new byte[5 + dest.Length];

                nlpBytes
                    .CopyTo(pbuf);

                pidBytes
                    .CopyTo(pbuf[1..]);

                BitConverter.GetBytes(h++)
                    .CopyTo(pbuf[3..]);

                dest
                    .CopyTo(pbuf[5..]);

                await listenerSocket.SendToAsync(pbuf.ToArray(), SocketFlags.None, endPoint);
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
                int len = listenerSocket?.EndSend(r) ?? 0;
                //при некоторых ошибках размер возвращает 0 или -1, проверяем
                if (len < 1)
                    throw new ConnectionLostException(GetRemotePoint(), false);
            }
            catch (Exception ex)
            {
                var sas = ((SendAsyncState)r.AsyncState);
                AddWaitPacket(sas.buf, sas.offset, sas.len);
                RunException(ex);
                Disconnect();
            }
            catch
            {
                Disconnect();
            }
        }
    }
}
