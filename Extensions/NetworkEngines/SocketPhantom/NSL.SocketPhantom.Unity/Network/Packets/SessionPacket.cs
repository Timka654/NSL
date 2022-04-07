using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketPhantom.Unity.Network.Packets
{
    internal class SessionPacket : IPacket<PhantomSocketNetworkClient>
    {
        private PhantomHubConnection phantomHubConnection;

        public SessionPacket(PhantomHubConnection phantomHubConnection)
        {
            this.phantomHubConnection = phantomHubConnection;
        }

        public override void Receive(PhantomSocketNetworkClient client, InputPacketBuffer data)
        {
            ProcessState(client, data.ReadByte());
        }

        private async void ProcessState(PhantomSocketNetworkClient client, byte state)
        {
            switch (state)
            {
                case byte.MaxValue:
                    client.connection.SetState(HubConnectionState.Connected);
                    break;
                case 0:
                    client.connection.ForceStop(new Exception($"Current hub path not found"));
                    break;
                case 1:
                    client.connection.ForceStop(new Exception($"Cannot sign by current data {{{phantomHubConnection.Path}, {phantomHubConnection.Session}, {await phantomHubConnection.GetAccessToken()}}}"));
                    break;
                default:
                    break;
            }
        }

        public static void Send(PhantomSocketNetworkClient client, string path, string session)
        {
            var packet = new OutputPacketBuffer();
            packet.PacketId = 1;
            packet.WriteString16(path);
            packet.WriteString16(session);

            client.Send(packet);
        }
    }
}
