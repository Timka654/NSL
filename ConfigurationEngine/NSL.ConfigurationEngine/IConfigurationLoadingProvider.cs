namespace NSL.ConfigurationEngine
{
    public interface IConfigurationLoadingProvider
    {
        bool LoadData(IConfigurationManager manager);
        bool LoadData(IConfigurationManager manager, byte[] data);
        bool SaveData(IConfigurationManager manager);
    }
}
