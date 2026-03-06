using NSL.Cipher.RC.RC4;
using NSL.Logger;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using NSL.TCP.Server;
using System.Diagnostics;
using TCP.BaseServerExample;

Console.WriteLine("TCP.Server");

ServerOptions<BaseServerNetworkClient> options = new ServerOptions<BaseServerNetworkClient>();

options.Port = 20008;

options.IpAddress = "0.0.0.0";

options.ReceiveBufferSize = 1024;

options.HelperLogger = new ConsoleLogger();

options.InputCipher = new XRC4Cipher("werty65343g353g");
options.OutputCipher = new XRC4Cipher("werty65343g353g");

options.OnExceptionEvent += (ex, c) =>
{
    Console.WriteLine($"Exception {ex}");
};

options.AddHandle(1, (client, p) =>
{
    Console.WriteLine($"Receive from client {p.ReadString()}");

    var o = OutputPacketBuffer.Create(4);

    o.WriteInt32(p.DataLength);

    client.Send(o);
});

options.AddHandle(7, (c, req) =>
{
    var d = req.ReadNullableClass<object>(() =>
    {
        var s1 = Enumerable.Range(0,1000).Select(x=> req.ReadString()).ToArray();

        return s1;
    });
    //res.WriteString("invoked");

    //return false;

});

options.AddHandle(3, (client, p) =>
{
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

var t = new TCPServerListener<BaseServerNetworkClient>(options, false);

t.Start();

Thread.Sleep(Timeout.Infinite);