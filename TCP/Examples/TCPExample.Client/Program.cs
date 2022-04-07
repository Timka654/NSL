using NSL.TCP.Client;
using SocketClient;
using SocketServer;
using System;
using TCPExample.Client;

ClientOptions<NetworkClient> options = new ClientOptions<NetworkClient>();

options.ReceiveBufferSize = 1024;

options.AddHandle(1, (c, p) =>
{
    Console.WriteLine($"received from server {p.PacketId}");
});

var t = new TCPNetworkClient<NetworkClient, ClientOptions<NetworkClient>>(options);

Console.WriteLine($"Current State {t.GetState()}, Try connect");

if (!t.Connect("127.0.0.1", 7554))
    Console.WriteLine($"Cannot connect, Current State {t.GetState()}");
else
{ 

}