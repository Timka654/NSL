using NSL.ConfigurationEngine.Info;
using System.Collections.Generic;
using System.Linq;

namespace NSL.ConfigurationEngine.Providers
{
    public abstract class BaseConfigurationProvider : IConfigurationProvider
    {

        private List<ConfigurationInfo> ConfigurationList = new List<ConfigurationInfo>();

        public BaseConfigurationManager Manager { get; set; }

        public abstract bool LoadData();

        public abstract bool LoadData(byte[] data);

        public abstract bool SaveData();

        public virtual bool Update(ConfigurationInfo configurationInfo, bool forceAdd = false)
        {
            var ex = ConfigurationList.FirstOrDefault(x => x.Path == configurationInfo.Path);

            if (ex == null && !forceAdd)
                return false;
            else if (ex != null)
                ConfigurationList.Remove(ex);

            ConfigurationList.Add(Manager.AddValue(configurationInfo));

            return true;
        }
    }
}
