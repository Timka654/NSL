using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.UDPServer;
using NSL.UDP.Client;

namespace Builder.UDP.BaseExample.Server
{
    internal class TestUDPServer
    {
        public static Task RunServer()
        {
            var server = UDPServerEndPointBuilder
                .Create()
                .WithClientProcessor<UDPServerNetworkClient>()
                .WithOptions<UDPClientOptions<UDPServerNetworkClient>>()
                .WithBindingPoint("0.0.0.0", 20006)
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
                        Console.WriteLine($"[Server] Success connected");
                    });

                    builder.AddDisconnectHandle(client =>
                    {
                        Console.WriteLine($"[Server] Client disconnected");
                    });

                    builder.AddExceptionHandle((ex, client) =>
                    {
                        Console.WriteLine($"[Server] Exception error handle - {ex}");
                    });

                    builder.AddSendHandle((client, pid, packet, stackTrace) =>
                    {
                        //Console.WriteLine($"[Server] Send packet({pid}) to {client.GetRemotePoint()} from\r\n{stackTrace}");
                        Console.WriteLine($"[Server] Send packet({pid}) to {client.GetRemotePoint()}");
                    });

                    builder.AddReceiveHandle((client, pid, packet) =>
                    {
                        Console.WriteLine($"[Server] Receive packet({pid}) from {client.GetRemotePoint()}");
                    });
                })
                .WithCode(builder =>
                {
                    builder.AddPacket(1, new ServerTestPacket1());

                    //builder.AddPacket(1, new testPacket1());

                    //builder.AddPacketHandle(2, (client, data) =>
                    //{
                    //    Console.WriteLine($"[Client] receive from packet handle(2) - {data.ReadString16()}");

                    //    cts.Cancel();
                    //});

                    //builder.LoadPackets(typeof(PacketTestLoadAttribute));
                })
                .Build();


            server.Start();

            return Task.CompletedTask;
        }
    }
}
