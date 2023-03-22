using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.UDPClient;
using NSL.UDP;
using NSL.UDP.Client;

namespace Builder.UDP.BaseExample.Client
{
    internal class TestUDPClient
    {
        static CancellationTokenSource cts;

        static UDPNetworkClient<UDPTestNetworkClient> client;

        static UDPClient<UDPTestNetworkClient> GetNetworkClient() => client.GetClient();

        public static async Task RunClient()
        {
            client = UDPClientEndPointBuilder
                .Create()
                .WithClientProcessor<UDPTestNetworkClient>()
                .WithOptions<UDPClientOptions<UDPTestNetworkClient>>()
                .UseBindingPoint("0.0.0.0",20005)
                .UseEndPoint("127.0.0.1", 20006)
                .WithCode(builder =>
                {
                    // builder.WithAddressFamily(System.Net.Sockets.AddressFamily.InterNetwork); //optional(setted on initialize to valid)
                    // builder.WithProtocolType(System.Net.Sockets.ProtocolType.UDP); //optional(setted on initialize to valid)
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
                        Console.WriteLine($"[Client] Send packet({pid}) to {client.GetRemotePoint()}");
                    });

                    builder.AddReceiveHandle((client, pid, packet) =>
                    {
                        Console.WriteLine($"[Client] Receive packet({pid}) from {client.GetRemotePoint()}");
                    });
                })
                .WithCode(builder =>
                {
                    builder.AddPacket(1, new ClientTestPacket1());

                    builder.LoadPackets(typeof(PacketTestLoadAttribute));

                    builder.AddPacketHandle(3, (client, data) =>
                    {
                        Console.WriteLine($"[Client] receive from packet handle(3) - {data.ReadString16()}");

                        cts.Cancel();
                    });
                })
                .Build();


            client.Connect();
        }

        public static async Task RunTest()
        {
            cts = new CancellationTokenSource();

            Console.WriteLine("write any text and press <Enter>:");

            var forpacket1 = new NSL.UDP.DgramOutputPacketBuffer();

            forpacket1.PacketId = 1;

            forpacket1.WriteString16(Console.ReadLine());

            client.GetClient().Send(forpacket1);

            await Task.Run(cts.Token.WaitHandle.WaitOne);
        }

        public static void Disconnect()
        {

            client.Connect();
        }
    }
}
