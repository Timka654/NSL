using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSL.ASPNET.Attributes;
using NSL.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.ASPNET.Services
{
    public static class NSLRootTimerServiceExtensions
    {
        public static IServiceCollection RegisterRootTimerService(this IServiceCollection serviceProvider, TimeSpan tickSpan)
        {
            serviceProvider.AddSingleton<NSLRootTimerService>(x => new NSLRootTimerService(x.GetRequiredService<ILoggerFactory>().CreateLogger<NSLRootTimerService>(), tickSpan));
            serviceProvider.AddHostedService(x => x.GetRequiredService<NSLRootTimerService>());
            return serviceProvider;
        }
    }

    public class NSLRootTimerService(ILogger<NSLRootTimerService> logger, TimeSpan ts) : IHostedService
    {
        public delegate Task TickActionDelegate(long iter, TimeSpan iterTime, CancellationToken cancellationToken);

        public event TickActionDelegate Tick = (iter, iterTime, ct) => Task.CompletedTask;

        Timer t;

        CancellationTokenSource cts;

        long iter = 0;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            cts = new CancellationTokenSource();

            var token = cts.Token;

            t = new Timer(async (e) =>
            {
                await Tick.InvokeAsync(async c =>
                {
                    try
                    {
                        await c(++iter, ts, cts.Token);
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"AccessCleanTimerService error - {ex}");
                    }
                });
            }, null, ts, ts);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await cts.CancelAsync();
        }
    }

}
