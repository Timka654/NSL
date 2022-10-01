
using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.TCPClient;
using NSL.BuilderExtensions.TCPServer;
using NSL.Extensions.RPC;
using NSL.Extensions.RPC.EndPointBuilder;
using NSL.Logger;
using NSL.SocketClient;
using NSL.SocketCore.Utils;
using NSL.SocketServer;
using RPCWithBuilder.TCP.Example;
using RPCWithBuilder.TCP.Example.Models;
using System.Security.AccessControl;

//Server init

var server = NSL.BuilderExtensions.TCPServer.TCPServerEndPointBuilder
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

        builder.AddDefaultEventHandlers<TCPServerEndPointBuilder<NetworkServerClient, ServerOptions<NetworkServerClient>>, NetworkServerClient>(
            "[Server]",
             DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace,
             GetRPCPacketType,
             GetRPCPacketType);
    })
    .Build();

// Start server

server.Start();

// Client init

var client = NSL.BuilderExtensions.TCPClient.TCPClientEndPointBuilder
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

        builder.AddDefaultEventHandlers<TCPClientEndPointBuilder<NetworkClient, ClientOptions<NetworkClient>>, NetworkClient>(
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

var rpcCallResult = client.ConnectionOptions.ClientData.RPCRepository.abc1(1, null, "abc", null, new TestDataModel() { tdValue = 10 }, new TestStructModel() { tsValue = 1010 });

Console.WriteLine($"[Client] has result {rpcCallResult}");



Console.ReadKey();



string GetRPCPacketType(ushort pid)
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