using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using System.Net;
using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Threading;

namespace NSL.LocalBridge
{
    public class LocalBridgeClient<TClient, TOClient> : IClient<OutputPacketBuffer>
        where TClient : BaseNetworkConnection, new()
        where TOClient : BaseNetworkConnection, new()
    {
        public LocalBridgeClient(CoreOptions options) : this(options, null, null)
        { }

        public LocalBridgeClient(CoreOptions options, IPEndPoint connectionEndPoint) : this(options, connectionEndPoint, null)
        { }

        public LocalBridgeClient(CoreOptions options, IPEndPoint connectionEndPoint, LocalBridgeClient<TOClient, TClient> otherClient)
        {
            normalOptions = options;

            OnReceivePacket += (client, pid, len) => options.CallReceivePacketEvent(clientData, pid, len);
            OnSendPacket += (client, pid, len, st) => options.CallSendPacketEvent(clientData, pid, len, st);

            clientData = new TClient();

            clientData.Network = this;

            PacketHandles = normalOptions.GetHandleMap();

            this.connectionEndPoint = connectionEndPoint ?? new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);

            if (otherClient != null)
                SetOtherClient(otherClient);
        }

        public event ReceivePacketDebugInfo<LocalBridgeClient<TClient, TOClient>> OnReceivePacket;
        public event SendPacketDebugInfo<LocalBridgeClient<TClient, TOClient>> OnSendPacket;

        public void SetOtherClient(LocalBridgeClient<TOClient, TClient> otherClient)
        {
            if (this.otherClient == otherClient)
                return;

            if (otherClient == null)
                throw new ArgumentNullException(nameof(otherClient), $"Use {nameof(Disconnect)} method for clear data");

            this.otherClient = otherClient;

            otherClient.SetOtherClient(this);

            normalOptions.CallClientConnectEvent(clientData);
        }

        public CoreOptions Options => normalOptions;

        private CoreOptions normalOptions;

        private TClient clientData;
        private LocalBridgeClient<TOClient, TClient> otherClient;
        private readonly IPEndPoint connectionEndPoint;


        private Dictionary<ushort, CoreOptions.PacketHandle> PacketHandles;

        public void SetClientData(BaseNetworkConnection from)
        {
            if (from == null)
            {
                clientData = null;
                return;
            }

            if (from is TClient td)
            {
                // current data for dispose and move data
                var oldData = clientData;

                clientData = td;
                clientData.Network = this;

                oldData.Network = null;

                from.ChangeOwner(oldData);

                return;
            }

            throw new Exception($"{nameof(from)} must have type {typeof(TClient)}");
        }

        public void Disconnect()
        {
            var c = Interlocked.Exchange(ref otherClient, null);

            if (c == null)
                return;

            normalOptions.CallClientDisconnectEvent(clientData);

            c.Disconnect();
        }

        public IPEndPoint GetRemotePoint()
            => connectionEndPoint;

        public Socket GetSocket()
            => null;

        public bool GetState()
            => otherClient != null;

        public short GetTtl()
            => -1;

        public object GetUserData()
            => clientData;

        public void Send(OutputPacketBuffer packet, bool disposeOnSend = true)
        {
#if DEBUG
            OnSend(packet, Environment.StackTrace);
#else
            OnSend(packet, "");
#endif

            try
            {
                Send(packet.CompilePacket());
            }
            finally
            {
                if (disposeOnSend)
                    packet.Dispose();
            }
        }

        public void Send(byte[] buf)
        {
            otherClient?.Receive(buf);
        }

        public void Send(byte[] buf, int offset, int length)
        {
            if (buf.Length == length && offset == 0)
            {
                Send(buf);
                return;
            }

            var buf2 = new byte[length];

            Array.Copy(buf, offset, buf2, 0, length);

            Send(buf2);
        }

        public void SendEmpty(ushort packetId)
        {
            var p = new OutputPacketBuffer();

            p.PacketId = packetId;

            Send(p);
        }

        private void Receive(byte[] buf)
        {
            var pbuff = new InputPacketBuffer(buf);

            pbuff.SetData(buf[InputPacketBuffer.DefaultHeaderLength..]);

            OnReceive(pbuff.PacketId, pbuff.PacketLength);

            //предотвращение ошибок в пакете
            try
            {
                //ищем пакет и выполняем его, передаем ему данные сессии, полученные данные
                if (!PacketHandles.TryGetValue(pbuff.PacketId, out var handle))
                    return;

                handle(clientData, pbuff);
            }
            catch (Exception ex)
            {
                normalOptions.CallExceptionEvent(ex, clientData);
            }

            if (!pbuff.ManualDisposing)
                pbuff.Dispose();
        }

        protected virtual void OnSend(OutputPacketBuffer rbuff, string stackTrace = "")
        {
            OnSendPacket?.Invoke(this, rbuff.PacketId, rbuff.PacketLength, stackTrace);
        }

        protected virtual void OnReceive(ushort pid, int len)
        {
            OnReceivePacket?.Invoke(this, pid, len);
        }

    }
}