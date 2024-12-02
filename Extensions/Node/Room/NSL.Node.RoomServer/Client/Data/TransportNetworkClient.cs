using NSL.SocketCore.Utils.Buffer;
using System;
using NSL.Node.RoomServer.Shared.Client.Core;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;
using NSL.WebSockets.Server.AspNetPoint;
using NSL.UDP.Enums;
using NSL.UDP;
using NSL.SocketServer.Utils;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using System.Diagnostics;

namespace NSL.Node.RoomServer.Client.Data
{
    public partial class TransportNetworkClient : AspNetWSNetworkServerClient, INodeClientNetwork
    {
        public string Token { get; set; }

        public string Id { get; set; }

        public Guid RoomId { get; set; }

        public string NodeId { get; set; }

        public string EndPoint { get; set; }

        public bool Ready { get; set; }

        public RoomInfo Room { get; set; }

        public NodeInfo Node { get; set; }

        public bool IsLocalNode => false;

        public INodeNetworkClient UDPClient => throw new NotImplementedException();

        //static ConcurrentBag<long> items = new ();

        //static TransportNetworkClient()
        //{
        //    new Timer(state =>
        //    {
        //        var r = items.ToArray();

        //        Console.WriteLine($"Send time - {items.ToArray().Min()} - {items.ToArray().Max()} - {items.ToArray().Average()}");
        //    }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        //}


        public void Send(byte[] buffer)
        {
            //var sw = Stopwatch.StartNew();
            Network.Send(buffer);

            //items.Add(sw.ElapsedMilliseconds);
        }

        public void Send(byte[] buffer, int offset, int len)
        {
            Network.Send(buffer, offset, len);
        }

        public void Send(DgramOutputPacketBuffer packet, UDPChannelEnum channel = UDPChannelEnum.ReliableOrdered, bool disposeOnSend = true)
        {
            if (Network != null)
                Network.Send(packet, disposeOnSend);
            else if (disposeOnSend)
                packet.Dispose();
        }

        public void Send(ushort code, Action<DgramOutputPacketBuffer> build, UDPChannelEnum channel = UDPChannelEnum.ReliableOrdered)
        {
            Send(p =>
            {
                p.WriteUInt16(code);
                build(p);
            });
        }

        public void Send(Action<DgramOutputPacketBuffer> build, UDPChannelEnum channel = UDPChannelEnum.ReliableOrdered)
        {
            var packet = new DgramOutputPacketBuffer();

            packet.WriteString(Id);

            build(packet);

            packet.WithPid(RoomPacketEnum.TransportMessage);

            Send(packet, channel, true);
        }

        public void Send(DgramOutputPacketBuffer packet, bool disposeOnSend = true)
        {
            if (Network != null)
                Network.Send(packet, disposeOnSend);
            else if (disposeOnSend)
                packet.Dispose();
        }

        public void Send(OutputPacketBuffer packet, bool disposeOnSend = true)
        {
            if (Network != null)
                Network.Send(packet, disposeOnSend);
            else if (disposeOnSend)
                packet.Dispose();
        }

        public void Send(ushort code, Action<DgramOutputPacketBuffer> build)
        {
            Send(p =>
            {
                p.WriteUInt16(code);
                build(p);
            });
        }

        public void Send(Action<DgramOutputPacketBuffer> build)
        {
            var packet = new DgramOutputPacketBuffer();

            packet.WriteString(Id);

            build(packet);

            packet.WithPid(RoomPacketEnum.TransportMessage);

            Send(packet, true);
        }

        public void SetObjectOwner(INodeOwneredObject _object)
        {
            _object.SetOwner(Room, this);
        }

        public override void ChangeOwner(IServerNetworkClient from)
        {
            if (Room?.TryRecoverySession(this) != true)
            {
                Room = null;
            }

        }

        internal bool ManualDisconnected { get; private set; }

        public void Disconnect()
        {
            ManualDisconnected = true;

            Network?.Disconnect();
        }
    }
}
