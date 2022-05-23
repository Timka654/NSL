using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using NSL.ConfigurationEngine.Info;
using System;

namespace NSL.ConfigurationEngine.Providers.IConfiguration
{
    public class IConfigurationProvider : BaseConfigurationProvider
    {
        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; set; }

        private IChangeToken changeToken;

        public override bool LoadData()
        {
            if (Configuration == null)
                return false;

            if (changeToken != Configuration.GetReloadToken())
            {
                Configuration.GetReloadToken().RegisterChangeCallback((obj) =>
                {
                    LoadData();
                }, null);

                changeToken = Configuration.GetReloadToken();
            }

            var configurationValues = Configuration.AsEnumerable();

            foreach (var item in configurationValues)
            {
                if (item.Value == null)
                    continue;

                Update(new ConfigurationInfo(item.Key.Replace(":", "."), item.Value, this, default), true);
            }

            return true;
        }

        public override bool LoadData(byte[] data)
        {
            return true;
        }

        public override bool SaveData()
        {
            return true;
        }
    }
}
