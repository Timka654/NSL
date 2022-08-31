﻿using NSL.SocketClient;
using NSL.SocketCore.Utils.Buffer;
using NSL.TCP.Client;
using System;
using TCPExample.Client;

ClientOptions<NetworkClient> options = new ClientOptions<NetworkClient>();

options.ReceiveBufferSize = 1024;

options.AddHandle(1, (c, p) =>
{
    Console.WriteLine($"received from server {p.PacketId} - {p.ReadString16()}");
});

var t = new TCPNetworkClient<NetworkClient, ClientOptions<NetworkClient>>(options);

Console.WriteLine($"Current State {t.GetState()}, Try connect");

if (!t.Connect("127.0.0.1", 20004))
    Console.WriteLine($"Cannot connect, Current State {t.GetState()}");
else
{
    while (true)
    {
        Console.WriteLine($"Write any text:");

        var outputPacketBuffer = new OutputPacketBuffer();

        outputPacketBuffer.PacketId = 1;

        outputPacketBuffer.WriteString16(Console.ReadLine());

        t.Send(outputPacketBuffer);

    }
}

t.Disconnect();

Console.ReadKey();