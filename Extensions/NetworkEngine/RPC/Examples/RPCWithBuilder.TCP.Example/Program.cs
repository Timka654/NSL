
using NSL.BuilderExtensions.SocketCore;
using NSL.SocketServer;
using RPCWithBuilder.TCP.Example;

NSL.BuilderExtensions.TCPServer.TCPServerEndPointBuilder
    .Create()
    .WithClientProcessor<NetworkServerClient>()
    .WithOptions<ServerOptions<NetworkServerClient>>()
    .WithBindingPoint("0.0.0.0", 5506)
    .WithCode(builder => {
        builder.AddConnectHandle(client =>
        {
           new TestRPCClientContainerRPCRepository();
        });
    });
