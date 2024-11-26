using NSL.SocketClient;
using NSL.SocketCore.Utils.Buffer;
using NSL.TCP.Client;
using System.Diagnostics;
using TCPExample.Client;

Console.WriteLine("TCP.Client");

ClientOptions<NetworkClient> options = new ClientOptions<NetworkClient>();

options.ReceiveBufferSize = 1024;
options.OnClientConnectEvent += (client) => client.PingPongEnabled = true;

options.AddHandle(1, (c, p) =>
{
    Console.WriteLine($"received from server {p.PacketId} - {p.ReadString()}");
});

var t = new TCPNetworkClient<NetworkClient>(options);

t.OnReceivePacket += (c, pid, len) => {/* if (InputPacketBuffer.IsSystemPID(pid)) return; Console.WriteLine($"received {pid}");*/ };
t.OnSendPacket += (c, pid, len, stackTrace) => { /*Console.WriteLine($"sended {pid}");*/ };

Console.WriteLine($"Current State {t.GetState()}, Try connect");

if (!await t.ConnectAsync("127.0.0.1", 20004))
    Console.WriteLine($"Cannot connect, Current State {t.GetState()}");
else
{

    await Task.Delay(1000);
    using var opb = new OutputPacketBuffer();

    opb.PacketId = 1;

    byte[] buf = new byte[ushort.MaxValue];

    Random.Shared.NextBytes(buf);


    Stopwatch sw = new Stopwatch();
    for (int i = 0; i < 1_000_000; i++)
    {
        sw.Start();

        t.Send(opb, false);

        sw.Stop();
    }

    Console.WriteLine($"Send {sw.ElapsedMilliseconds}");



    while (true)
    {
        Console.WriteLine($"Write any text:");

        var line = Console.ReadLine();

        var outputPacketBuffer = new OutputPacketBuffer();

        outputPacketBuffer.PacketId = 1;

        outputPacketBuffer.WriteString(line);

        t.Send(outputPacketBuffer);

    }
}

t.Disconnect();

Console.ReadKey();