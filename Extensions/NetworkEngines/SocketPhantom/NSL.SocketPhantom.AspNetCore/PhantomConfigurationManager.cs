using ConfigurationEngine;
using ConfigurationEngine.Providers.IConfiguration;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketPhantom.AspNetCore
{
    internal class PhantomConfigurationManager : IConfigurationManager
    {
        public PhantomConfigurationManager(string path, IConfiguration configuration) : base("")
        {
            Provider = new LoadingProvider() { Configuration = configuration };

            SetDefaults(new List<ConfigurationEngine.Info.ConfigurationInfo>()
            {
                new ConfigurationEngine.Info.ConfigurationInfo($"{path}.io.ip","0.0.0.0",string.Empty),
                new ConfigurationEngine.Info.ConfigurationInfo($"{path}.io.port","9494",string.Empty),
                new ConfigurationEngine.Info.ConfigurationInfo($"{path}.io.ipv","4",string.Empty),
                new ConfigurationEngine.Info.ConfigurationInfo($"{path}.io.protocol","tcp",string.Empty),
                new ConfigurationEngine.Info.ConfigurationInfo($"{path}.io.buffer.size","2048",string.Empty),
                new ConfigurationEngine.Info.ConfigurationInfo($"{path}.io.backlog","100",string.Empty),
                new ConfigurationEngine.Info.ConfigurationInfo($"{path}.security.type","RSA",string.Empty),
                new ConfigurationEngine.Info.ConfigurationInfo($"{path}.security.rsa.keyPath","rsakey.key",string.Empty),
                new ConfigurationEngine.Info.ConfigurationInfo($"{path}.security.rsa.private.keyPath","",string.Empty),
                new ConfigurationEngine.Info.ConfigurationInfo($"{path}.security.rsa.publish.keyPath","",string.Empty),
            }, true);
        }
    }
}
