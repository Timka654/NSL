using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace NSL.ConfigurationEngine.Providers.IConfiguration
{
    public static class ConfigurationExtensions
    {
        public static Microsoft.Extensions.Configuration.IConfiguration BuildConfiguration(this BaseConfigurationManager conf)
        {
            var b = new ConfigurationBuilder();

            b.AddInMemoryCollection(conf.GetAllValues().Select(x => new KeyValuePair<string, string>(x.Path, x.Value)).AsEnumerable());

            return b.Build();
        }
    }
}
