﻿
using NSL.BuilderExtensions.SocketCore;
using NSL.Extensions.RPC;
using NSL.Extensions.RPC.EndPointBuilder;
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
        builder.AddConnectHandle(client => { client.InitializeObjectBag(); });

        builder.RegisterRPCProcessor();
        builder.AddRPCContainer(_client => _client.RPCRepository);

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
            //Console.WriteLine($"[Client] Send packet({pid}) to {client.GetRemotePoint()} from\r\n{stackTrace}");
            Console.WriteLine($"[Server] Send packet({pid}) - {GetRPCPacketType(pid)} to {client.GetRemotePoint()}");
        });

        builder.AddReceiveHandle((client, pid, packet) =>
        {
            Console.WriteLine($"[Server] Receive packet({pid}) - {GetRPCPacketType(pid)} from {client.GetRemotePoint()}");
        });
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
        builder.AddConnectHandle(client => { client.InitializeObjectBag(); });

        builder.RegisterRPCProcessor();
        builder.AddRPCContainer(_client => _client.RPCRepository);

        builder.AddConnectHandle(client =>
        {
            //client.RPCRepository.abc1(1,);
        });

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
            Console.WriteLine($"[Client] Send packet({pid}) - {GetRPCPacketType(pid)} to {client.GetRemotePoint()}");
        });

        builder.AddReceiveHandle((client, pid, packet) =>
        {
            Console.WriteLine($"[Client] Receive packet({pid}) - {GetRPCPacketType(pid)} from {client.GetRemotePoint()}");
        });
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