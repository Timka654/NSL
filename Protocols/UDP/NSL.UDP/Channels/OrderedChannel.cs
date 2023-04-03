using NSL.SocketServer.Utils;
using NSL.UDP.Enums;
using NSL.UDP.Packet;
using System;
using System.Collections.Concurrent;
using System.Linq;
//using UnityEngine;

namespace NSL.UDP.Channels
{
    internal class OrderedChannel<TClient, TParent> : BaseChannel<TClient, TParent>
        where TClient : IServerNetworkClient
        where TParent : BaseUDPClient<TClient, TParent>
    {
        private readonly BaseChannel<TClient, TParent> parent;

        public override UDPChannelEnum Channel => UDPChannelEnum.Ordered;

        public OrderedChannel(BaseUDPClient<TClient, TParent> udpClient) : base(udpClient) { }
        public OrderedChannel(BaseUDPClient<TClient, TParent> udpClient, BaseChannel<TClient, TParent> parent) : this(udpClient)
        {
            receivePidBuffer.Enqueue(uint.MaxValue);

            this.parent = parent;
        }

        public override void Receive(UDPChannelEnum channel, Span<byte> data)
        {
            var pid = UDPPacket.ReadPID(data);

            if (receivePidBuffer.Contains(pid))
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

                if (!receivePidBuffer.Contains(packet.PID - 1))
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

            if (receivePidBuffer.Contains(packet.PID))
            {
                packetReceiveBuffer.TryRemove(packet.PID, out _);
                return;
            }

            receivePidBuffer.Enqueue(packet.PID);

            if (receivePidBuffer.Count > 1000) // 5k max, next - freeze 
                receivePidBuffer.TryDequeue(out _);

            base.ProcessPacket(channel, packet);
        }


        private ConcurrentQueue<uint> receivePidBuffer = new ConcurrentQueue<uint>();
    }
}