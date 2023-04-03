using NSL.SocketCore.Utils;
using System.Collections.Generic;
using NSL.UDP.Enums;
using System;
using NSL.UDP.Packet;
using System.Threading.Tasks;
using System.Threading;
using System.Buffers;
using NSL.SocketServer.Utils;
using NSL.UDP.Client;
using System.Collections.Concurrent;
using System.Linq;

namespace NSL.UDP.Channels
{
    public class ReliableChannel<TClient, TParent> : BaseChannel<TClient, TParent>
        where TClient : IServerNetworkClient
        where TParent : BaseUDPClient<TClient, TParent>
    {
        public override UDPChannelEnum Channel => UDPChannelEnum.Reliable;

        /// <summary>
        /// Min ping with latest multiple requests
        /// </summary>
        public int MINPing => pings.Min();

        /// <summary>
        /// Average ping with latest multiple requests
        /// </summary>
        public int AVGPing => (int)pings.Average();

        /// <summary>
        /// Max ping with latest multiple requests
        /// </summary>
        public int MAXPing => pings.Max();

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

            //Debug.Log($"ack received {pid}"); // ok

            ActReceived(pid);
        }

        private void ReceiveHandle(UDPChannelEnum channel, uint pid)
        {
            var packet = ArrayPool<byte>.Shared.Rent(6);

            packet[0] = (byte)DgramHeadTypeEnum.ACK;

            packet[1] = (byte)channel;

            BitConverter.GetBytes(pid).CopyTo(packet, 4);

            udpClient.SocketSend(packet);

            ArrayPool<byte>.Shared.Return(packet);
        }

        private async void SendHandle(BaseChannel<TClient, TParent> dummy, PacketWaitTemp temp)
        {
            DateTime reqTime = default;
            using var locker = new CancellationTokenSource();

            using CancellationTokenSource linkedCts =
                    CancellationTokenSource.CreateLinkedTokenSource(locker.Token, udpClient.LiveStateToken);

            int delay = UDPOptions.ReliableSendRepeatDelay;

            Action<uint> action = (pid) =>
            {
                if (temp.PID == pid)
                {
                    var edt = DateTime.UtcNow;

                    locker.Cancel();

                    PingProcess(reqTime, edt);
                }
            };

            ActReceived += action;

            try
            {
                reqTime = DateTime.UtcNow;

                do
                {
                    await Task.Delay(delay, linkedCts.Token);

                    base.SendFull(temp);
                }
                while (!locker.IsCancellationRequested);
            }
            catch (TaskCanceledException) { }
            finally
            {

                //Debug.Log($"ack success cancel {temp.PID}"); // ok
                ActReceived -= action;
                //Debug.Log($"ack success cancel2 {temp.PID}"); // ok
            }
        }

        private async void PingProcess(DateTime s, DateTime e)
        {
            await Task.Run(() =>
            {
                pings.Enqueue((int)(e - s).TotalMilliseconds);

                if (pings.Count > 3)
                {
                    while (pings.Count > 3)
                        pings.TryDequeue(out _);
                }
            });
        }

        private ConcurrentQueue<int> pings = new ConcurrentQueue<int>();
    }
}
