using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.TCPClient;
using NSL.BuilderExtensions.TCPServer;
using NSL.Extensions.RPC;
using NSL.Extensions.RPC.EndPointBuilder;
using NSL.Logger;
using NSL.SocketClient;
using NSL.SocketServer;
using RPCWithBuilder.TCP.Example;
using RPCWithBuilder.TCP.Example.Models;

internal class Program
{
    private static void Main(string[] args)
    {
        //Server init

        var server = TCPServerEndPointBuilder
            .Create()
            .WithClientProcessor<NetworkServerClient>()
            .WithOptions<ServerOptions<NetworkServerClient>>()
            .WithBindingPoint("0.0.0.0", 5506)
            .WithCode(builder =>
            {
                builder.SetLogger(ConsoleLogger.Create());

                builder.AddConnectHandle(client => { client.InitializeObjectBag(); });

                builder.RegisterRPCProcessor();
                builder.AddRPCContainer(_client => _client.RPCRepository);

                builder.AddDefaultEventHandlers(
                    "[Server]",
                     DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace,
                     GetRPCPacketType,
                     GetRPCPacketType);
            })
            .Build();

        // Start server

        server.Start();

        // Client init

        var client = TCPClientEndPointBuilder
            .Create()
            .WithClientProcessor<NetworkClient>()
            .WithOptions<ClientOptions<NetworkClient>>()
            .WithEndPoint("127.0.0.1", 5506)
            .WithCode(builder =>
            {
                builder.SetLogger(ConsoleLogger.Create());

                builder.AddConnectHandle(client => { client.InitializeObjectBag(); });

                builder.RegisterRPCProcessor();
                builder.AddRPCContainer(_client => _client.RPCRepository);

                builder.AddDefaultEventHandlers(
                    "[Client]",
                     DefaultEventHandlersEnum.All & ~(DefaultEventHandlersEnum.DisplayEndPoint | DefaultEventHandlersEnum.HasSendStackTrace),
                    GetRPCPacketType,
                    GetRPCPacketType);
            })
            .Build();

        // Client try connect to server

        if (!client.Connect())
            throw new Exception();

        //Try call RPC from Client to Server

        Console.WriteLine($"[Client] Try call {nameof(TestRPCClientContainerRPCRepository<NetworkClient>.abc1)}");

        var repo = client.Data.RPCRepository;

        var rpcCallResult = repo.abc1(1, null, "abc", null, new TestDataModel() { tdValue = 10 }, new TestStructModel() { tsValue = 1010 });
        Console.WriteLine($"[Client] has result {rpcCallResult}");

        Console.WriteLine($"[Client] Try call {nameof(TestRPCClientContainerRPCRepository<NetworkClient>.testTupleReceive)} with 5");

        var tupleResult1 = repo.testTupleReceive(5);

        Console.WriteLine($"[Client] has result {tupleResult1}");

        Console.WriteLine($"[Client] Try call {nameof(TestRPCClientContainerRPCRepository<NetworkClient>.testTupleReceive)} with 55");

        var tupleResult2 = repo.testTupleReceive(55);

        Console.WriteLine($"[Client] has result {tupleResult2}");

        Console.ReadKey();
    }

    static string GetRPCPacketType(ushort pid)
    {
        switch (pid)
        {
            case RPCChannelProcessor.DefaultCallPacketId:
                return "Call";
            case RPCChannelProcessor.DefaultResultPacketId:
                return "Result";
            default:
                return "Other";
        }
    }
}