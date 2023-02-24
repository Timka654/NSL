﻿using NSL.SocketCore.Utils;
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

		public virtual void Receive(UDPChannelEnum channel, SocketAsyncEventArgs result)
		{
			var data = result.MemoryBuffer[..result.BytesTransferred];

			var checksum = UDPPacket.ReadChecksum(data);

			if (!checksum.Equals(UDPPacket.GetChecksum(data)))
			{
				InvalidRecvChecksum(this, result);
				return;
			}

			var pid = UDPPacket.ReadPID(data);

			var packet = packetReceiveBuffer.GetOrAdd(
				pid,
				id => new PacketReciveTemp(id));

			if (LPacket.ReadISLP(data))
			{
				packet.Lenght = LPacket.ReadPacketLen(data);

				return;
			}

			if (CompPacket.ReadISComp(data))
			{

				return;
			}

			lock (packet.ContainsParts)
			{
				if (packet.ContainsParts.Contains(pid))
					return;

				packet.ContainsParts.Add(pid);
			}

			packet.Parts.Add(data[6..result.BytesTransferred]);

			if (packet.Ready() &&
				packetReceiveBuffer.TryRemove(packet.PID, out packet))
			{
				udpClient.Receive(packet.Parts
				.OrderBy(x => PacketReciveTemp.ReadPartDataOffset(x))
				.SelectMany(x => x[2..].ToArray())
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



		uint currentPID = 0;

		public uint CreatePID()
		{
			lock (this)
			{
				return currentPID++;
			}
		}
	}
}
