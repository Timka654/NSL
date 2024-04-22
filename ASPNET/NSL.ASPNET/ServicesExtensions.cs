using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.ASPNET
{
    public static class ServicesExtensions
    {
        public delegate Task InvokeInScopeAsyncDelegate(IServiceProvider provider);
        public delegate void InvokeInScopeDelegate(IServiceProvider provider);

        public static async Task InvokeInScopeAsync(this IServiceProvider provider, InvokeInScopeAsyncDelegate action)
        {
            await using var scope = provider.CreateAsyncScope();

            await action(scope.ServiceProvider);
        }

        public static void InvokeInScope(this IServiceProvider provider, InvokeInScopeDelegate action)
        {
            using var scope = provider.CreateScope();

            action(scope.ServiceProvider);
        }
    }
}
