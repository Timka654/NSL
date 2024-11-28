using NSL.SocketClient;
using NSL.SocketCore.Utils.Buffer;
using NSL.TCP.Client;
using System.Diagnostics;
using TCPExample.Client;

Console.WriteLine("TCP.Client");

ClientOptions<NetworkClient> options = new ClientOptions<NetworkClient>();

options.ReceiveBufferSize = 20480;
//options.OnClientConnectEvent += (client) => client.PingPongEnabled = true;

options.OnExceptionEvent += (ex, c) =>
{

};

options.AddHandle(4, (c, p) =>
{
    Console.WriteLine($"received from server {p.PacketId} - packet received on server with len {p.ReadInt32()}");
});

options.SegmentSize = 20480;

var t = new TCPNetworkClient<NetworkClient>(options, true);

//t.OnReceivePacket += (c, pid, len) => { if (InputPacketBuffer.IsSystemPID(pid)) return; Console.WriteLine($"received {pid}"); };
//t.OnSendPacket += (c, pid, len, stackTrace) => { Console.WriteLine($"sended {pid}"); };

Console.WriteLine($"Current State {t.GetState()}, Try connect");
    await Task.Delay(3000);

if (!await t.ConnectAsync("127.0.0.1", 20004, 20_000))
    Console.WriteLine($"Cannot connect, Current State {t.GetState()}");
else
{

    //using var opb = OutputPacketBuffer.Create(3);

    //byte[] buf = new byte[ushort.MaxValue];

    //Random.Shared.NextBytes(buf);

    ////opb.WriteByteArray(buf);


    //Stopwatch sw = new Stopwatch();
    //for (int i = 0; i < 1_000_000; i++)
    //{
    //    sw.Start();

    //    t.Send(opb, false);

    //    sw.Stop();
    //}

    //Console.WriteLine($"Send {sw.ElapsedMilliseconds}");


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