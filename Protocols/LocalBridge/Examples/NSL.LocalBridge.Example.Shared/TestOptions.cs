using NSL.LocalBridge;
using NSL.SocketClient;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer;

namespace LocalBridge.Example.Shared
{
    public class TestOptions
    {
        public static void ConfigureListener(ServerOptions<TCPExampleServer> options)
        {

            options.AddHandle(1, lhandle1);
            options.AddHandle(2, lhandle2);
            options.AddHandle(100, lhandleReceive);

            options.OnClientConnectEvent += (client) => { Console.WriteLine($"listener connected client"); };
            options.OnClientDisconnectEvent += (client) => { Console.WriteLine($"listener disconnected client"); };

        }
        public static void ConfigureClient(ClientOptions<TCPExampleClient> options)
        {

            options.AddHandle(3, chandle3);
            options.AddHandle(4, chandle4);
            options.AddHandle(100, chandleReceive);

            options.OnClientConnectEvent += (client) => { Console.WriteLine($"client success connect"); };
            options.OnClientDisconnectEvent += (client) => { Console.WriteLine($"client lost connection"); };

        }

        public static void ProcessTest(LocalBridgeClient<TCPExampleClient, TCPExampleServer> clientToServer, LocalBridgeClient<TCPExampleServer, TCPExampleClient> serverToClient)
        {

            // invoke listener handle 1
            var packet = new OutputPacketBuffer();

            packet.PacketId = 1;

            packet.WriteString($"From client to server pid:{packet.PacketId}");

            clientToServer.Send(packet);

            // invoke listener handle 2

            packet = new OutputPacketBuffer();

            packet.PacketId = 2;

            packet.WriteString($"From client to server pid:{packet.PacketId}");

            clientToServer.Send(packet);

            // invoke listener with answer

            packet = new OutputPacketBuffer();

            packet.PacketId = 100;

            packet.WriteString($"client content");

            clientToServer.Send(packet);


            packet = new OutputPacketBuffer();

            packet.PacketId = 3;

            packet.WriteString($"From server to client pid:{packet.PacketId}");

            serverToClient.Send(packet);


            packet = new OutputPacketBuffer();

            packet.PacketId = 4;

            packet.WriteString($"From server to client pid:{packet.PacketId}");

            serverToClient.Send(packet);

            clientToServer.Disconnect();
        }

        static void lhandle1(TCPExampleServer client, InputPacketBuffer data)
        {
            Console.WriteLine($"{nameof(lhandle1)} received - {data.ReadString()}");
        }

        static void lhandle2(TCPExampleServer client, InputPacketBuffer data)
        {
            Console.WriteLine($"{nameof(lhandle2)} received - {data.ReadString()}");
        }

        static void lhandleReceive(TCPExampleServer client, InputPacketBuffer data)
        {
            var p = new OutputPacketBuffer();

            p.PacketId = 100;

            p.WriteString(data.ReadString());
            p.WriteString("server content");

            client.Network?.Send(p);
        }


        static void chandle3(TCPExampleClient client, InputPacketBuffer data)
        {
            Console.WriteLine($"{nameof(chandle3)} received - {data.ReadString()}");
        }

        static void chandle4(TCPExampleClient client, InputPacketBuffer data)
        {
            Console.WriteLine($"{nameof(chandle4)} received - {data.ReadString()}");
        }

        static void chandleReceive(TCPExampleClient client, InputPacketBuffer data)
        {
            Console.WriteLine($"{nameof(chandleReceive)} - cmsg: {data.ReadString()} - lmsg: {data.ReadString()}");
        }
    }
}
