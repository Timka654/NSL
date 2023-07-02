using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using System.Net;
using System.Net.Sockets;
using System;
using System.Collections.Generic;

namespace NSL.LocalBridge
{
    public class LocalBridgeClient<TClient, TOClient> : IClient<OutputPacketBuffer>
        where TClient : INetworkClient, new()
        where TOClient : INetworkClient, new()
    {
        public LocalBridgeClient(CoreOptions options) : this(options, null, null)
        { }

        public LocalBridgeClient(CoreOptions options, IPEndPoint connectionEndPoint) : this(options, connectionEndPoint, null)
        { }

        public LocalBridgeClient(CoreOptions options, IPEndPoint connectionEndPoint, LocalBridgeClient<TOClient, TClient> otherClient)
        {
            normalOptions = options as CoreOptions<TClient>;

            clientData = new TClient();

            clientData.Network = this;

            PacketHandles = normalOptions.GetHandleMap();

            this.connectionEndPoint = connectionEndPoint ?? new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);

            if (otherClient != null)
                SetOtherClient(otherClient);
        }

        public void SetOtherClient(LocalBridgeClient<TOClient, TClient> otherClient)
        {
            if (this.otherClient == otherClient)
                return;

            if (otherClient == null)
                throw new ArgumentNullException(nameof(otherClient), $"Use {nameof(Disconnect)} method for clear data");

            this.otherClient = otherClient;

            otherClient.SetOtherClient(this);

            normalOptions.RunClientConnect(clientData);
        }

        public CoreOptions Options => normalOptions;

        private CoreOptions<TClient> normalOptions;

        private TClient clientData;
        private LocalBridgeClient<TOClient, TClient> otherClient;
        private readonly IPEndPoint connectionEndPoint;


        private Dictionary<ushort, CoreOptions<TClient>.PacketHandle> PacketHandles;

        public void ChangeUserData(INetworkClient setClient)
        {
            if (setClient is TClient valid)
            {
                clientData = valid;
            }
            else
                throw new ArgumentException("Invalid type", nameof(setClient));
        }

        public void Disconnect()
        {
            var c = otherClient;

            if (c == null)
                return;

            otherClient = null;

            normalOptions.RunClientDisconnect(clientData);

            c.Disconnect();
        }

        public IPEndPoint GetRemotePoint()
            => connectionEndPoint;

        public Socket GetSocket()
            => new Socket(SocketType.Unknown, ProtocolType.Unknown);

        public bool GetState()
            => otherClient != null;

        public short GetTtl()
            => -1;

        public object GetUserData()
            => clientData;

        public void Send(OutputPacketBuffer packet, bool disposeOnSend = true)
        {
            Send(packet.CompilePacket());

            if (disposeOnSend)
                packet.Dispose();
        }

        public void Send(byte[] buf)
        {
            Send(buf, 0, buf.Length);
        }

        public void Send(byte[] buf, int offset, int length)
        {
            otherClient.Receive(buf[offset..length]);
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

            //предотвращение ошибок в пакете
            try
            {
                //ищем пакет и выполняем его, передаем ему данные сессии, полученные данные
                PacketHandles[pbuff.PacketId](clientData, pbuff);
            }
            catch (Exception ex)
            {
                normalOptions.RunException(ex, clientData);
            }

            if (!pbuff.ManualDisposing)
                pbuff.Dispose();
        }

    }
}