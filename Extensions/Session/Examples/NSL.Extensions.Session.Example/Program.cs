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
using NSL.SocketCore.Utils.Logger;
using NSL.SocketServer.Utils;
using NSL.TCP.Client;

namespace NSL.Extensions.Session.Example
{
    internal class Program
    {
        static NSLSessionInfo clientSession;

        static void ServerInit(IBasicLogger conLog)
        {
            NSLSessionManager<BaseServerNetworkClient> serverSessionManager;

            var server = TCPServerEndPointBuilder.Create()
                .WithClientProcessor<BaseServerNetworkClient>()
                .WithOptions()
                .WithBindingPoint(1555)
                .WithCode(b =>
                {
                    b.SetLogger(conLog);

                    b.AddDefaultEventHandlers(handleOptions: DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);

                    b.AddConnectHandle(c => c.InitializeObjectBag());

                    serverSessionManager = b.GetOptions()
                        .AddNSLSessions(c =>
                        {
                            c.CloseSessionDelay = TimeSpan.FromSeconds(9);

                            c.OnExpiredSession = (client, session) =>
                            {
                                if (session == null)
                                {
                                    conLog.AppendLog($"Expired session - disconnect without session");
                                    // expired without session
                                    return Task.CompletedTask;
                                }

                                conLog.AppendLog($"Expired session {session?.Session}");

                                return Task.CompletedTask;
                            };

                            c.OnRecoverySession = (client, session) =>
                            {
                                conLog.AppendLog($"Recovered session {session.Session}");
                                return Task.CompletedTask;
                            };

                            c.OnClientValidate = (client) =>
                            {
                                if (client.ObjectBag.Exists("EndSession"))
                                {
                                    conLog.AppendError("Client Validate - closed session");

                                    return Task.FromResult(false);
                                }

                                conLog.AppendInfo("Client Validate - normal session");

                                return Task.FromResult(true);
                            };
                        });

                    b.GetOptions().SetDefaultResponsePID();

                    b.AddPacketHandle(3, (c, d) =>
                    {
                        c.ObjectBag.Set("EndSession", true);
                    });

                    b.AddPacketHandle(2, (c, d) =>
                    {
                        var session = serverSessionManager.CreateSession(c);

                        conLog.AppendLog($"Client connected session {session.Session}");

                        var result = OutputPacketBuffer.Create(2);

                        session.WriteFullTo(result);

                        c.Send(result);
                    });
                })
                .Build();


            server.Run();

        }

        static TCPNetworkClient<BasicNetworkClient, ClientOptions<BasicNetworkClient>> client;

        static void ClientInit(IBasicLogger conLog)
        {
            client = TCPClientEndPointBuilder.Create()
               .WithClientProcessor<BasicNetworkClient>()
               .WithOptions()
               .WithEndPoint("127.0.0.1", 1555)
               .WithCode(b =>
               {
                   b.SetLogger(conLog);

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

        }

        private static async Task RequestSession()
        {
            //sigin

            client.SendEmpty(2);

            await Task.Delay(2_000);

            client.Disconnect();
        }

        private static async Task<NSLSessionInfo> RecoverySessionTest(NSLSessionInfo session, string testName, int delayConnectedMS, Func<Task> onConnectedAction = null)
        {
            client.Connect();

            Console.ForegroundColor = ConsoleColor.Blue;

            Console.WriteLine(testName);

            client.Data.SetNSLSessionInfo(session);

            var recovery = await client.Data.NSLSessionSendRequestAsync();

            var validInfo = recovery.SessionInfo;

            Console.WriteLine($"{JsonConvert.SerializeObject(recovery)}");

            if (onConnectedAction != null)
                await onConnectedAction();

            await Task.Delay(delayConnectedMS);

            client.Disconnect();

            return validInfo;
        }


        static async Task TestBase()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine("Test base");

            await RequestSession();

            await Task.Delay(1_000);

            var validSession = await RecoverySessionTest(clientSession, "Success recovery", 9_500);

            await Task.Delay(1_000);

            await RecoverySessionTest(clientSession, "Invalid session", 1_000);

            await Task.Delay(1_000);

            validSession = await RecoverySessionTest(validSession, "Valid session", 1_000);

            await Task.Delay(9_500);

            await RecoverySessionTest(validSession, "Expired session", 1_000);
        }

        static async Task TestNormalClose()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine("Test close request");

            await RequestSession();

            await Task.Delay(1_000);

            var validSession = await RecoverySessionTest(clientSession, "Success recovery", 9_500, async () =>
            {
                await Task.Delay(1_000);

                client.SendEmpty(3);
            });


            await Task.Delay(1_000);

            await RecoverySessionTest(validSession, "Closed session recovery", 9_500);

            await Task.Delay(1_000);
        }


        static async Task Main(string[] args)
        {
            var conLog = new ConsoleLogger();

            var sl = new PrefixableLoggerProxy(conLog, "[Server]");

            ServerInit(sl);

            var cl = new PrefixableLoggerProxy(conLog, "[Client]");

            ClientInit(cl);

            if (true)
                await TestBase();
            else
                await TestNormalClose();


            Console.ReadKey();
        }
    }
}