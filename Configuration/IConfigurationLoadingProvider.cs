namespace ConfigurationEngine
{
    public interface IConfigurationLoadingProvider
    {
        bool LoadData(IConfigurationManager manager);
    }
}
