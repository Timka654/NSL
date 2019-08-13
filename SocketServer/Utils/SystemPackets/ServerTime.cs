using System;
using System.Collections.Generic;
using System.Text;
using SocketServer.Utils.Buffer;

namespace SocketServer.Utils.SystemPackets
{
    public class SystemTime<T> : IPacket<T> where T : INetworkClient
    {
        public void Receive(T client, InputPacketBuffer data)
        {
            var now = DateTime.Now;

            var dt = data.ReadDateTime().Value;
            try
            {
                client.ServerDateTimeOffset = now - dt;

            }
            catch (Exception ex)
            {
                //ThreadHelper.InvokeOnMain(() => { UnityEngine.Debug.Log($"{now} - {dt} =  {now - dt}"); });
            }
        }

        public static void Send(INetworkClient client)
        {
            var packet = new OutputPacketBuffer()
            {
                PacketId = (ushort)Enums.ClientPacketEnum.ServerTime
            };

            packet.WriteDateTime(DateTime.Now);

            client.Network.Send(packet);
        }
}
}
