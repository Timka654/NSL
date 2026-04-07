using NSL.SocketCore.Utils;
using NSL.SocketServer.Utils;
using NSL.UDP.Packet;
using System;
using System.Collections.Concurrent;
using System.Linq;
//using UnityEngine;

namespace NSL.UDP.Channels
{
    internal class OrderedChannel<TClient, TParent> : BaseChannel<TClient, TParent>
        where TClient : BaseNetworkConnection, new()
        where TParent : BaseUDPClient<TClient, TParent>
    {
        private readonly BaseChannel<TClient, TParent> parent;

        public override UDPChannelEnum Channel => UDPChannelEnum.Ordered;

        public OrderedChannel(BaseUDPClient<TClient, TParent> udpClient) : base(udpClient) { }
        public OrderedChannel(BaseUDPClient<TClient, TParent> udpClient, BaseChannel<TClient, TParent> parent) : this(udpClient)
        {
            receivedPidSet.TryAdd(uint.MaxValue, true);
            receivedPidOrder.Enqueue(uint.MaxValue);

            this.parent = parent;
        }

        public override void Receive(UDPChannelEnum channel, Span<byte> data)
        {
            var pid = UDPPacket.ReadPID(data);

            if (receivedPidSet.ContainsKey(pid))
                return;

            base.Receive(channel, data);
        }

        protected override void ProcessPacket(UDPChannelEnum channel, PacketReciveTemp packet)
        {
            if (!packet.Ready())
                return;

            lock (this)
            {
                //if (packet.PID == uint.MaxValue || packet.PID == uint.MinValue)
                //{
                //    Debug.Log($"received {packet.PID} - prev {packet.PID - 1} - next {packet.PID + 1}");
                //} //ok

                if (!receivedPidSet.ContainsKey(packet.PID - 1))
                {
                    //Debug.Log($"recovery order dropped {packet.PID - 1}"); // ok
                    Action<uint> rcvHandle = default;

                    rcvHandle = (pid) =>
                    {
                        if (pid == packet.PID - 1)
                        {
                            //Debug.Log($"recovery order - received {pid}, process {packet.PID}"); // ok
                            ProcessPacket(channel, packet);

                            OnReceive -= rcvHandle;
                        }
                    };

                    OnReceive += rcvHandle;

                    return;
                }
            }

            if (receivedPidSet.ContainsKey(packet.PID))
            {
                packetReceiveBuffer.TryRemove(packet.PID, out _);
                return;
            }

            receivedPidSet.TryAdd(packet.PID, true);
            receivedPidOrder.Enqueue(packet.PID);

            if (receivedPidOrder.Count > 1000) // 5k max, next - freeze
                if (receivedPidOrder.TryDequeue(out var evicted))
                    receivedPidSet.TryRemove(evicted, out _);

            base.ProcessPacket(channel, packet);
        }


        private readonly ConcurrentDictionary<uint, bool> receivedPidSet = new ConcurrentDictionary<uint, bool>();
        private readonly ConcurrentQueue<uint> receivedPidOrder = new ConcurrentQueue<uint>();
    }
}