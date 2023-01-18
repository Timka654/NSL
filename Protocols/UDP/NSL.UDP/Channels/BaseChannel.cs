using NSL.SocketCore.Utils;
using NSL.UDP.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using NSL.UDP.Interface;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;

namespace NSL.UDP.Channels
{
    public abstract class BaseChannel<TClient, TParent>
        where TClient : INetworkClient
        where TParent : BaseUDPClient<TClient, TParent>
    {
        protected readonly BaseUDPClient<TClient, TParent> udpClient;

        protected readonly IUDPOptions UDPOptions;

        public abstract UDPChannelEnum Channel { get; }

        public BaseChannel(BaseUDPClient<TClient, TParent> udpClient)
        {
            this.udpClient = udpClient;

            UDPOptions = udpClient.Options as IUDPOptions;
        }

        protected virtual void AfterBuild(BaseChannel<TClient, TParent> fromChannel, PacketWaitTemp packet) { }

        protected virtual void InvalidRecvChecksum(BaseChannel<TClient, TParent> fromChannel, SocketAsyncEventArgs packet) { }

        public virtual void Send(UDPChannelEnum channel, byte[] data)
        {
            var count = (int)Math.Ceiling((double)data.Length / UDPOptions.SendFragmentSize);

            var ppid = udpClient.CreatePID();

            var pidBytes = BitConverter.GetBytes(ppid);

            var channelBytes = new byte[] { (byte)channel };

            var packet = new PacketWaitTemp()
            {
                PID = ppid,
                Head = CreateHeader(pidBytes, channelBytes, (ushort)count),
                Parts = CreateParts(pidBytes, channelBytes, data)
            };

            AfterBuild(this, packet);

            SendFull(packet);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Memory<byte> CreateHeader(byte[] pidBytes, byte[] channelBytes, ushort count)
        {
            var lpBuf = (Memory<byte>)new byte[10];

            lpBytes
                .CopyTo(lpBuf);

            pidBytes
                .CopyTo(lpBuf[1..]);

            channelBytes
                .CopyTo(lpBuf[5..]);

            BitConverter.GetBytes(count)
                .CopyTo(lpBuf[6..]);

            var lpBufRes = lpBuf.ToArray();

            var cs = GetChecksum(lpBufRes);

            BitConverter.GetBytes(cs)
                .CopyTo(lpBuf[8..]);

            return lpBuf;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<Memory<byte>> CreateParts(byte[] pidBytes, byte[] channelBytes, byte[] data)
        {
            List<Memory<byte>> result = new List<Memory<byte>>();

            ushort h = default;

            for (int i = 0; i < data.Length; i += UDPOptions.SendFragmentSize)
            {
                var dest = i + UDPOptions.SendFragmentSize > data.Length ? data[i..] : data[i..(i + UDPOptions.SendFragmentSize)];

                Memory<byte> pbuf = new byte[10 + dest.Length];

                dataBytes
                    .CopyTo(pbuf);

                pidBytes
                    .CopyTo(pbuf[1..]);

                channelBytes
                    .CopyTo(pbuf[5..]);

                BitConverter.GetBytes(h++)
                    .CopyTo(pbuf[6..]);

                dest
                    .CopyTo(pbuf[10..]);

                var pbufResult = pbuf.ToArray();

                BitConverter.GetBytes(GetChecksum(pbufResult))
                    .CopyTo(pbuf[8..]);

                result.Add(pbuf);
            }

            return result;
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

        protected static byte[] dataBytes = new byte[] { 1 };
        protected static byte[] lpBytes = new byte[] { 0 };
        protected static byte[] emptySumBytes = new byte[] { 0, 0 };


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadPID(Memory<byte> buffer) => BitConverter.ToUInt16(buffer.Span[1..]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UDPChannelEnum ReadChannel(Memory<byte> buffer) => (UDPChannelEnum)buffer.Span[5];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadChecksum(Memory<byte> buffer) => BitConverter.ToUInt16(buffer.Span[8..]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort GetChecksum(byte[] buffer)
            => GetChecksum(buffer.AsMemory());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort GetChecksum(Memory<byte> buffer)
        {
            emptySumBytes.CopyTo(buffer[8..]);
            //using var hasher = new SHA256()
            //    #if NET
            return (ushort)(SHA256.HashData(buffer.ToArray()).Sum(x => x) % ushort.MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadLP(Memory<byte> buffer) => (DgramHeadTypeEnum)buffer.Span[0] == DgramHeadTypeEnum.LP;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadLPLen(Memory<byte> buffer) => BitConverter.ToUInt16(buffer.Span[6..]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadPDataOffset(Memory<byte> buffer) => BitConverter.ToUInt16(buffer.Span);


        protected ConcurrentDictionary<uint, PacketReciveTemp> packetReceiveBuffer = new ConcurrentDictionary<uint, PacketReciveTemp>();

        public virtual void Receive(UDPChannelEnum channel, SocketAsyncEventArgs result)
        {
            var data = result.MemoryBuffer[..result.BytesTransferred];

            var checksum = ReadChecksum(data);

            if (!checksum.Equals(GetChecksum(data)))
            {
                InvalidRecvChecksum(this, result);
                return;
            }

            var pid = ReadPID(data);

            var isLP = ReadLP(data);


            var packet = packetReceiveBuffer.GetOrAdd(
                pid,
                id => new PacketReciveTemp(id));

            if (isLP)
                packet.Lenght = ReadLPLen(data);
            else
                packet.Parts.Add(data[6..result.BytesTransferred]);

            if (packet.Ready() &&
                packetReceiveBuffer.TryRemove(packet.PID, out packet))
            {
                udpClient.Receive(packet.Parts
                .OrderBy(x => ReadPDataOffset(x))
                .SelectMany(x => x[10..].ToArray())
                .ToArray());
            }
        }

        protected struct PacketWaitTemp
        {
            public uint PID;

            public Memory<byte> Head;

            public IEnumerable<Memory<byte>> Parts;
        }


        protected class PacketReciveTemp
        {
            public uint PID;

            public ushort Lenght;

            public ConcurrentBag<Memory<byte>> Parts;

            public PacketReciveTemp(uint PID, ushort len) : this(PID)
            {
                this.Lenght = len;
            }

            public PacketReciveTemp(uint PID)
            {
                this.PID = PID;
                this.Lenght = 0;
                Parts = new ConcurrentBag<Memory<byte>>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Ready() => Lenght > 0 && Parts.Count == Lenght;
        }
    }
}
