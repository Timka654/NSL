using NSL.ConfigurationEngine.Info;

namespace NSL.ConfigurationEngine.Providers
{
    public interface IConfigurationProvider
    {
        BaseConfigurationManager Manager { get; set; }

        bool Update(ConfigurationInfo configurationInfo, bool forceAdd = false);

        bool LoadData();

        bool LoadData(byte[] data);

        bool SaveData();
    }
}
