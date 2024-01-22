using Newtonsoft.Json;
using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.TCPClient;
using NSL.BuilderExtensions.TCPServer;
using NSL.Extensions.Session.Client;
using NSL.Extensions.Session.Client.Packets;
using NSL.Extensions.Session.Server;
using NSL.Logger;
using NSL.SocketClient;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer;
using NSL.SocketServer.Utils;

namespace NSL.Extensions.Session.Example
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var conLog = new ConsoleLogger();

            var sl = new PrefixableLoggerProxy(conLog, "[Server]");
            var cl = new PrefixableLoggerProxy(conLog, "[Client]");

            NSLSessionManager<BaseServerNetworkClient> serverSessionManager;

            var server = TCPServerEndPointBuilder.Create()
                .WithClientProcessor<BaseServerNetworkClient>()
                .WithOptions()
                .WithBindingPoint(1555)
                .WithCode(b =>
                {
                    b.SetLogger(sl);

                    b.AddDefaultEventHandlers(handleOptions: DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);

                    b.AddConnectHandle(c => c.InitializeObjectBag());

                    serverSessionManager = b.GetOptions()
                        .AddNSLSessions(c =>
                        {
                            c.CloseSessionDelay = TimeSpan.FromSeconds(9);

                            c.OnExpiredSession = (client, session) =>
                            {
                                sl.AppendLog($"Expired session {session.Session}");

                                return Task.CompletedTask;
                            };

                            c.OnRecoverySession = (client, session) =>
                            {
                                sl.AppendLog($"Recovered session {session.Session}");
                                return Task.CompletedTask;
                            };
                        });

                    b.GetOptions().SetDefaultResponsePID();

                    b.AddPacketHandle(2, (c, d) =>
                    {
                        var session = serverSessionManager.CreateSession(c);

                        sl.AppendLog($"Client connected session {session.Session}");

                        var result = OutputPacketBuffer.Create(2);

                        session.WriteFullTo(result);

                        c.Send(result);
                    });
                })
                .Build();


            server.Run();

            NSLSessionInfo clientSession = default;

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

                    b.AddDefaultEventHandlers(handleOptions: DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);

                    b.AddPacketHandle(2, (c, d) =>
                    {
                        clientSession = NSLSessionInfo.ReadFullFrom(d);
                    });

                    b.GetOptions()
                    .SetDefaultResponsePID();

                    b.GetOptions()
                    .AddNSLSessions();
                })
                .Build();

            client.Connect();


            //sigin

            client.SendEmpty(2);

            await Task.Delay(2_000);

            client.Disconnect();

            // success recovery test
            await Task.Delay(2_000);

            client.Connect();

            client.SetNSLSessionInfo<BasicNetworkClient>(clientSession);

            var recovery = await NSLRecoverySessionPacket<BasicNetworkClient>.SendRequestAsync(client.Data);

            var validInfo = recovery.SessionInfo;

            cl.AppendInfo($"{JsonConvert.SerializeObject(recovery)}");

            await Task.Delay(9_500);

            client.Disconnect();

            await Task.Delay(1_000);

            Console.ForegroundColor = ConsoleColor.Blue;

            Console.WriteLine($"With invalid info");

            client.Connect();

            client.SetNSLSessionInfo<BasicNetworkClient>(clientSession); // invalid info

            recovery = await NSLRecoverySessionPacket<BasicNetworkClient>.SendRequestAsync(client.Data);

            cl.AppendInfo($"{JsonConvert.SerializeObject(recovery)}");


            await Task.Delay(1_000);

            client.Disconnect();

            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine($"With correct info");

            clientSession = validInfo;

            await Task.Delay(1_000);

            client.Connect();

            client.SetNSLSessionInfo<BasicNetworkClient>(clientSession);

            recovery = await NSLRecoverySessionPacket<BasicNetworkClient>.SendRequestAsync(client.Data);

            cl.AppendInfo($"{JsonConvert.SerializeObject(recovery)}");


            await Task.Delay(1_000);

            client.Disconnect();



            // success recovery end test

            Console.ForegroundColor = ConsoleColor.Red;

            // expired recovery test

            await Task.Delay(9_500);

            client.Connect();

            client.SetNSLSessionInfo<BasicNetworkClient>(clientSession);

            recovery = await NSLRecoverySessionPacket<BasicNetworkClient>.SendRequestAsync(client.Data);

            cl.AppendInfo($"{JsonConvert.SerializeObject(recovery)}");

            // expired recovery end test

            Console.ReadKey();
        }
    }
}