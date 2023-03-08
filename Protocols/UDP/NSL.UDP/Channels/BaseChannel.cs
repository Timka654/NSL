using NSL.SocketCore.Utils;
using NSL.UDP.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using NSL.UDP.Interface;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using NSL.UDP.Packet;

namespace NSL.UDP.Channels
{
    public abstract class BaseChannel<TClient, TParent>
        where TClient : INetworkClient
        where TParent : BaseUDPClient<TClient, TParent>
    {
        protected readonly BaseUDPClient<TClient, TParent> udpClient;

        protected readonly IUDPOptions UDPOptions;

        public abstract UDPChannelEnum Channel { get; }

        public Action<uint> OnReceive;

        public BaseChannel(BaseUDPClient<TClient, TParent> udpClient)
        {
            this.udpClient = udpClient;

            UDPOptions = udpClient.Options as IUDPOptions;

            receivePidBuffer.Enqueue(uint.MaxValue);
        }

        protected virtual void AfterBuild(BaseChannel<TClient, TParent> fromChannel, PacketWaitTemp packet) { }

        protected virtual void InvalidRecvChecksum(BaseChannel<TClient, TParent> fromChannel, Span<byte> data) { }

        public virtual void Send(UDPChannelEnum channel, byte[] data)
        {
            var count = (int)Math.Ceiling((double)data.Length / UDPOptions.SendFragmentSize);

            var ppid = CreatePID();

            var pidBytes = BitConverter.GetBytes(ppid);

            var packet = new PacketWaitTemp()
            {
                PID = ppid,
                Head = LPacket.CreateHeader(pidBytes, (byte)channel, (ushort)count),
                Parts = DataPacket.CreateParts(pidBytes, (byte)channel, data, UDPOptions)
            };

            AfterBuild(this, packet);

            SendFull(packet);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SendFull(PacketWaitTemp packet)
        {
            SendHead(packet);

            SendParts(packet);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SendHead(PacketWaitTemp packet)
        {
            udpClient.SocketSend(packet.Head.ToArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SendParts(PacketWaitTemp packet)
        {
            Parallel.ForEach(packet.Parts, data => udpClient.SocketSend(data.ToArray()));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SendParts(PacketWaitTemp packet, int fromPartInc, int toPartExc)
        {
            Parallel.For(fromPartInc, toPartExc, idx => udpClient.SocketSend(packet.Parts.ElementAt(idx).ToArray()));
        }

        protected ConcurrentDictionary<uint, PacketReciveTemp> packetReceiveBuffer = new ConcurrentDictionary<uint, PacketReciveTemp>();

        private ConcurrentQueue<uint> receivePidBuffer = new ConcurrentQueue<uint>();

        public virtual void Receive(UDPChannelEnum channel, Span<byte> data)
        {
            var checksum = UDPPacket.ReadChecksum(data);

            if (!checksum.Equals(UDPPacket.GetChecksum(data)))
            {
                InvalidRecvChecksum(this, data);
                return;
            }

            var pid = UDPPacket.ReadPID(data);

            if (receivePidBuffer.Contains(pid))
                return;

            var packet = packetReceiveBuffer.GetOrAdd(
                pid,
                id => new PacketReciveTemp(id));

            Console.WriteLine($"{channel} received {pid} lp:{LPacket.ReadISLP(data)}");

            if (LPacket.ReadISLP(data))
            {
                packet.Lenght = LPacket.ReadPacketLen(data);
            }
            else
            {
                lock (packet.ContainsParts)
                {
                    if (packet.ContainsParts.Contains(pid))
                        return;

                    packet.ContainsParts.Add(pid);

                    packet.Parts.Add(data[6..].ToArray());
                }
            }

            ProcessPacket(channel, packet);
        }

        protected virtual void ProcessPacket(UDPChannelEnum channel, PacketReciveTemp packet)
        {
            lock (this)
            {
                if (channel.HasFlag(UDPChannelEnum.Ordered | UDPChannelEnum.Reliable))
                {
                    if (!receivePidBuffer.Contains(packet.PID - 1))
                    {
                        Action<uint> rcvHandle = default;

                        rcvHandle = (pid) =>
                        {
                            Console.WriteLine($"{nameof(rcvHandle)} - {pid}");
                            if (pid - 1 == packet.PID)
                                ProcessPacket(channel, packet);
                            OnReceive -= rcvHandle;
                        };

                        OnReceive += rcvHandle;

                        return;
                    }
                }

                if (packet.Ready() &&
                    packetReceiveBuffer.TryRemove(packet.PID, out packet))
                {
                    if (receivePidBuffer.Contains(packet.PID))
                    {
                        packetReceiveBuffer.TryRemove(packet.PID, out _);
                        return;
                    }

                    receivePidBuffer.Enqueue(packet.PID);

                    while (receivePidBuffer.Count > 100)
                        receivePidBuffer.TryDequeue(out _);

                    udpClient.Receive(packet.Parts
                    .OrderBy(x => PacketReciveTemp.ReadPartDataOffset(x))
                    .SelectMany(x => x[2..].ToArray())
                    .ToArray());

                    OnReceive(packet.PID);
                }
            }
        }

        internal virtual uint CreatePID() => 0;

        public struct PacketWaitTemp
        {
            public uint PID;

            public Memory<byte> Head;

            public IEnumerable<Memory<byte>> Parts;
        }

        protected class PacketReciveTemp
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ushort ReadPartDataOffset(Memory<byte> buffer) => BitConverter.ToUInt16(buffer.Span);

            public uint PID;

            public ushort Lenght;

            public ConcurrentBag<Memory<byte>> Parts;

            public ConcurrentBag<ushort> ContainsParts;

            public PacketReciveTemp(uint PID, ushort len) : this(PID)
            {
                this.Lenght = len;
            }

            public PacketReciveTemp(uint PID)
            {
                this.PID = PID;
                this.Lenght = 0;
                Parts = new ConcurrentBag<Memory<byte>>();
                ContainsParts = new ConcurrentBag<ushort>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Ready() => Lenght > 0 && Parts.Count == Lenght;
        }
    }
}
