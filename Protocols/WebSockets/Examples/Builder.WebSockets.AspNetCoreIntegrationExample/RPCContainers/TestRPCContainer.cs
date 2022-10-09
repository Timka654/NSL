using NSL.Extensions.RPC;
using NSL.Extensions.RPC.Generator.Attributes;
using NSL.SocketCore.Utils;
using NSL.WebSockets.Server.AspNetPoint;

namespace Builder.WebSockets.AspNetCoreIntegrationExample.RPCContainers
{
    public class TestRPCContainer<TClient> : RPCHandleContainer<TClient>
        where TClient : INetworkClient
    {
        [RPCMethod]
        public virtual int test(int value)
        {
            return (value * 2);
        }

        [RPCMethod]
        public virtual async void testasyncvoid(int value)
        {
            await Task.Delay(1000);
            Console.WriteLine($"{nameof(testasyncvoid)} - {value}");
        }

        [RPCMethod]
        public virtual async Task testasyncTask(int value)
        {
            await Task.Run(() =>
            {
                Console.WriteLine($"{nameof(testasyncTask)} - {value}");
            });
        }

        [RPCMethod]
        public virtual async Task<int> testasynctaskwithresult(int value)
        {
            return await Task.FromResult(value * 2);
        }
    }
}
