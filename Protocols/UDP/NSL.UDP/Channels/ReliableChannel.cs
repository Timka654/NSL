using NSL.SocketCore.Utils;
using System.Collections.Generic;
using NSL.UDP.Enums;
using System.Net.Sockets;
using System;
using NSL.UDP.Packet;
using System.Threading.Tasks;
using System.Threading;
using System.Buffers;

namespace NSL.UDP.Channels
{
    public class ReliableChannel<TClient, TParent> : BaseChannel<TClient, TParent>
        where TClient : INetworkClient
        where TParent : BaseUDPClient<TClient, TParent>
    {
        public override UDPChannelEnum Channel => UDPChannelEnum.Reliable;

        BaseChannel<TClient, TParent> orderedChannel;
        BaseChannel<TClient, TParent> unorderedChannel;

        public ReliableChannel(BaseUDPClient<TClient, TParent> udpClient) : base(udpClient)
        {
            orderedChannel = new OrderedChannel<TClient, TParent>(udpClient, this)
            {
                OnSend = SendHandle,
                OnReceive = pid => ReceiveHandle(UDPChannelEnum.ReliableOrdered, pid)
            };

            unorderedChannel = new UnorderedChannel<TClient, TParent>(udpClient, this)
            {
                OnSend = SendHandle,
                OnReceive = pid => ReceiveHandle(UDPChannelEnum.ReliableUnordered, pid)
            };
        }

        public override void Send(UDPChannelEnum channel, byte[] data)
        {
            if (channel.HasFlag(UDPChannelEnum.Ordered))
                orderedChannel.Send(channel, data);
            else if (channel.HasFlag(UDPChannelEnum.Unordered))
                unorderedChannel.Send(channel, data);
            else
                throw new KeyNotFoundException(channel.ToString());
        }

        public override void Receive(UDPChannelEnum channel, Span<byte> data)
        {
            if (ACKPacket.ReadISACK(data))
                ProcessACK(data);
            else if (channel.HasFlag(UDPChannelEnum.Ordered))
                orderedChannel.Receive(channel, data);
            else if (channel.HasFlag(UDPChannelEnum.Unordered))
                unorderedChannel.Receive(channel, data);

            else
                throw new KeyNotFoundException(channel.ToString());
        }

        private event Action<uint> ActReceived = pid => { };

        private void ProcessACK(Span<byte> arr)
        {
            var pid = ACKPacket.ReadPID(arr);

            ActReceived(pid);
        }

        private void ReceiveHandle(UDPChannelEnum channel, uint pid)
        {
            var packet = ArrayPool<byte>.Shared.Rent(6);

            packet[0] = (byte)DgramHeadTypeEnum.ACK;

            BitConverter.GetBytes(pid).CopyTo(packet, 1);

            packet[5] = (byte)channel;

            udpClient.SocketSend(packet);

            ArrayPool<byte>.Shared.Return(packet);
        }

        private async void SendHandle(PacketWaitTemp temp)
        {
            var locker = new CancellationTokenSource();

            Action<uint> action = (pid) => { if (temp.PID == pid) locker.Cancel(); };

            ActReceived += action;

            try
            {
                do
                {
                    await Task.Delay(30, locker.Token);

                    base.SendFull(temp);
                }
                while (!locker.IsCancellationRequested);
            }
            catch (TaskCanceledException) { }
            finally
            {
                ActReceived -= action;
                locker.Dispose();
            }
        }




        uint currentPID = 0;

        internal override uint CreatePID()
        {
            lock (this)
            {
                return currentPID++;
            }
        }
    }
}
