using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;

namespace NSL.ConfigurationEngine.Providers.IConfiguration
{
    public class LoadingProvider : IConfigurationLoadingProvider
    {
        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; set; }
        private IChangeToken changeToken;

        public bool LoadData(IConfigurationManager manager)
        {
            if (Configuration == null)
                return false;

            if (changeToken != Configuration.GetReloadToken())
            {
                Configuration.GetReloadToken().RegisterChangeCallback((obj) =>
                {
                    LoadData(manager);
                }, null);

                changeToken = Configuration.GetReloadToken();
            }

            var configurationValues = Configuration.AsEnumerable();

            foreach (var item in configurationValues)
            {
                if (item.Value == null)
                    continue;

                manager.AddValue(item.Key.Replace(":", "."), item.Value);
            }

            return true;
        }

        public bool LoadData(IConfigurationManager manager, byte[] data)
        {
            throw new NotSupportedException();
        }

        public bool SaveData(IConfigurationManager manager)
        {
            throw new NotSupportedException();
        }
    }
}
