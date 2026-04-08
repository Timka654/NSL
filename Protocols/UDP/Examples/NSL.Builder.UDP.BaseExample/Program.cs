using NSL.Builder.UDP.BaseExample.Client;
using NSL.Builder.UDP.BaseExample.Server;

namespace NSL.Builder.UDP.BaseExample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Scenario 1: connect when server is already up ===");
            await TestUDPServer.RunServer();
            await TestUDPClient.RunClient();
            bool ok = await TestUDPClient.TryConnect(timeoutMs: 3000);
            Console.WriteLine($"[Scenario 1] ConnectAsync returned: {ok}");

            if (ok)
            {
                Console.WriteLine("[Scenario 1] Sending test packet...");
                await TestUDPClient.RunTest();
            }

            Console.WriteLine();
            Console.WriteLine("=== Scenario 2: connect when server is NOT yet running ===");
            await TestUDPClient.RebuildClient();
            TestUDPServer.Stop();

            // First attempt — server is down, expect false
            bool attemptWithoutServer = await TestUDPClient.TryConnect(timeoutMs: 2000);
            Console.WriteLine($"[Scenario 2] Attempt without server: {attemptWithoutServer} (expected false)");

            // Start server, reconnect
            await TestUDPServer.RunServer();
            bool reconnected = false;
            for (int i = 0; i < 5 && !reconnected; i++)
            {
                reconnected = await TestUDPClient.TryConnect(timeoutMs: 3000);
                Console.WriteLine($"[Scenario 2] Reconnect attempt {i + 1}: {reconnected}");
                if (!reconnected) await Task.Delay(500);
            }

            Console.WriteLine();
            Console.WriteLine("=== Scenario 3: server restarts while client is connected ===");
            if (reconnected)
            {
                Console.WriteLine("[Scenario 3] Stopping server...");
                TestUDPServer.Stop();
                await Task.Delay(4000); // wait for AliveCheckTimeout (3s default) + margin

                Console.WriteLine("[Scenario 3] Starting server...");
                await TestUDPServer.RunServer();

                bool afterRestart = false;
                for (int i = 0; i < 5 && !afterRestart; i++)
                {
                    afterRestart = await TestUDPClient.TryConnect(timeoutMs: 3000);
                    Console.WriteLine($"[Scenario 3] Reconnect after restart attempt {i + 1}: {afterRestart}");
                    if (!afterRestart) await Task.Delay(500);
                }
            }

            TestUDPServer.Stop();
            Console.WriteLine("\nDone. Press any key to exit.");
            Console.ReadKey();
        }
    }
}