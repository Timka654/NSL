using Newtonsoft.Json;
using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.TCPClient;
using NSL.BuilderExtensions.TCPServer;
using NSL.Extensions.Version.Client;
using NSL.Extensions.Version.Client.Packets;
using NSL.Extensions.Version.Server;
using NSL.Logger;
using NSL.SocketClient;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer;
using NSL.SocketServer.Utils;

namespace NSL.Extensions.Version.Example
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var conLog = new ConsoleLogger();

            var sl = new PrefixableLoggerProxy(conLog, "[Server]");
            var cl = new PrefixableLoggerProxy(conLog, "[Client]");

            var server = TCPServerEndPointBuilder.Create()
                .WithClientProcessor<BaseServerNetworkClient>()
                .WithOptions()
                .WithBindingPoint(1555)
                .WithCode(b =>
                {
                    b.SetLogger(sl);

                    b.AddDefaultEventHandlers<TCPServerEndPointBuilder<BaseServerNetworkClient, ServerOptions<BaseServerNetworkClient>>, BaseServerNetworkClient>(handleOptions: DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);

                    b.AddConnectHandle(c => c.InitializeObjectBag());


                    b.GetOptions().SetDefaultResponsePID();

                    b.GetOptions().AddNSLVersion(c =>
                    {
                        c.Version = "155";
                        c.RequireVersion = "10";
                        c.MinVersion = "5";

                    });
                })
                .Build();


            server.Run();

            var client = TCPClientEndPointBuilder.Create()
                .WithClientProcessor<BasicNetworkClient>()
                .WithOptions()
                .WithEndPoint("127.0.0.1", 1555)
                .WithCode(b =>
                {
                    b.SetLogger(cl);

                    b.AddConnectHandle(c =>
                    {
                        c.InitializeObjectBag();
                    });

                    b.GetOptions().ConfigureRequestProcessor();

                    b.GetOptions()
                    .SetDefaultResponsePID();

                    b.GetOptions()
                    .AddNSLVersion(c => { c.Version = "10"; });

                    b.AddDefaultEventHandlers<TCPClientEndPointBuilder<BasicNetworkClient, ClientOptions<BasicNetworkClient>>, BasicNetworkClient>(handleOptions: DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);
                })
                .Build();

            client.Connect();

            var versionInfo = await NSLVersionPacket<BasicNetworkClient>.SendRequestAsync(client.Data);

            cl.AppendInfo($"{JsonConvert.SerializeObject(versionInfo)}");

            Console.ReadKey();
        }
    }
}