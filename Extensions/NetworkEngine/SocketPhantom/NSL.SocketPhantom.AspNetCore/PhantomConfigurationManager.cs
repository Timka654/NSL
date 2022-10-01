using Microsoft.Extensions.Configuration;
using NSL.ConfigurationEngine;
using NSL.ConfigurationEngine.Info;
using System.Collections.Generic;

namespace NSL.SocketPhantom.AspNetCore
{
    internal class PhantomConfigurationManager : BaseConfigurationManager
    {
        public PhantomConfigurationManager(string path, IConfiguration configuration)
        {
            AddProvider(new ConfigurationEngine.Providers.IConfiguration.IConfigurationProvider() { Configuration = configuration });

            SetDefaults(new List<ConfigurationInfo>()
            {
                new ConfigurationInfo($"{path}.io.ip","0.0.0.0",string.Empty),
                new ConfigurationInfo($"{path}.io.port","9494",string.Empty),
                new ConfigurationInfo($"{path}.io.ipv","4",string.Empty),
                new ConfigurationInfo($"{path}.io.protocol","tcp",string.Empty),
                new ConfigurationInfo($"{path}.io.buffer.size","2048",string.Empty),
                new ConfigurationInfo($"{path}.io.backlog","100",string.Empty),
                new ConfigurationInfo($"{path}.security.type","RSA",string.Empty),
                new ConfigurationInfo($"{path}.security.rsa.keyPath","rsakey.key",string.Empty),
                new ConfigurationInfo($"{path}.security.rsa.private.keyPath","",string.Empty),
                new ConfigurationInfo($"{path}.security.rsa.publish.keyPath","",string.Empty),
            }, true);
        }
    }
}
