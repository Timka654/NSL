using NSL.Generators.RpcGenerator.Shared;
using NSL.Generators.RpcGenerator.Shared.Attributes;
using NSL.Generators.RpcGenerator.Tests.Shared;
using NSL.SocketServer.Utils;
using System;
using System.Threading.Tasks;

namespace NSL.Generators.RpcGenerator.Tests.Server
{
    [NSLRPCImplement(typeof(ITestRpcService), typeof(BaseServerNetworkClient), Direction = NSLRPCDirection.Server)]
    [NSLRPCImplement(typeof(ITestChatService),  typeof(BaseServerNetworkClient), Direction = NSLRPCDirection.Server)]
    public partial class TestRpcServer : ITestRpcService, ITestChatService
    {
        // ── ITestRpcService ──────────────────────────────────────────────────────

        /// <summary>Scenario 1: Fire-and-forget — just log and return, no response expected.</summary>
        public Task SendNotification(string message)
        {
            Console.WriteLine($"[Server] SendNotification: {message}");
            return Task.CompletedTask;
        }

        /// <summary>Scenario 2: Default Task — acknowledge by completing the task.</summary>
        public Task Ping()
        {
            Console.WriteLine("[Server] Ping");
            return Task.CompletedTask;
        }

        /// <summary>Scenario 3: Return a player by id.</summary>
        public Task<PlayerInfo> GetPlayer(int playerId)
        {
            return Task.FromResult(new PlayerInfo { Id = playerId, Name = $"Player#{playerId}", Score = playerId * 100 });
        }

        /// <summary>
        /// Scenario 4: Exception forwarding — throws so the client receives NSLRPCRemoteException.
        /// For id == 0 returns a valid result; otherwise throws to demonstrate forwarding.
        /// </summary>
        public Task<PlayerInfo> GetPlayerSafe(int playerId)
        {
            if (playerId == 0)
                return Task.FromResult(new PlayerInfo { Id = 0, Name = "DefaultPlayer", Score = 0 });

            throw new InvalidOperationException($"Player {playerId} does not exist");
        }

        /// <summary>Scenario 5: Multiple params, add score and return new total.</summary>
        public Task<int> AddScore(int playerId, int delta)
        {
            int newScore = playerId * 100 + delta;
            Console.WriteLine($"[Server] AddScore player={playerId} delta={delta} result={newScore}");
            return Task.FromResult(newScore);
        }

        // ── ITestChatService ─────────────────────────────────────────────────────

        /// <summary>Scenario 6: FAF inside exception-handler interface; ExceptionHandler is irrelevant for FAF.</summary>
        public Task BroadcastMessage(string text)
        {
            Console.WriteLine($"[Server] BroadcastMessage: {text}");
            return Task.CompletedTask;
        }

        /// <summary>Scenario 7: Interface-level exception forwarding, returns last message.</summary>
        public Task<ChatMessage> GetLastMessage(string channel)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));

            return Task.FromResult(new ChatMessage { Author = "system", Text = $"Last message in #{channel}", Channel = channel });
        }
    }
}
