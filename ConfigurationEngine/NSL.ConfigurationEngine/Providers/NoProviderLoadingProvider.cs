﻿namespace NSL.ConfigurationEngine.Providers
{
    public class NoProviderLoadingProvider : BaseConfigurationProvider
    {
        public override bool LoadData()
        {
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

        internal NoProviderLoadingProvider()
        {

        }
    }
}
