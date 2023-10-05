using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using NSL.TCP.Server;
using TCP.BaseServerExample;

Console.WriteLine("TCP.Server");

ServerOptions<BaseServerNetworkClient> options = new ServerOptions<BaseServerNetworkClient>();

options.Port = 20004;

options.IpAddress = "0.0.0.0";

options.ReceiveBufferSize = 1024;

options.AddHandle(1, (client, p) =>
{
    Console.WriteLine($"received from client({client.ObjectBag["uid"]}) pid:{p.PacketId} - {p.ReadString()}");
});

int counter = 0;

options.OnClientConnectEvent += (client) =>
{
    client.InitializeObjectBag();

    client.ObjectBag.Set("uid", Interlocked.Increment(ref counter));

    var outputPacketBuffer = new OutputPacketBuffer();

    outputPacketBuffer.PacketId = 1;

    outputPacketBuffer.WriteString("Hello! I'm server");

    client.Send(outputPacketBuffer);
};

options.OnClientDisconnectEvent += (client) =>
{
    Console.WriteLine($"Client({client.ObjectBag["uid"]}) disconnected!!");
};

var t = new TCPServerListener<BaseServerNetworkClient>(options);

t.Start();

Thread.Sleep(Timeout.Infinite);