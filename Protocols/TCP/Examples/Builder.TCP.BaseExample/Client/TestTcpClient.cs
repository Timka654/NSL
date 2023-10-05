using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.TCPClient;
using NSL.SocketClient;
using NSL.SocketCore.Utils.Buffer;
using NSL.TCP.Client;

namespace Builder.TCP.BaseExample.Client
{
    internal class TestTcpClient
    {
        static CancellationTokenSource cts;

        static TCPNetworkClient<TCPNetworkClient, ClientOptions<TCPNetworkClient>> client;

        public static async Task RunClient()
        {
            client = TCPClientEndPointBuilder
                .Create()
                .WithClientProcessor<TCPNetworkClient>()
                .WithOptions()
                .WithEndPoint("127.0.0.1", 20006)
                .WithCode(builder =>
                {
                    // builder.WithAddressFamily(System.Net.Sockets.AddressFamily.InterNetwork); //optional(setted on initialize to valid)
                    // builder.WithProtocolType(System.Net.Sockets.ProtocolType.Tcp); //optional(setted on initialize to valid)
                    builder.WithBufferSize(8192); //optional
                })
                .WithCode(builder =>
                {
                    builder.AddConnectHandle(client =>
                    {
                        Console.WriteLine($"[Client] Success connected");
                    });

                    builder.AddDisconnectHandle(client =>
                    {
                        Console.WriteLine($"[Client] Client disconnected");
                    });

                    builder.AddExceptionHandle((ex, client) =>
                    {
                        Console.WriteLine($"[Client] Exception error handle - {ex}");
                    });

                    builder.AddSendHandle((client, pid, packet, stackTrace) =>
                    {
                        //Console.WriteLine($"[Client] Send packet({pid}) to {client.GetRemotePoint()} from\r\n{stackTrace}");
                        Console.WriteLine($"[Client] Send packet({pid}) to {client?.Network?.GetRemotePoint()}");
                    });

                    builder.AddReceiveHandle((client, pid, packet) =>
                    {
                        Console.WriteLine($"[Client] Receive packet({pid}) from {client.Network?.GetRemotePoint()}");
                    });
                })
                .WithCode(builder =>
                {
                    builder.AddPacket(1, new ClientTestPacket1());

                    builder.LoadPackets(typeof(PacketTestLoadAttribute));

                    builder.AddPacketHandle(3, (client, data) =>
                    {
                        Console.WriteLine($"[Client] receive from packet handle(3) - {data.ReadString()}");

                        cts.Cancel();
                    });
                })
                .Build();


            if (!await client.ConnectAsync())
                throw new Exception($"cannot connect to remote host!!");
        }

        public static async Task RunTest()
        {
            cts = new CancellationTokenSource();

            Console.WriteLine("write any text and press <Enter>:");

            var forpacket1 = new OutputPacketBuffer();

            forpacket1.PacketId = 1;

            forpacket1.WriteString(Console.ReadLine());

            client.Send(forpacket1);

            await Task.Run(cts.Token.WaitHandle.WaitOne);
        }

        public static void Disconnect()
        {

            client.Disconnect();
        }
    }
}
