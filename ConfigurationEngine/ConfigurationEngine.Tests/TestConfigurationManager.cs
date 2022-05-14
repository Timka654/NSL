using NSL.ConfigurationEngine;
using NSL.ConfigurationEngine.Info;
using System.Collections.Generic;

namespace ConfigurationEngine.Tests
{
    internal class TestConfigurationManager : IConfigurationManager
    {
        private static List<ConfigurationInfo> defaultCongiguration = new List<ConfigurationInfo>()
        {
            new ConfigurationInfo("testName1", "1", string.Empty),
            new ConfigurationInfo("testName2.testName3", "23", string.Empty),
            new ConfigurationInfo("testName4.testName5.testName6", "456", string.Empty),
        };

        public TestConfigurationManager(string fileName, IConfigurationLoadingProvider provider) : base(fileName)
        {
            Provider = provider;

        }

        public bool ReloadWithDefaults()
        {
            return SetDefaults(defaultCongiguration, true);
        }
    }
}
