using Microsoft.Extensions.Hosting;
using System.IO;
using System.Text.Json.Nodes;
using System.Text.Json;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace NSL.ASPNET.Develop.Services
{
    public class LaunchUrlReceiveService(ILogger<LaunchUrlReceiveService> logger, string path, bool skipExceptions) : IHostedService
    {
        Channel<string> updateChannel = Channel.CreateUnbounded<string>();

        async void Process()
        {
            var reader = updateChannel.Reader;

            while (true)
            {
                string latestItem = null;

                await foreach (var item in reader.ReadAllAsync())
                {
                    latestItem = item;
                }

                await SetLaunchUrlForRememberedProfiles(latestItem);
            }
        }

        public async Task SetLaunchUrl(string url, CancellationToken cancellationToken)
        {
            await updateChannel.Writer.WriteAsync(url, cancellationToken);
        }

        private async Task SetLaunchUrlForRememberedProfiles(string url)
        {
            url = url.TrimStart('/');

            if (!File.Exists(path))
                return;

            int i = 0;
            var errors = new List<Exception>();

            while (i++ < 5)
            {
                try
                {
                    var json = File.ReadAllText(path);
                    var root = JsonNode.Parse(json)?.AsObject();
                    if (root is null || !root.ContainsKey("profiles"))
                        return;

                    var profiles = root["profiles"]?.AsObject();
                    if (profiles is null)
                        return;

                    var modified = false;

                    foreach (var profile in profiles)
                    {
                        var profileObj = profile.Value?.AsObject();
                        if (profileObj is null)
                            continue;

                        if (profileObj.TryGetPropertyValue("rememberLastUrl", out var rememberNode) &&
                            rememberNode is JsonValue rememberVal &&
                            rememberVal.TryGetValue(out bool remember) &&
                            remember)
                        {
                            profileObj["launchUrl"] = url;
                            modified = true;
                        }
                    }

                    if (modified)
                    {
                        File.WriteAllText(path, root.ToJsonString(new JsonSerializerOptions
                        {
                            WriteIndented = true
                        }));
                    }

                    break;
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }

                await Task.Delay(500);
            }

            if (!skipExceptions && i == 5)
            {
                throw new Exception(string.Join($"{Environment.NewLine}{Environment.NewLine}", errors));
            }
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Process();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            updateChannel.Writer.Complete();

            return Task.CompletedTask;
        }
    }
}
