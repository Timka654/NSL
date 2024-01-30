using NSL.UDP.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using NSL.UDP.Packet;
using System.Threading;
using NSL.SocketServer.Utils;

namespace NSL.UDP.Channels
{
    public delegate void OnSendDelegate<TClient, TParent>(BaseChannel<TClient, TParent> fromChannel, PacketWaitTemp packet)
        where TClient : IServerNetworkClient
        where TParent : BaseUDPClient<TClient, TParent>;

    public abstract class BaseChannel<TClient, TParent>
        where TClient : IServerNetworkClient
        where TParent : BaseUDPClient<TClient, TParent>
    {
        protected readonly BaseUDPClient<TClient, TParent> udpClient;

        protected readonly UDPClientOptions<TClient> UDPOptions;

        public abstract UDPChannelEnum Channel { get; }

        public Action<uint> OnReceive;

        public OnSendDelegate<TClient, TParent> OnSend = (channel, packet) => { };

        public BaseChannel(BaseUDPClient<TClient, TParent> udpClient)
        {
            this.udpClient = udpClient;

            UDPOptions = udpClient.Options as UDPClientOptions<TClient>;
        }

        protected virtual void InvalidRecvChecksum(BaseChannel<TClient, TParent> fromChannel, Span<byte> data) { }

        public virtual void Send(UDPChannelEnum channel, byte[] data)
        {
            var count = UDPOptions.SendFragmentSize - data.Length;

            // full packet
            if (count >= UDPPacket.BaseHeadLen + 2)
                count = 0;
            else
                count = (int)Math.Ceiling((double)data.Length / UDPOptions.SendFragmentSize);

            var ppid = CreatePID();

            var pidBytes = BitConverter.GetBytes(ppid);

            var packet = new PacketWaitTemp()
            {
                PID = ppid,
                Head = count > 0 ? LPacket.CreateHeader(pidBytes, (byte)channel, (ushort)count) : UDPPacket.CreateFull(pidBytes, (byte)channel, data),
                Parts = count > 0 ? DataPacket.CreateParts(pidBytes, (byte)channel, data, UDPOptions) : default
            };

            //if (packet.PID == uint.MaxValue || packet.PID == uint.MinValue)
            //{
            //    Debug.Log($"send {packet.PID} - prev {packet.PID - 1} - next {packet.PID + 1}");
            //} //ok

            OnSend(this, packet);

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
            udpClient.SocketSend(packet.Head.ToArray(), packet);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SendParts(PacketWaitTemp packet)
        {
            if (packet.Parts == null)
                return;

            Parallel.ForEach(packet.Parts, data => udpClient.SocketSend(data.ToArray(), packet));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SendParts(PacketWaitTemp packet, int fromPartInc, int toPartExc)
        {
            if (packet.Parts == null)
                return;

            Parallel.For(fromPartInc, toPartExc, idx => udpClient.SocketSend(packet.Parts.ElementAt(idx).ToArray(), packet));
        }

        protected ConcurrentDictionary<uint, PacketReciveTemp> packetReceiveBuffer = new ConcurrentDictionary<uint, PacketReciveTemp>();

        public virtual void Receive(UDPChannelEnum channel, Span<byte> data)
        {
            var checksum = UDPPacket.ReadChecksum(data);

            if (!checksum.Equals(UDPPacket.GetChecksum(data)))
            {
                InvalidRecvChecksum(this, data);
                return;
            }

            var pid = UDPPacket.ReadPID(data);

            var packet = packetReceiveBuffer.GetOrAdd(
                pid,
                id => new PacketReciveTemp(id));

            if (UDPPacket.ReadISFull(data))
            {
                var poffset = DataPacket.ReadPOffset(data);

                lock (packet.ContainsParts)
                {
                    if (packet.ContainsParts.Contains(poffset))
                        return;

                    packet.Lenght = 1;

                    packet.Parts.Add(data[UDPPacket.BaseHeadLen..].ToArray());

                    packet.ContainsParts.Add(poffset);
                }
            }
            else if (LPacket.ReadISLP(data))
            {
                packet.Lenght = LPacket.ReadPacketLen(data);
            }
            else // data part
            {
                var poffset = DataPacket.ReadPOffset(data);
                lock (packet.ContainsParts)
                {
                    if (packet.ContainsParts.Contains(poffset))
                        return;

                    packet.ContainsParts.Add(poffset);

                    packet.Parts.Add(data[UDPPacket.BaseHeadLen..].ToArray());
                }
            }

            ProcessPacket(channel, packet);
        }

        protected virtual void ProcessPacket(UDPChannelEnum channel, PacketReciveTemp packet)
        {
            if (packetReceiveBuffer.TryRemove(packet.PID, out packet))
            {
                udpClient.Receive(packet.Parts
                .OrderBy(x => PacketReciveTemp.ReadPartDataOffset(x))
                .SelectMany(x => x[2..].ToArray())
                .ToArray(), channel);

                OnReceive(packet.PID);
            }
        }

        uint currentPID = uint.MaxValue;

        private uint CreatePID()
        {
            int incrementedSigned = Interlocked.Increment(ref Unsafe.As<uint, int>(ref currentPID));
            return Unsafe.As<int, uint>(ref incrementedSigned);
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

    public struct PacketWaitTemp
    {
        public uint PID;

        public Memory<byte> Head;

        public IEnumerable<Memory<byte>> Parts;
    }
}
