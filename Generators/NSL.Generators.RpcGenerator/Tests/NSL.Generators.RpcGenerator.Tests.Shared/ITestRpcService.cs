using NSL.Generators.RpcGenerator.Shared.Attributes;
using System.Threading.Tasks;

namespace NSL.Generators.RpcGenerator.Tests.Shared
{
    /// <summary>
    /// Contract demonstrating all RPC generation scenarios:
    ///
    ///  Pid 1  — fire-and-forget: client sends and forgets; server handles, no response
    ///  Pid 2  — request-response, no return value: client awaits server completion (Task)
    ///  Pid 3  — request-response, with return value: client awaits Task&lt;PlayerInfo&gt;
    ///  Pid 4  — request-response with exception forwarding (method-level [NSLRPCExceptionHandler])
    ///  Pid 5  — multiple params, Task&lt;int&gt;
    /// </summary>
    [NSLRPCContainer]
    public interface ITestRpcService
    {
        /// <summary>Scenario 1: Fire-and-forget (client sends; server processes with no reply).</summary>
        [NSLRPCMethod(1)]
        [NSLRPCFireAndForget]
        Task SendNotification(string message);

        /// <summary>Scenario 2: Default Task — awaits server execution (empty response ACK).</summary>
        [NSLRPCMethod(2)]
        Task Ping();

        /// <summary>Scenario 3: Task&lt;T&gt; — awaits value returned by server.</summary>
        [NSLRPCMethod(3)]
        Task<PlayerInfo> GetPlayer(int playerId);

        /// <summary>Scenario 4: Exception serialization — [NSLRPCExceptionHandler] on method.</summary>
        [NSLRPCMethod(4)]
        [NSLRPCExceptionHandler]
        Task<PlayerInfo> GetPlayerSafe(int playerId);

        /// <summary>Scenario 5: Multiple params, primitive return type.</summary>
        [NSLRPCMethod(5)]
        Task<int> AddScore(int playerId, int delta);
    }

    /// <summary>
    /// Second interface with [NSLRPCExceptionHandler] at interface level
    /// (all methods inherit exception forwarding).
    /// </summary>
    [NSLRPCContainer]
    [NSLRPCExceptionHandler]
    public interface ITestChatService
    {
        /// <summary>Scenario 6: FAF inside exception-handler interface (ExceptionHandler is irrelevant for FAF).</summary>
        [NSLRPCMethod(10)]
        [NSLRPCFireAndForget]
        Task BroadcastMessage(string text);

        /// <summary>Scenario 7: Interface-level exception forwarding, Task&lt;T&gt;.</summary>
        [NSLRPCMethod(11)]
        Task<ChatMessage> GetLastMessage(string channel);
    }
}
